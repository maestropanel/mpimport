namespace MpMigrate.Data.Dal
{
    using Microsoft.Web.Administration;
    using MpMigrate.Data.Entity;
    using MySql.Data.MySqlClient;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Windows.Forms;
    

    public class WebSitePanel_MsSQL : DboFactory
    {
        private string DEFAULT_WEBSITE_NAME = "websitepanel.info";
        private readonly string WEBSITEPANEL_WEBSITE_NAME = "WebsitePanel Enterprise Server";

        /// User role ID:
        /// 		Administrator = 1,
        /// 		Reseller = 2,
        /// 		User = 3

        /// User account status:
        /// Active = 1,
        /// Suspended = 2,
        /// Cancelled = 3,
        /// Pending = 4

        private bool EncryptionEnabled;
        private string CryptoKey;
        private string connectionString;

        public WebSitePanel_MsSQL()
        {
            
        }

        public override List<Domain> GetDomains()
        {
            SetCryptoKey();

            var _tmp = new List<Domain>();

            //WebsitePanel veritabanlarını herhangi bir yere bağlanamdığından dolayı. 
            //Havada duran tüm dataları buraya ekliyoruz.
            var _def = new Domain();
            _def.Name = DEFAULT_WEBSITE_NAME;
            _def.Status = 1;
            _def.ClientName = "admin";
            _def.isForwarding = false;
            _def.Username = _def.Name;
            _def.Password = DataHelper.GetPassword();    
            _def.Expiration = DateTime.Now.AddYears(1);
            _def.DomainPassword = _def.Password;      
            _def.Databases = GetDatabases(_def.Name);

            _tmp.Add(_def);

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT D.DomainID, D.PackageID, Users.UserID, Users.OwnerID, ISNULL(D.WebSiteID, 0) AS WebSiteID, 
			                                                    ISNULL(D.MailDomainID, 0) AS MailDomainID, D.DomainName,  Users.Username As Owner
                                                                FROM Domains AS D INNER JOIN
                                                                    Packages AS P ON D.PackageID = P.PackageID INNER JOIN
                                                        Users ON P.UserID = Users.UserID WHERE (D.IsDomainPointer = 0) AND (D.IsSubDomain = 0) AND (D.IsInstantAlias = 0)
                                                        AND D.DomainName IN (SELECT ServiceItems.ItemName
                                                        FROM ServiceItems INNER JOIN
                                                               ServiceItemTypes ON ServiceItems.ItemTypeID = ServiceItemTypes.ItemTypeID
                                                        WHERE (ServiceItemTypes.DisplayName = N'WebSite'))", _conn))
                {
                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new Domain();
                            _d.Id = DataExtensions.GetColumnValue<int>(_read, "DomainID");
                            _d.Name = DataExtensions.GetColumnValue<String>(_read, "DomainName").ToLower();
                            _d.ClientName = DataExtensions.GetColumnValue<String>(_read, "Owner");
                            _d.DomainPassword = DataHelper.GetPassword();
                            
                            _d.Username = _d.Name;
                            _d.Password = _d.DomainPassword;

                            _d.Status = 1;
                            _d.Expiration = DateTime.Now.AddYears(1);
                            _d.isForwarding = false;                            
                            _d.Limits = GetDomainLimits(_d.Name);
                            _d.Subdomains = GetSubdomains(_d.Name);                            
                            _d.Emails = GetEmails(_d.Name);
                            
                            // _d.Aliases aliaslar websitepanel'de bizdeki mantkta tutulmuyor.
                            // _d.Zone bu property'i eklemek lazım.   

                            //WebsitePanel forwarding'i sitenin web.config'inde tutuyor.
                            _d.isForwarding = false;
                            _d.ForwardUrl = "";                            

                            _tmp.Add(_d);
                        }
                    }
                }

                _conn.Close();
            }

            return _tmp;
        }

        public override List<Email> GetEmails(string domainName)
        {
            var _tmp = new List<Email>();
            var domainMailBoxSizeQuota = GetDomainEmailQuota(domainName);

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT ServiceItems.ItemID, ServiceItems.ItemName, ServiceItemTypes.DisplayName, ServiceItemProperties.PropertyValue, Packages.UserID
                                                    FROM ServiceItems INNER JOIN
                                                                          ServiceItemTypes ON ServiceItems.ItemTypeID = ServiceItemTypes.ItemTypeID INNER JOIN
                                                                          ServiceItemProperties ON ServiceItems.ItemID = ServiceItemProperties.ItemID INNER JOIN
                                                                          Packages ON ServiceItems.PackageID = Packages.PackageID
                                                    WHERE (ServiceItemTypes.DisplayName IN ('MailAccount')) AND 
                                                                          (ServiceItemProperties.PropertyName = N'Password') AND (ServiceItems.ItemName LIKE @NAME)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", String.Format("%@{0}",domainName));

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var password = DataExtensions.GetColumnValue<string>(_read, "PropertyValue");
                            var ItemName = DataExtensions.GetColumnValue<string>(_read, "ItemName");

                            var da = new Email();
                            da.DomainName = domainName;
                            da.Name = ItemName.Split('@').FirstOrDefault();
                            da.Password = Decrypt(password);
                            da.Quota = domainMailBoxSizeQuota;
                            da.Redirect = String.Empty;
                            da.RedirectedEmail = String.Empty;

                            _tmp.Add(da);
                        }
                    }
                }

                _conn.Close();
            }

            return _tmp;
        }

        private int GetDomainEmailQuota(string domainName)
        {            
            var mailboxsize = -1;

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT TOP 1 ISNULL(HostingPlanQuotas.QuotaValue,-1) AS NU 
                            FROM Domains INNER JOIN
                                                  Packages ON Domains.PackageID = Packages.PackageID INNER JOIN
                                                  HostingPlans ON Packages.PlanID = HostingPlans.PlanID INNER JOIN
                                                  HostingPlanQuotas ON HostingPlans.PlanID = HostingPlanQuotas.PlanID INNER JOIN
                                                  Quotas ON HostingPlanQuotas.QuotaID = Quotas.QuotaID
                            WHERE (Domains.DomainName = @NAME) AND (Domains.IsSubDomain = 0) AND (Domains.IsInstantAlias = 0) AND (Domains.IsDomainPointer = 0) AND (Quotas.QuotaName = 'Mail.MaxBoxSize')", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    var _quota = _cmd.ExecuteScalar();

                    if (_quota != null)
                        mailboxsize = Convert.ToInt32(_quota);
                }

                _conn.Close();
            }


            return mailboxsize;
        }

        public override HostLimit GetDomainLimits(string domainName)
        {
            var _tmp_limits = new List<LimitRow>();

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT Domains.DomainName, Packages.PackageName, HostingPlans.PlanName, HostingPlanQuotas.QuotaValue, Quotas.QuotaName, Domains.DomainID, Domains.IsSubDomain, 
                      Domains.IsInstantAlias, Domains.IsDomainPointer
                            FROM         Domains INNER JOIN
                                                  Packages ON Domains.PackageID = Packages.PackageID INNER JOIN
                                                  HostingPlans ON Packages.PlanID = HostingPlans.PlanID INNER JOIN
                                                  HostingPlanQuotas ON HostingPlans.PlanID = HostingPlanQuotas.PlanID INNER JOIN
                                                  Quotas ON HostingPlanQuotas.QuotaID = Quotas.QuotaID
                            WHERE     (Domains.DomainName = @NAME) AND (Domains.IsSubDomain = 0) AND (Domains.IsInstantAlias = 0) AND (Domains.IsDomainPointer = 0)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new LimitRow();

                            _d.Name = DataExtensions.GetColumnValue<string>(_read, "QuotaName");
                            _d.Value = DataExtensions.GetColumnValue<int>(_read, "QuotaValue");

                            _tmp_limits.Add(_d);
                        }
                    }
                }

                _conn.Close();
            }

            return new HostLimit(_tmp_limits, WebSitePanel:1);
        }

        public override DnsZone GetDnsZone(string domainName)
        {
            var _tmp = new DnsZone();
            _tmp.Name = domainName;

            return _tmp;
        }

        public override List<DnsZoneRecord> GetZoneRecords(string domainName)
        {
            return new List<DnsZoneRecord>();
        }

        public override Forwarding GetForwarding(string domainName)
        {
            return new Forwarding();
        }

        public override List<DomainAlias> GetDomainAliases(string domainName)
        {
            return new List<DomainAlias>();
        }

        public override List<Database> GetDatabases(string domainName)
        {
            var _tmp = new List<Database>();

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT ServiceItems.ItemID, ServiceItems.ItemName, ServiceItemTypes.DisplayName 
                                                            FROM ServiceItems INNER JOIN ServiceItemTypes ON ServiceItems.ItemTypeID = ServiceItemTypes.ItemTypeID 
                                                            WHERE (ServiceItemTypes.DisplayName IN 
                                            ('MsSQL2000Database', 'MySQL4Database', 'MsSQL2005Database', 'MySQL5Database', 'MsSQL2008Database', 'MsSQL2012Database'))", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var da = new Database();
                            da.Id = DataExtensions.GetColumnValue<int>(_read, "ItemID");
                            da.Name = DataExtensions.GetColumnValue<String>(_read, "ItemName");
                            da.Domain = domainName;
                            da.DbType = getDbType(DataExtensions.GetColumnValue<String>(_read, "DisplayName"));                            
                            da.Users = GetDatabaseUsers(da.Id);

                            _tmp.Add(da);
                        }
                    }
                }

                _conn.Close();
            }

            return _tmp;
        }

        private string getDbType(string displayName)
        {
            var _dbtype = "none";

            switch (displayName)
            {
                case "MySQL5Database":
                case "MySQL4Database":
                    _dbtype = "mysql";
                    break;
                case "MsSQL2000Database":
                case "MsSQL2005Database":
                case "MsSQL2008Database":
                case "MsSQL2012Database":
                case "MsSQL2014Database":
                    _dbtype = "mssql";
                    break;
                default:
                    break;
            }

            return _dbtype;
        }

        public override List<DatabaseUser> GetDatabaseUsers(int database_id)
        {
            var _tmp = new List<DatabaseUser>();

            //Veritabanı Adını bul
            var db = GetDatabaseItem(database_id);

            //Veritabanına ait kullanıcıların listesini mysql'den getir.
            var databaseUsers = db.dbtype == "mssql" ?
                GetDatabaseUsersFromMsSQL(db) :
                GetDatabaseUsersFromMySQL(db);

            //Kullanıcıyı websitepanel veritabanından bul.
            var websitePanelUserId = GetDatabaseOwnerFromWebSitePanel(db);
            var websitePanelDatabaseUsers = GetDatabaseUsersFromWebSitePanel(websitePanelUserId);

            //Database Kullanıcıları
            var currentUser = websitePanelDatabaseUsers.Where(m => m.UserType == db.dbtype && databaseUsers.Contains(m.Username)).FirstOrDefault();

            if (!String.IsNullOrEmpty(currentUser.Username))
            {                
                var dbuser = new DatabaseUser();
                dbuser.Username = currentUser.Username;
                dbuser.Password = Decrypt(currentUser.Password);                

                _tmp.Add(dbuser);
            }
            
            return _tmp;
        }
        
        private List<DataBaseUserItem> GetDatabaseUsersFromWebSitePanel(int websiteUserID)
        {
            var tmp = new List<DataBaseUserItem>();

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT ServiceItems.ItemID, ServiceItems.ItemName, ServiceItemTypes.DisplayName, ServiceItemProperties.PropertyValue, Packages.UserID
                                                    FROM ServiceItems INNER JOIN
                                                                          ServiceItemTypes ON ServiceItems.ItemTypeID = ServiceItemTypes.ItemTypeID INNER JOIN
                                                                          ServiceItemProperties ON ServiceItems.ItemID = ServiceItemProperties.ItemID INNER JOIN
                                                                          Packages ON ServiceItems.PackageID = Packages.PackageID
                                                    WHERE (ServiceItemTypes.DisplayName IN ('MsSQL2000User', 'MySQL4User', 'MsSQL2005User', 'MySQL5User', 'MsSQL2008User', 'MsSQL2012User')) AND 
                                                                          (ServiceItemProperties.PropertyName = N'Password') AND (Packages.UserID = @ID)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@ID", websiteUserID);

                    using (SqlDataReader _read = _cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                    {
                        while (_read.Read())
                        {
                            var uit = new DataBaseUserItem();
                            uit.Id = DataExtensions.GetColumnValue<int>(_read, "ItemID");
                            uit.Username = DataExtensions.GetColumnValue<string>(_read, "ItemName");
                            uit.Password = Decrypt(DataExtensions.GetColumnValue<string>(_read, "PropertyValue"));

                            var dbtype = DataExtensions.GetColumnValue<string>(_read, "DisplayName").StartsWith("MySQL") ? "mysql" : "mssql";
                            uit.UserType = dbtype;

                            tmp.Add(uit);
                        }
                    }
                }

                _conn.Close();
            }

            return tmp;
        }

        private int GetDatabaseOwnerFromWebSitePanel(DataBaseItem dbitem)
        {
            int websiteuserId = 0;

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT TOP 1 Packages.UserID
                                        FROM  ServiceItems INNER JOIN
                                        ServiceItemTypes ON ServiceItems.ItemTypeID = ServiceItemTypes.ItemTypeID INNER JOIN
                                        Packages ON ServiceItems.PackageID = Packages.PackageID
                                        WHERE     (ServiceItemTypes.DisplayName IN ('MsSQL2000Database', 'MySQL4Database', 'MsSQL2005Database', 'MySQL5Database', 'MsSQL2008Database', 'MsSQL2012Database'))  
                                        AND ServiceItems.ItemID = @ID", _conn))
                {
                    _cmd.Parameters.AddWithValue("@ID", dbitem.Id);

                    using (SqlDataReader _read = _cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                    {
                        if (_read.Read())
                            websiteuserId = DataExtensions.GetColumnValue<int>(_read, "UserID");
                    }
                }

                _conn.Close();
            }

            return websiteuserId;
        }

        private DataBaseItem GetDatabaseItem(int database_id)
        {
            var db = new DataBaseItem();
            db.Id = database_id;

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT TOP 1 ServiceItems.ItemID, ServiceItems.ItemName, ServiceItemTypes.DisplayName 
                                                            FROM ServiceItems INNER JOIN ServiceItemTypes ON ServiceItems.ItemTypeID = ServiceItemTypes.ItemTypeID 
                                                            WHERE (ServiceItemTypes.DisplayName IN ('MsSQL2000Database', 'MySQL4Database', 'MsSQL2005Database', 'MySQL5Database', 'MsSQL2008Database', 'MsSQL2012Database'))
                                                            AND ServiceItems.ItemID = @ID", _conn))
                {
                    _cmd.Parameters.AddWithValue("@ID", database_id);

                    using (SqlDataReader _read = _cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                    {
                        if (_read.Read())
                        {
                            db.Name = DataExtensions.GetColumnValue<String>(_read, "ItemName");
                            var displayName = DataExtensions.GetColumnValue<String>(_read, "DisplayName");

                            db.dbtype = displayName.StartsWith("MySQL") ? "mysql" : "mssql";                            
                        }
                    }
                }

                _conn.Close();
            }

            db.connectionStr = GetSQLConnectionString(db);

            return db;
        }

        private string GetSQLConnectionString(DataBaseItem dbitem)
        {
            string dbHost = String.Empty;
            string rootLogin = String.Empty;
            string rootPassword = String.Empty;
            string dbtype = "mssql";

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT ServiceItems.ItemID, ServiceItemTypes.DisplayName, Services.ServiceName, ServiceProperties.PropertyName, ServiceProperties.PropertyValue
                                    FROM ServiceItems INNER JOIN
                                    ServiceItemTypes ON ServiceItems.ItemTypeID = ServiceItemTypes.ItemTypeID INNER JOIN
                                    Services ON ServiceItems.ServiceID = Services.ServiceID INNER JOIN
                                    ServiceProperties ON Services.ServiceID = ServiceProperties.ServiceID
                                WHERE     (ServiceItemTypes.DisplayName IN ('MsSQL2000Database', 'MySQL4Database', 'MsSQL2005Database', 'MySQL5Database', 'MsSQL2008Database', 'MsSQL2012Database')) AND 
                      (ServiceItems.ItemID = @ID)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@ID", dbitem.Id);

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var serviceName = DataExtensions.GetColumnValue<String>(_read, "ServiceName");
                            var propertyName = DataExtensions.GetColumnValue<String>(_read, "PropertyName");
                            var propertyValue = DataExtensions.GetColumnValue<String>(_read, "PropertyValue");
                            
                            //MessageBox.Show(String.Format("ID: {3}, DBName: {4}, Service: {0}, Name: {1}, Value: {2}", serviceName, propertyName, propertyValue, dbitem.Id, dbitem.Name));

                            if (propertyName == "internaladdress")
                                dbHost = propertyValue;

                            if (propertyName == "rootlogin")
                                rootLogin = propertyValue;

                            if (propertyName == "rootpassword")
                                rootPassword = Decrypt(propertyValue);

                            if (propertyName == "salogin")
                                rootLogin = propertyValue;

                            if (propertyName == "sapassword")
                                rootPassword = Decrypt(propertyValue);

                            if (serviceName.StartsWith("MySQL"))
                                dbtype = "mysql";
                        }
                    }
                }

                _conn.Close();
            }

            //MessageBox.Show(String.Format("ID: {1}, Type: {0}, Name: {2}, Login: {3} Pass: {4}", dbitem, dbitem.Id, dbitem.Name, rootLogin, rootPassword));

            return createConnectionString(dbtype, dbHost, dbitem.Name, rootLogin, rootPassword);
        }

        private string createConnectionString(string dbtype, string host, string database, string username, string password)
        {
            var conn = String.Empty;

            var hostname = host.Split(',').FirstOrDefault();
            var port = host.Split(',').LastOrDefault();

            var msssql_conn = String.Format("Server={0};Database={1};User Id={2};Password={3};",
                host,
                "master",
                username,
                password);

            var mysql_conn = String.Format("Server={0};Port={1};Database={2};Uid={3};Pwd={4};",
                hostname,
                port,
                "mysql",
                username,
                password);

            return dbtype == "mssql" ? msssql_conn : mysql_conn;
        }

        private List<String> GetDatabaseUsersFromMySQL(DataBaseItem dbitem)
        {
            var list = new List<String>();

            var _con = GetSQLConnectionString(dbitem);

            using (MySqlConnection _conn = new MySqlConnection(_con))
            {
                _conn.Open();

                using (MySqlCommand _cmd = new MySqlCommand(String.Format("SELECT User FROM db WHERE Db='{0}' AND Host='%' AND Select_priv = 'Y' AND Insert_priv = 'Y' AND Update_priv = 'Y' AND Delete_priv = 'Y' AND Index_priv = 'Y' AND Alter_priv = 'Y' AND Create_priv = 'Y' AND Drop_priv = 'Y' AND Create_tmp_table_priv = 'Y' AND Lock_tables_priv = 'Y'",dbitem.Name), _conn))
                {                    
                    using (MySqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var user = DataExtensions.GetColumnValue<String>(_read, "User");
                            list.Add(user);
                        }
                    }
                }
                _conn.Close();
            }

            return list;
        }

        private List<String> GetDatabaseUsersFromMsSQL(DataBaseItem dbitem)
        {
            var list = new List<String>();

            var _con = GetSQLConnectionString(dbitem);

            //MessageBox.Show(_con);

            using (SqlConnection _conn = new SqlConnection(_con))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(String.Format(@"
				select su.name FROM [{0}]..sysusers as su
				inner JOIN master..syslogins as sl on su.sid = sl.sid
				where su.hasdbaccess = 1 AND su.islogin = 1 AND su.issqluser = 1 AND su.name <> 'dbo'",
                    dbitem.Name), _conn))
                {                    
                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var user = DataExtensions.GetColumnValue<String>(_read, "name");
                            list.Add(user);
                        }
                    }
                }

                _conn.Close();
            }

            return list;
        }
        
        public override List<Subdomain> GetSubdomains(string domainName)
        {
            var _tmp = new List<Subdomain>();
            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT DomainName, DomainID
                                                        FROM Domains
                                                        WHERE (IsSubDomain = 1) AND (DomainName LIKE @NAME)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", String.Format("%{0}",domainName));

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var da = new Subdomain();
                            da.Domain = domainName;
                            da.Login = domainName;
                            da.Name = DataExtensions.GetColumnValue<String>(_read, "DomainName");
                            da.Password = "";                            

                            _tmp.Add(da);
                        }
                    }
                }

                _conn.Close();
            }

            return _tmp;  
        }

        public override List<Reseller> GetResellers()
        {
            var _tmp = new List<Reseller>();
            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT UserID, OwnerID, RoleID, StatusID, Username, Password, IsPeer, IsDemo, FirstName, 
                                                        LastName, Email, Address, City, State, Country, Zip, PrimaryPhone, Fax, CompanyName
                                                            FROM Users
                                                        WHERE (IsDemo = 0) AND (RoleID IN (2,3)) AND (StatusID = 1)", _conn))
                {
                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var Password = DataExtensions.GetColumnValue<String>(_read, "Password");

                            var da = new Reseller();
                            da.Id = DataExtensions.GetColumnValue<int>(_read, "UserID");
                            da.Username = DataExtensions.GetColumnValue<String>(_read, "Username");
                            da.Password = Decrypt(Password);
                            da.Address1 = DataExtensions.GetColumnValue<String>(_read, "Address");                            
                            da.City = DataExtensions.GetColumnValue<String>(_read, "City");
                            da.Country = DataExtensions.GetColumnValue<String>(_read, "Country");
                            da.Email = DataExtensions.GetColumnValue<String>(_read, "Email");
                            da.fax = DataExtensions.GetColumnValue<String>(_read, "Fax");
                            da.FirstName = DataExtensions.GetColumnValue<String>(_read, "FirstName");
                            da.LastName = DataExtensions.GetColumnValue<String>(_read, "LastName");
                            da.Organization = DataExtensions.GetColumnValue<String>(_read, "CompanyName");
                            da.Phone = DataExtensions.GetColumnValue<String>(_read, "PrimaryPhone");
                            da.PostalCode = DataExtensions.GetColumnValue<String>(_read, "Zip");
                            da.Province = DataExtensions.GetColumnValue<String>(_read, "State");
                            da.Limits = ResellerLimits(da.Username);

                            _tmp.Add(da);
                        }
                    }
                }

                _conn.Close();
            }

            return _tmp; 
        }

        public override PanelStats GetPanelStats()
        {
            var pstats = new PanelStats();
            pstats.TotalDomainAliasCount = 0;

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT COUNT(*) FROM ServiceItems 
                                                            INNER JOIN ServiceItemTypes ON ServiceItems.ItemTypeID = ServiceItemTypes.ItemTypeID 
                                                                WHERE (ServiceItemTypes.DisplayName IN ('MsSQL2000Database', 'MySQL4Database', 'MsSQL2005Database', 'MySQL5Database', 'MsSQL2008Database', 'MsSQL2012Database'))", _conn))
                {
                    pstats.TotalDatabaseCount = GetScalarValue(_cmd.ExecuteScalar());
                }


                using (SqlCommand _cmd = new SqlCommand(@"SELECT
	                                                            RG.GroupID,
	                                                            RG.GroupName,
	                                                            ROUND(CONVERT(float, ISNULL(GD.Diskspace, 0)) / 1024 / 1024, 0) AS Diskspace,
	                                                            ISNULL(GD.Diskspace, 0) AS DiskspaceBytes
                                                            FROM ResourceGroups AS RG
                                                            LEFT OUTER JOIN
                                                            (
	                                                            SELECT
		                                                            PD.GroupID,
		                                                            SUM(ISNULL(PD.DiskSpace, 0)) AS Diskspace -- in megabytes
	                                                            FROM PackagesTreeCache AS PT
	                                                            INNER JOIN PackagesDiskspace AS PD ON PT.PackageID = PD.PackageID
	                                                            INNER JOIN Packages AS P ON PT.PackageID = P.PackageID
	                                                            INNER JOIN HostingPlanResources AS HPR ON PD.GroupID = HPR.GroupID
		                                                            AND HPR.PlanID = P.PlanID AND HPR.CalculateDiskspace = 1	
	                                                            GROUP BY PD.GroupID
                                                            ) AS GD ON RG.GroupID = GD.GroupID
                                                            WHERE GD.Diskspace <> 0
                                                            ORDER BY RG.GroupOrder", _conn))
                {

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var gname = DataExtensions.GetColumnValue<String>(_read, "GroupName");

                            if (gname == "MsSQL2008" || gname == "MySQL5")
                                pstats.TotalDatabaseDiskSpace += Convert.ToDecimal(DataExtensions.GetColumnValue<double>(_read, "Diskspace"));

                            if (gname == "Mail")
                                pstats.TotalEmailDiskSpace = Convert.ToDecimal(DataExtensions.GetColumnValue<double>(_read, "Diskspace"));

                            if (gname == "OS")
                                pstats.TotalDomainDiskSpace = Convert.ToDecimal(DataExtensions.GetColumnValue<double>(_read, "Diskspace"));
                        }
                    }
                }


                using (SqlCommand _cmd = new SqlCommand(@"SELECT COUNT(*) FROM  Domains WHERE (IsDomainPointer = 0) AND (IsInstantAlias = 0) AND (IsSubDomain = 0)", _conn))
                {
                    pstats.TotalDomainCount = GetScalarValue(_cmd.ExecuteScalar());
                }

                //EmailCount
                using (SqlCommand _cmd = new SqlCommand(@"SELECT COUNT(*)
                                                            FROM ServiceItems INNER JOIN ServiceItemTypes ON ServiceItems.ItemTypeID = ServiceItemTypes.ItemTypeID 
                                                            WHERE (ServiceItemTypes.DisplayName IN ('MailAccount'))", _conn))
                {
                    pstats.TotalEmailCount = GetScalarValue(_cmd.ExecuteScalar());
                }

                //Reseller Count
                using (SqlCommand _cmd = new SqlCommand(@"SELECT COUNT(*) FROM Users WHERE (IsDemo = 0) AND (RoleID IN (2,3)) AND (StatusID = 1)", _conn))
                {
                    pstats.TotalResellerCount = GetScalarValue(_cmd.ExecuteScalar());
                }

                //Subdomain
                using (SqlCommand _cmd = new SqlCommand(@"SELECT COUNT(*) FROM Domains WHERE (IsSubDomain = 1)", _conn))
                {
                    pstats.TotalSubdomainCount = GetScalarValue(_cmd.ExecuteScalar());
                }

                _conn.Close();
            }


            return pstats;
        }

        private int GetScalarValue(object obj)
        {
            int _value = 0;

            if (obj == null)
                _value = 0;
            else
                _value = obj == DBNull.Value ? 0 : Convert.ToInt32(obj.ToString());

            return _value;
        }

        public override HostLimit ResellerLimits(string clientName)
        {
            var _tmp_limits = new List<LimitRow>();
            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT Users.Username, Packages.PackageName, HostingPlans.PlanName, Quotas.QuotaName, HostingPlanQuotas.QuotaValue
                                                            FROM Users INNER JOIN
                                                          Packages ON Users.UserID = Packages.UserID INNER JOIN
                                                          HostingPlans ON Packages.PlanID = HostingPlans.PlanID INNER JOIN
                                                          HostingPlanQuotas ON HostingPlans.PlanID = HostingPlanQuotas.PlanID INNER JOIN
                                                          Quotas ON HostingPlanQuotas.QuotaID = Quotas.QuotaID  WHERE (Users.Username = @NAME)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", clientName);

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var da = new LimitRow();
                            da.Name = DataExtensions.GetColumnValue<String>(_read, "QuotaName");
                            da.Value = DataExtensions.GetColumnValue<int>(_read, "QuotaValue");

                            _tmp_limits.Add(da);
                        }
                    }
                }

                _conn.Close();
            }

            return new HostLimit(_tmp_limits, WebSitePanel:1);
        }

        public override void LoadConnectionString(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public override bool SecurePasswords()
        {            
            return EncryptionEnabled;
        }
        
        public string Encrypt(string InputText)
        {
            string Password = CryptoKey;

            if (!EncryptionEnabled)
                return InputText;

            if (InputText == null)
                return InputText;

            // We are now going to create an instance of the 
            // Rihndael class.
            RijndaelManaged RijndaelCipher = new RijndaelManaged();

            // First we need to turn the input strings into a byte array.
            byte[] PlainText = System.Text.Encoding.Unicode.GetBytes(InputText);


            // We are using salt to make it harder to guess our key
            // using a dictionary attack.
            byte[] Salt = Encoding.ASCII.GetBytes(Password.Length.ToString());


            // The (Secret Key) will be generated from the specified 
            // password and salt.
            PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(Password, Salt);


            // Create a encryptor from the existing SecretKey bytes.
            // We use 32 bytes for the secret key 
            // (the default Rijndael key length is 256 bit = 32 bytes) and
            // then 16 bytes for the IV (initialization vector),
            // (the default Rijndael IV length is 128 bit = 16 bytes)
            ICryptoTransform Encryptor = RijndaelCipher.CreateEncryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));


            // Create a MemoryStream that is going to hold the encrypted bytes 
            MemoryStream memoryStream = new MemoryStream();


            // Create a CryptoStream through which we are going to be processing our data. 
            // CryptoStreamMode.Write means that we are going to be writing data 
            // to the stream and the output will be written in the MemoryStream
            // we have provided. (always use write mode for encryption)
            CryptoStream cryptoStream = new CryptoStream(memoryStream, Encryptor, CryptoStreamMode.Write);

            // Start the encryption process.
            cryptoStream.Write(PlainText, 0, PlainText.Length);


            // Finish encrypting.
            cryptoStream.FlushFinalBlock();

            // Convert our encrypted data from a memoryStream into a byte array.
            byte[] CipherBytes = memoryStream.ToArray();



            // Close both streams.
            memoryStream.Close();
            cryptoStream.Close();



            // Convert encrypted data into a base64-encoded string.
            // A common mistake would be to use an Encoding class for that. 
            // It does not work, because not all byte values can be
            // represented by characters. We are going to be using Base64 encoding
            // That is designed exactly for what we are trying to do. 
            string EncryptedData = Convert.ToBase64String(CipherBytes);



            // Return encrypted string.
            return EncryptedData;
        }

        public string Decrypt(string InputText)
        {
            try
            {
                if (!EncryptionEnabled)
                    return InputText;

                if (InputText == null || InputText == "")
                    return InputText;

                string Password = CryptoKey;
                RijndaelManaged RijndaelCipher = new RijndaelManaged();


                byte[] EncryptedData = Convert.FromBase64String(InputText);
                byte[] Salt = Encoding.ASCII.GetBytes(Password.Length.ToString());


                PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(Password, Salt);

                // Create a decryptor from the existing SecretKey bytes.
                ICryptoTransform Decryptor = RijndaelCipher.CreateDecryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));


                MemoryStream memoryStream = new MemoryStream(EncryptedData);

                // Create a CryptoStream. (always use Read mode for decryption).
                CryptoStream cryptoStream = new CryptoStream(memoryStream, Decryptor, CryptoStreamMode.Read);


                // Since at this point we don't know what the size of decrypted data
                // will be, allocate the buffer long enough to hold EncryptedData;
                // DecryptedData is never longer than EncryptedData.
                byte[] PlainText = new byte[EncryptedData.Length];

                // Start decrypting.
                int DecryptedCount = cryptoStream.Read(PlainText, 0, PlainText.Length);


                memoryStream.Close();
                cryptoStream.Close();

                // Convert decrypted data into a string. 
                string DecryptedData = Encoding.Unicode.GetString(PlainText, 0, DecryptedCount);


                // Return decrypted string.   
                return DecryptedData;
            }
            catch
            {
                return "";
            }
        }

        private string SHA1(string plainText)
        {
            // Convert plain text into a byte array.
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            HashAlgorithm hash = new SHA1Managed(); ;

            // Compute hash value of our plain text with appended salt.
            byte[] hashBytes = hash.ComputeHash(plainTextBytes);

            // Return the result.
            return Convert.ToBase64String(hashBytes);
        }
        
        private string GetPhysicalPathByDomainName(string siteName)
        {
            var _physicalPath = "";

            using (ServerManager _server = new ServerManager())
            {                
                    var _site = _server.Sites[siteName];
                    var _siteApp = _site.Applications.Where(m => m.Path.Equals("/")).FirstOrDefault();
                    var _vdir = _siteApp.VirtualDirectories.Where(m => m.Path == "/").FirstOrDefault();

                    if (_vdir != null)
                        _physicalPath = _vdir.PhysicalPath;                
            }

            return _physicalPath;
        }

        private bool isWebSiteExists()
        {
            using (ServerManager _server = new ServerManager())
            {
                return _server.Sites.Where(m => m.Name == WEBSITEPANEL_WEBSITE_NAME).Any();
            }
        }

        public void SetCryptoKey()
        {
            //WebSitePanel Parolaları Endoce edilmiş mi?
            if (isWebSiteExists())
            {
                System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/", WEBSITEPANEL_WEBSITE_NAME);

                CryptoKey = rootWebConfig1.AppSettings.Settings["WebsitePanel.CryptoKey"].Value;
                EncryptionEnabled = ConfigurationManager.AppSettings["WebsitePanel.EncryptionEnabled"] != null ? Boolean.Parse(ConfigurationManager.AppSettings["WebsitePanel.EncryptionEnabled"]) : true;
            }
        }
    }

    public struct DataBaseItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string dbtype { get; set; }
        public string connectionStr { get; set; }
    }


    public struct DataBaseUserItem
    {
        public int Id { get; set; }
        public string DatabaseName { get; set; }
        public string Username { get; set; }
        public string UserType { get; set; }
        public string Password { get; set; }
    }

}
