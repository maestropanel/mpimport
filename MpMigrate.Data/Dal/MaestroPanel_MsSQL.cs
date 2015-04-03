namespace MpMigrate.Data.Dal
{
    using MpMigrate.Data.Entity;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;

    public class MaestroPanel_MsSQL : DboFactory
    {
        private string connectionString;

        public override List<Domain> GetDomains()
        {
            var _tmp = new List<Domain>();

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT Domain.DomainId, Domain.Name, Domain.CreationDate, Domain.Status, Domain.OwnerLoginId, Domain.UserLoginId, 
                                                            Domain.ExpirationDate, LoginAccount_1.UserName, LoginAccount_1.Password,
                                                               LoginAccount.UserName AS Owner FROM Domain LEFT OUTER JOIN
                                                              LoginAccount ON Domain.OwnerLoginId = LoginAccount.LoginId LEFT OUTER JOIN
                                                              LoginAccount AS LoginAccount_1 ON Domain.UserLoginId = LoginAccount_1.LoginId", _conn))
                {
                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new Domain();
                            _d.Id = DataExtensions.GetColumnValue<int>(_read, "DomainId");
                            _d.Name = DataExtensions.GetColumnValue<String>(_read, "Name").ToLower();
                            _d.ClientName = DataExtensions.GetColumnValue<String>(_read, "Owner");
                            _d.DomainPassword = DataExtensions.GetColumnValue<String>(_read, "Password");

                            var ftpAccount = GetFirstFtpAccount(_d.Id);

                            _d.Username = ftpAccount.Username;
                            _d.Password = ftpAccount.Password;
                            
                            _d.Status = DataExtensions.GetColumnValue<short>(_read, "Status");
                            _d.Expiration = DataExtensions.GetColumnValue<DateTime>(_read, "ExpirationDate");

                            _d.isForwarding = isForwarding(_d.Id);

                            if (_d.isForwarding)
                            {
                                var frw = GetForwarding(_d.Name);
                                _d.ForwardUrl = frw.ForwardUrl;
                            }

                            _d.Aliases = GetDomainAliases(_d.Name);
                            _d.Databases = GetDatabases(_d.Name);
                            _d.Limits = GetDomainLimits(_d.Name);
                            _d.Subdomains = GetSubdomains(_d.Name);
                            _d.Zone = GetDnsZone(_d.Name);
                            _d.Emails = GetEmails(_d.Name);

                            _tmp.Add(_d);
                        }
                    }
                }

                _conn.Close();
            }

            return _tmp;
        }

        private FtpAccount GetFirstFtpAccount(int domainId)
        {
            var account = new FtpAccount();
            account.Username = String.Empty;
            account.Password = String.Empty;

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT DomainMsFTPUser.Username, DomainMsFTPUser.Password, DomainMsFTP.DomainId
                                                    FROM  DomainMsFTP INNER JOIN DomainMsFTPUser ON DomainMsFTP.Id = DomainMsFTPUser.FtpId 
                                    WHERE (DomainMsFTP.DomainId = @ID)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@ID", domainId);

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        if (_read.Read())
                        {
                            account.Username = DataExtensions.GetColumnValue<String>(_read, "Username");
                            account.Password = DataExtensions.GetColumnValue<String>(_read, "Password");
                        }
                    }
                }

                _conn.Close();
            }

            return account;
        }

        private bool isForwarding(int domainId)
        {
            var forwarding = false;

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT COUNT(*) FROM DomainRedirection WHERE DomainId = @ID AND Enabled = 1", _conn))
                {
                    _cmd.Parameters.AddWithValue("@ID", domainId);
                    forwarding = ((int)_cmd.ExecuteScalar() > 0);                    
                }

                _conn.Close();
            }

            return forwarding;
        }

        public override List<Email> GetEmails(string domainName)
        {
            var _tmp = new List<Email>();

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT DomainPostofficeUser.Username, DomainPostofficeUser.Password, 
                                                                    DomainPostofficeUser.AutoResponderSubject, 
                                                                        DomainPostofficeUser.AutoResponderMessage, 
                                                                            DomainPostofficeUser.Quota, 
                                                                          DomainPostofficeUser.Status, 
                                                                        DomainPostofficeUser.RedirectionStatus, 
                                                                        DomainPostofficeUser.RedirectionCopyStatus, 
                                                                            DomainPostofficeUser.AutoResponderStatus, 
                                                                          DomainPostofficeUser.UsageValue, Domain.Name AS DomainName
                                                                FROM Domain INNER JOIN
                                                                          DomainPostoffice ON Domain.DomainId = DomainPostoffice.DomainId INNER JOIN
                                                                          DomainPostofficeUser ON DomainPostoffice.Id = DomainPostofficeUser.PostOfficeId
                                                                WHERE (Domain.Name = @NAME)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var da = new Email();
                            da.DomainName = domainName;
                            da.Name = DataExtensions.GetColumnValue<string>(_read, "Username");
                            da.Password = DataExtensions.GetColumnValue<string>(_read, "Password");
                            da.Quota = DataExtensions.GetColumnValue<long>(_read, "Quota");
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

        public override HostLimit GetDomainLimits(string domainName)
        {
            var _tmp_limits = new List<LimitRow>();

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT DomainLimit.LimitName, DomainLimit.LimitValue, 
                                                                DomainLimit.Usage, DomainLimit.isUnlimited, Domain.Name 
                                                             FROM  DomainLimit INNER JOIN Domain ON DomainLimit.DomainId = Domain.DomainId 
                                                WHERE (Domain.Name = @NAME)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new LimitRow();

                            _d.Name = DataExtensions.GetColumnValue<string>(_read, "LimitName");
                            _d.Value = DataExtensions.GetColumnValue<int>(_read, "LimitValue");

                            _tmp_limits.Add(_d);
                        }
                    }
                }

                _conn.Close();
            }

            return new HostLimit(_tmp_limits, true);
        }

        public override DnsZone GetDnsZone(string domainName)
        {
            var _tmp = new DnsZone();
            _tmp.Name = domainName;

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT  Domain.Name, DomainZone.Id, DomainZone.ZoneType, 
			                                                DomainZone.AllowZoneTransfers, 
				                                                DomainZone.SecondaryServers, DomainZone.SerialNumber, 
				                                                DomainZone.PrimaryServer, DomainZone.ResponsiblePerson, 
				                                                DomainZone.RefreshInterval, DomainZone.RetryInterval, 
				                                                DomainZone.Expires, DomainZone.TTL
                                                FROM DomainZone 
	                                                INNER JOIN Domain ON DomainZone.DomainId = Domain.DomainId WHERE (Domain.Name = @NAME)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (SqlDataReader _read = _cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                    {
                        if(_read.Read())
                        {
                            _tmp.Name = DataExtensions.GetColumnValue<string>(_read, "Name").ToLower();
                            _tmp.mininum =-1;
                            _tmp.refresh = DataExtensions.GetColumnValue<int>(_read, "RefreshInterval");
                            _tmp.retry = DataExtensions.GetColumnValue<int>(_read, "RetryInterval");
                            _tmp.expire = DataExtensions.GetColumnValue<int>(_read, "Expires");

                            var serial = DataExtensions.GetColumnValue<string>(_read, "SerialNumber");
                            _tmp.serial = Convert.ToInt32(serial);

                            _tmp.ttl = DataExtensions.GetColumnValue<int>(_read, "TTL");
                            _tmp.Email = DataExtensions.GetColumnValue<string>(_read, "ResponsiblePerson");
                        }
                    }
                }

                _conn.Close();
            }

            _tmp.Records = GetZoneRecords(domainName);

            return _tmp;
        }

        public override List<DnsZoneRecord> GetZoneRecords(string domainName)
        {
            var _tmp = new List<DnsZoneRecord>();

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT Domain.Name AS DomainName, DomainMsDns.RecordType, DomainMsDns.Name, 
                                                    DomainMsDns.Value, DomainMsDns.Priority 
                                                        FROM Domain 
                                                    INNER JOIN DomainMsDns ON Domain.DomainId = DomainMsDns.DomainId 
                                                        WHERE (Domain.Name = @NAME)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var da = new DnsZoneRecord();
                            da.name = DataExtensions.GetColumnValue<string>(_read, "Name");
                            da.type = DataExtensions.GetColumnValue<string>(_read, "RecordType");
                            da.value = DataExtensions.GetColumnValue<string>(_read, "Value");
                            da.priority = DataExtensions.GetColumnValue<int>(_read, "Priority");

                            _tmp.Add(da);
                        }
                    }
                }

                _conn.Close();
            }

            return _tmp;
        }

        public override Forwarding GetForwarding(string domainName)
        {
            var frw = new Forwarding();

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT DomainRedirection.Id, DomainRedirection.DomainId, DomainRedirection.Destination, DomainRedirection.Enabled, 
					                                                    DomainRedirection.childOnly, DomainRedirection.exactDestination, 
                                                            DomainRedirection.httpResponseStatus
                                                        FROM DomainRedirection INNER JOIN
                                                            Domain ON DomainRedirection.DomainId = Domain.DomainId 
                                            WHERE (Domain.Name = @NAME)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (SqlDataReader _read = _cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                    {
                        _read.Read();

                        frw.Name = domainName;
                        frw.ForwardUrl = DataExtensions.GetColumnValue<String>(_read, "Destination");
                    }
                }

                _conn.Close();
            }

            return frw;
        }

        public override List<DomainAlias> GetDomainAliases(string domainName)
        {
            var _tmp = new List<DomainAlias>();
            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT DomainAlias.Hostname FROM Domain 
                                                            INNER JOIN  DomainAlias ON Domain.DomainId = DomainAlias.DomainId  WHERE (Domain.Name = @NAME)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var da = new DomainAlias();
                            da.Alias = DataExtensions.GetColumnValue<String>(_read, "Hostname");
                            da.Domain = domainName;
                            da.Status = 1;

                            _tmp.Add(da);
                        }
                    }
                }

                _conn.Close();
            }

            return _tmp;
        }

        public override List<Database> GetDatabases(string domainName)
        {
            var _tmp = new List<Database>();
            
            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                #region MSSQL 2008
                using (SqlCommand _cmd = new SqlCommand(@"SELECT DomainMsSQL.Id, DomainMsSQL.Name, Domain.Name AS DomainName FROM Domain INNER JOIN DomainMsSQL ON Domain.DomainId = DomainMsSQL.DomainId  
                                                            WHERE (Domain.Name = @NAME)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var da = new Database();
                            da.Id = DataExtensions.GetColumnValue<int>(_read, "Id");                            
                            da.Name = DataExtensions.GetColumnValue<String>(_read, "Name");
                            da.Domain = domainName;
                            da.DbType = "mssql";

                            da.Users = GetDatabaseUsers(da.Id);
                            
                            _tmp.Add(da);
                        }
                    }
                }
                #endregion

                #region MSSQL 2012
                using (SqlCommand _cmd = new SqlCommand(@"SELECT DomainMsSQL2012.Id, DomainMsSQL2012.Name, DomainMsSQL2012.Id, Domain.Name AS DomainName FROM Domain 
                                                                INNER JOIN DomainMsSQL2012 ON Domain.DomainId = DomainMsSQL2012.DomainId WHERE (Domain.Name = @NAME)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var da = new Database();
                            da.Id = DataExtensions.GetColumnValue<int>(_read, "Id");
                            da.Name = DataExtensions.GetColumnValue<String>(_read, "Name");
                            da.Domain = domainName;
                            da.DbType = "mssql";

                            da.Users = GetDatabaseUsersMsSQL2012(da.Id);

                            _tmp.Add(da);
                        }
                    }
                }
                #endregion

                #region MYSQL
                using (SqlCommand _cmd = new SqlCommand(@"SELECT DomainMySQL.Id, DomainMySQL.Name, DomainMySQL.Id, Domain.Name AS DomainName 
                                                            FROM DomainMySQL INNER JOIN Domain ON DomainMySQL.DomainId = Domain.DomainId WHERE (Domain.Name = @NAME)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var da = new Database();
                            da.Id = DataExtensions.GetColumnValue<int>(_read, "Id");
                            da.Name = DataExtensions.GetColumnValue<String>(_read, "Name");
                            da.Domain = domainName;
                            da.DbType = "mysql";
                            da.Users = GetDatabaseUsersMySQL(da.Id);

                            _tmp.Add(da);
                        }
                    }
                }
                #endregion

                _conn.Close();
            }

            return _tmp;
        }

        public override List<DatabaseUser> GetDatabaseUsers(int database_id)
        {
            var _tmp = new List<DatabaseUser>();
            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT Password, Username FROM DomainMsSQLUser WHERE (DatabaseId = @ID)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@ID", database_id);

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var da = new DatabaseUser();
                            da.Password = DataExtensions.GetColumnValue<String>(_read, "Password");
                            da.Username = DataExtensions.GetColumnValue<String>(_read, "Username");
                            
                            _tmp.Add(da);
                        }
                    }
                }

                _conn.Close();
            }

            return _tmp;
        }

        public List<DatabaseUser> GetDatabaseUsersMsSQL2012(int database_id)
        {
            var _tmp = new List<DatabaseUser>();
            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT Username, Password, DatabaseId FROM DomainMsSQL2012User WHERE (DatabaseId = @ID)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@ID", database_id);

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var da = new DatabaseUser();
                            da.Password = DataExtensions.GetColumnValue<String>(_read, "Password");
                            da.Username = DataExtensions.GetColumnValue<String>(_read, "Username");

                            _tmp.Add(da);
                        }
                    }
                }

                _conn.Close();
            }

            return _tmp;
        }

        public List<DatabaseUser> GetDatabaseUsersMySQL(int database_id)
        {
            var _tmp = new List<DatabaseUser>();
            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT DomainMySQLUser.Username, DomainMySQLUser.Password, DomainMySQL.Id 
                                                            FROM DomainMySQL 
                                                            INNER JOIN DomainMySQLUser ON DomainMySQL.Id = DomainMySQLUser.DatabaseId 
                                                    WHERE (DomainMySQL.Id = @ID)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@ID", database_id);

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var da = new DatabaseUser();
                            da.Password = DataExtensions.GetColumnValue<String>(_read, "Password");
                            da.Username = DataExtensions.GetColumnValue<String>(_read, "Username");
                            
                            _tmp.Add(da);
                        }
                    }
                }

                _conn.Close();
            }

            return _tmp;
        }

        public override List<Subdomain> GetSubdomains(string domainName)
        {
            var _tmp = new List<Subdomain>();
            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT SubDomain.Name, SubDomain.Username, Domain.Name AS DomainName 
                                                                FROM Domain INNER JOIN SubDomain ON Domain.DomainId = SubDomain.DomainId 
                                                        WHERE (Domain.Name = @NAME)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var da = new Subdomain();
                            da.Domain = domainName;
                            da.Login = DataExtensions.GetColumnValue<String>(_read, "Username");
                            da.Name = DataExtensions.GetColumnValue<String>(_read, "Name");
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

                using (SqlCommand _cmd = new SqlCommand(@"SELECT LoginAccount.LoginId, LoginAccount.UserName, LoginAccount.Password, LoginAccount.UserType, 
                                                            LoginAccount.ProfileId, LoginAccount.ExpirationDate, LoginAccount.ApiKey, LoginAccount.Status, 
                                                            LoginAccount.ApiAccess, Profile.FirstName, Profile.LastName, Profile.Email, Profile.Country, 
                                                            Profile.Organization, Profile.Address1, 
                                                                        Profile.Address2, Profile.City, 
                                                                        Profile.Province, Profile.PostalCode,
                                                                                       Profile.Phone, Profile.Fax
                                                                FROM         LoginAccount LEFT OUTER JOIN
                                                                                      Profile ON LoginAccount.ProfileId = Profile.ProfileId
                                                                WHERE     (LoginAccount.UserType = 1)", _conn))
                {
                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var da = new Reseller();
                            da.Id = DataExtensions.GetColumnValue<int>(_read, "LoginId");
                            da.Username = DataExtensions.GetColumnValue<String>(_read, "UserName");
                            da.Password = DataExtensions.GetColumnValue<String>(_read, "Password");
                            da.Address1 = DataExtensions.GetColumnValue<String>(_read, "Address1");
                            da.Address2 = DataExtensions.GetColumnValue<String>(_read, "Address2");
                            da.City = DataExtensions.GetColumnValue<String>(_read, "City");
                            da.Country = DataExtensions.GetColumnValue<String>(_read, "Country");
                            da.Email = DataExtensions.GetColumnValue<String>(_read, "Email");
                            da.fax = DataExtensions.GetColumnValue<String>(_read, "Fax");
                            da.FirstName = DataExtensions.GetColumnValue<String>(_read, "FirstName");
                            da.LastName = DataExtensions.GetColumnValue<String>(_read, "LastName");
                            da.Organization = DataExtensions.GetColumnValue<String>(_read, "Organization");
                            da.Phone = DataExtensions.GetColumnValue<String>(_read, "Phone");
                            da.PostalCode = DataExtensions.GetColumnValue<String>(_read, "PostalCode");
                            da.Province = DataExtensions.GetColumnValue<String>(_read, "Province");
                            da.Limits = ResellerLimits(da.Username);                                                                                  
                            
                            _tmp.Add(da);
                        }
                    }
                }

                _conn.Close();
            }

            return _tmp;   
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

        public override PanelStats GetPanelStats()
        {
            var pstats = new PanelStats();            

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                #region total database count
                using (SqlCommand _cmd = new SqlCommand(@"SELECT SUM(DbSize) FROM DomainMsSQL", _conn))
                {
                    pstats.TotalDatabaseDiskSpace = GetScalarValue(_cmd.ExecuteScalar());
                }

                using (SqlCommand _cmd = new SqlCommand(@"SELECT SUM(DbSize) FROM DomainMsSQL2012", _conn))
                {
                    pstats.TotalDatabaseDiskSpace += GetScalarValue(_cmd.ExecuteScalar());
                }

                using (SqlCommand _cmd = new SqlCommand(@"SELECT SUM(DbSize) FROM DomainMySQL", _conn))
                {
                    pstats.TotalDatabaseDiskSpace += GetScalarValue(_cmd.ExecuteScalar());
                }
                #endregion

                #region total database size
                using (SqlCommand _cmd = new SqlCommand(@"SELECT COUNT(*) FROM DomainMsSQL", _conn))
                {
                    pstats.TotalDatabaseCount = GetScalarValue(_cmd.ExecuteScalar());
                }

                using (SqlCommand _cmd = new SqlCommand(@"SELECT COUNT(*) FROM DomainMySQL", _conn))
                {
                    pstats.TotalDatabaseCount += GetScalarValue(_cmd.ExecuteScalar());
                }

                using (SqlCommand _cmd = new SqlCommand(@"SELECT COUNT(*) FROM DomainMsSQL2012", _conn))
                {
                    pstats.TotalDatabaseCount += GetScalarValue(_cmd.ExecuteScalar());
                }
                #endregion


                using (SqlCommand _cmd = new SqlCommand(@"SELECT COUNT(*) From DomainAlias", _conn))
                {
                    pstats.TotalDomainAliasCount = GetScalarValue(_cmd.ExecuteScalar());
                }

                using (SqlCommand _cmd = new SqlCommand(@"SELECT COUNT(*) From Domain", _conn))
                {
                    pstats.TotalDomainCount = GetScalarValue(_cmd.ExecuteScalar());
                }

                using (SqlCommand _cmd = new SqlCommand(@"SELECT SUM(Usage) as WebSize FROM DomainLimit WHERE LimitName = 'MSFTP_FTPQUOTA'", _conn))
                {
                    pstats.TotalDomainDiskSpace = GetScalarValue(_cmd.ExecuteScalar());
                }

                using (SqlCommand _cmd = new SqlCommand(@"SELECT SUM(Usage) as MailBoxCount FROM DomainLimit WHERE LimitName IN ('MAILBOXCOUNT','ICEWARP_MAILBOX_ACCOUNT_LIMIT','SmarterMailUsers')", _conn))
                {                  
                    pstats.TotalEmailCount = GetScalarValue(_cmd.ExecuteScalar());
                }

                using (SqlCommand _cmd = new SqlCommand(@"SELECT SUM(Usage) as MailSize FROM DomainLimit WHERE LimitName IN ('MAILBOXSIZEQUOTA','ICEWARP_TOTAL_DISKQUOTA','SmarterMailDiskSpace')", _conn))
                {
                  
                    pstats.TotalEmailDiskSpace = GetScalarValue(_cmd.ExecuteScalar());
                }

                using (SqlCommand _cmd = new SqlCommand(@"SELECT COUNT(*) From LoginAccount WHERE UserType = 1", _conn))
                {
                  
                    pstats.TotalResellerCount = GetScalarValue(_cmd.ExecuteScalar());
                }

                using (SqlCommand _cmd = new SqlCommand(@"SELECT COUNT(*) From SubDomain", _conn))
                {
                  
                    pstats.TotalSubdomainCount = GetScalarValue(_cmd.ExecuteScalar());
                    pstats.TotalSubdomainDiskSpace = 0;
                }                

                _conn.Close();
            }

            
            return pstats;
        }

        public override HostLimit ResellerLimits(string clientName)
        {
            var _tmp_limits = new List<LimitRow>();
            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT    LoginLimit.Value, LoginLimit.LoginId, LoginLimit.LimitName, 
                                                        LoginLimit.isUnlimited, LoginLimit.Usage, 
                                                            LoginLimit.LoginLimitId, LoginAccount.UserName 
                                                        FROM LoginLimit INNER JOIN LoginAccount ON LoginLimit.LoginId = LoginAccount.LoginId 
                                                    WHERE (LoginAccount.UserName = @NAME)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", clientName);

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var da = new LimitRow();
                            da.Name = DataExtensions.GetColumnValue<String>(_read, "LimitName");
                            da.Value = DataExtensions.GetColumnValue<int>(_read, "Value");

                            _tmp_limits.Add(da);
                        }
                    }
                }

                _conn.Close();
            }

            return new HostLimit(_tmp_limits, true);
        }

        public override void LoadConnectionString(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public override bool SecurePasswords()
        {
            return false;
        }

        private class FtpAccount
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}

