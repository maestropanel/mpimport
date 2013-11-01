namespace MpMigrate.Data.Dal
{
    using MailEnable.Administration;
    using Microsoft.Win32;
    using MpMigrate.Data.Entity;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Odbc;
    using System.Data.OleDb;
    using System.IO;
    using System.Linq;

    public class Entrenix_Access : DboFactory
    {
        private string connectionString;

        public override List<MpMigrate.Data.Entity.Domain> GetDomains()
        {
            var tmp = new List<MpMigrate.Data.Entity.Domain>();

            using (OdbcConnection _conn = new OdbcConnection(connectionString))
            {
                _conn.Open();
                using (OdbcCommand _cmd = new OdbcCommand(@"SELECT Host_ID, Reseller_Username, Domain_Name, [Password], AcountDisabled, Limit_WebSpace, 
                                                                Limit_Traffic, Limit_Stats, Limit_MailAccounts, Limit_MySQL, Limit_DomainAlias, Active_Date, Limit_SubDomain
                                                FROM Host_Accounts", _conn))
                {
                    using (OdbcDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new MpMigrate.Data.Entity.Domain();
                            _d.Id = DataExtensions.GetColumnValue<int>(_read, "Host_ID");
                            _d.Name = DataExtensions.GetColumnValue<String>(_read, "Domain_Name").ToLower();
                            _d.ClientName = DataExtensions.GetColumnValue<String>(_read, "Reseller_Username");
                            _d.DomainPassword = DataExtensions.GetColumnValue<String>(_read, "Password");

                            _d.Username = DataExtensions.GetColumnValue<String>(_read, "Domain_Name");
                            _d.Password = DataExtensions.GetColumnValue<String>(_read, "Password");

                            if (String.IsNullOrEmpty(_d.Password))
                                _d.Password = DataHelper.GetPassword();

                            _d.Status = 1;                            
                            _d.Expiration = DateTime.Now.AddYears(1); //Expiration Date Entrenix'de tutulmuyor.                            
                            _d.isForwarding = false;

                            _d.Aliases = GetDomainAliases(_d.Name);
                            _d.Databases = GetDatabases(_d.Name);
                            _d.Limits = GetDomainLimits(_d.Name);
                            _d.Subdomains = GetSubdomains(_d.Name);
                            _d.Zone = GetDnsZone(_d.Name);
                            _d.Emails = GetEmails(_d.Name);

                            tmp.Add(_d);
                        }
                    }
                }
                _conn.Close();
            }

            return tmp;
        }

        public override List<Email> GetEmails(string domainName)
        {
            var _tmp = new List<Email>();
            var _root_dir = GetMailRoot();

            var domainNames = System.IO.Directory.GetDirectories(_root_dir).Select(m => Path.GetFileName(m)).ToList();
            var user_path = Path.Combine(Path.Combine(_root_dir, domainName), "MAILROOT");

            if (System.IO.Directory.Exists(user_path))
            {
                foreach (var user_item in System.IO.Directory.GetDirectories(user_path).Select(m => Path.GetFileName(m)).ToList())
                {
                    var mailenableMailBox = GetMailBox(domainName, user_item);

                    var _d = new Email();
                    _d.Name = Path.GetFileName(user_item);
                    _d.DomainName = domainName;
                    _d.Password = mailenableMailBox.Password;
                    _d.Redirect = "";
                    _d.RedirectedEmail = "";
                    _d.Quota = mailenableMailBox.MailBoxQuota;

                    _tmp.Add(_d);
                }
            }

            return _tmp;
        }

        private string GetMailRoot()
        {
            if(Environment.Is64BitOperatingSystem)
                return GetRegistryKeyValue(@"SOFTWARE\Wow6432Node\Mail Enable\Mail Enable", "Mail Root");
            else
                return GetRegistryKeyValue(@"SOFTWARE\Mail Enable\Mail Enable", "Mail Root");
        }

        private string GetRegistryKeyValue(string registryKey, string name)
        {
            var _value = "";
            var _key = RegistryKey
                .OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32)
                .OpenSubKey(registryKey);

            if (_key != null)
                _value = _key.GetValue(name, "").ToString();

            return _value;
        }

        private MailEnableEmailItem GetMailBox(string domainName, string accountName)
        {
            var mailbox = new MailEnableEmailItem();

            var _login = new Login();
            _login.Account = domainName;
            _login.LastAttempt = -1;
            _login.LastSuccessfulLogin = -1;
            _login.Password = "";
            _login.Rights = "";
            _login.Status = -1;
            _login.UserName = String.Format("{0}@{1}", accountName, domainName);
            _login.GetLogin();

            var _mbox = new Mailbox();
            _mbox.Postoffice = domainName;
            _mbox.MailboxName = accountName;

            mailbox.Password = _login.Password;
            mailbox.MailBoxQuota = (_mbox.GetQuota() / 1024);

            if (mailbox.MailBoxQuota == 0)
                mailbox.MailBoxQuota = -1;

            return mailbox;
        }

        private struct MailEnableEmailItem
        {
            public string Password { get; set; }
            public long MailBoxQuota { get; set; }
        }     

        public override HostLimit GetDomainLimits(string domainName)
        {
            var _tmp = new HostLimit();

            using (OdbcConnection _conn = new OdbcConnection(connectionString))
            {
                _conn.Open();
                using (OdbcCommand _cmd = new OdbcCommand(@"SELECT Limit_WebSpace, Limit_Traffic, Limit_Stats, Limit_FrontPage, Limit_MailAccounts, 
                                                                                    Limit_ODBCDSN, Limit_MySQL, Limit_DomainAlias, Limit_SubDomain, 
                                                                            Domain_Name FROM Host_Accounts WHERE (Domain_Name = ?)", _conn))
                {
                    _cmd.CommandType = CommandType.Text;
                    _cmd.Parameters.AddWithValue("NAME", domainName);

                    using (OdbcDataReader _read = _cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (_read.Read())
                        {
                            _tmp.DiskSpace = DataExtensions.GetColumnValue<int>(_read, "Limit_WebSpace");
                            _tmp.MaxDomainAlias = DataExtensions.GetColumnValue<int>(_read, "Limit_DomainAlias");
                            _tmp.MaxFtpTraffic = -1;
                            _tmp.MaxFtpUser = 1;
                            _tmp.MaxMailBox = DataExtensions.GetColumnValue<int>(_read, "Limit_MailAccounts");
                            _tmp.MaxMailTraffic = -1;
                            
                            _tmp.MaxMsSqlDb = 0;
                            _tmp.MaxMsSqlDbSpace = 0;
                            _tmp.MaxMsSqlDbUser = 0;
                            
                            _tmp.MaxMySqlDb = DataExtensions.GetColumnValue<int>(_read, "Limit_MySQL");
                            _tmp.MaxMySqlDbSpace = -1;
                            _tmp.MaxMySqlUser = DataExtensions.GetColumnValue<int>(_read, "Limit_MySQL");

                            _tmp.MaxSubDomain = DataExtensions.GetColumnValue<int>(_read, "Limit_SubDomain");
                            _tmp.MaxWebTraffic = -1;
                            _tmp.TotalMailBoxQuota = -1;
                            _tmp.Expiration = DateTime.Now.AddYears(1);

                        }
                    }
                }
                _conn.Close();
            }

            return _tmp;
        }

        public override DnsZone GetDnsZone(string domainName)
        {
            return new DnsZone() { Records = new List<DnsZoneRecord>() };
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
            var _tmp = new List<DomainAlias>();

            using (OdbcConnection _conn = new OdbcConnection(connectionString))
            {
                _conn.Open();
                using (OdbcCommand _cmd = new OdbcCommand(@"SELECT Host_Accounts.Domain_Name, Alias_Details.Alias_Domainname
                                                                        FROM (Host_Accounts INNER JOIN
                                                                                  Alias_Details ON Host_Accounts.Host_ID = Alias_Details.Host_ID)
                                                                    WHERE (Host_Accounts.Domain_Name = ?)", _conn))
                {
                    _cmd.CommandType = CommandType.Text;
                    _cmd.Parameters.AddWithValue("NAME", domainName);

                    using (OdbcDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new DomainAlias();
                            _d.Domain = DataExtensions.GetColumnValue<String>(_read, "Domain_Name");
                            _d.Alias = DataExtensions.GetColumnValue<String>(_read, "Alias_Domainname");

                            _tmp.Add(_d);
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

            using (OdbcConnection _conn = new OdbcConnection(connectionString))
            {
                _conn.Open();
                using (OdbcCommand _cmd = new OdbcCommand(@"SELECT DatabaseName, Username, [Password], Host_ID, DomainName 
                                                                    FROM MySQL_Details WHERE (DomainName = ?)", _conn))
                {
                    _cmd.CommandType = CommandType.Text;
                    _cmd.Parameters.AddWithValue("NAME", domainName);

                    using (OdbcDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new Database();
                            _d.Id = DataExtensions.GetColumnValue<int>(_read, "Host_ID");
                            _d.Name = DataExtensions.GetColumnValue<String>(_read, "DatabaseName");
                            _d.Domain = DataExtensions.GetColumnValue<String>(_read, "DomainName");
                            _d.DbType = "mysql";

                            //Her DB'ye tek user.
                            var userList = new List<DatabaseUser>();
                            userList.Add(new DatabaseUser() { 
                                    Username = DataExtensions.GetColumnValue<String>(_read, "Username"), 
                                    Password = DataExtensions.GetColumnValue<String>(_read, "Password") 
                                });
                            
                            _d.Users = userList;

                            _tmp.Add(_d);
                        }
                    }
                }
                _conn.Close();
            }

            return _tmp;
        }

        public override List<DatabaseUser> GetDatabaseUsers(int database_id)
        {
            return new List<DatabaseUser>();
        }

        public override List<Subdomain> GetSubdomains(string domainName)
        {
            var _tmp = new List<Subdomain>();

            using (OdbcConnection _conn = new OdbcConnection(connectionString))
            {
                _conn.Open();
                using (OdbcCommand _cmd = new OdbcCommand(@"SELECT Domain_Name, Sub_Domain, Host_ID, Reseller_Username FROM SubDomain_List 
                                                                WHERE (Domain_Name = ?)", _conn))
                {
                    _cmd.CommandType = CommandType.Text;
                    _cmd.Parameters.AddWithValue("NAME", domainName);

                    using (OdbcDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new Subdomain();
                            _d.Domain = DataExtensions.GetColumnValue<String>(_read, "Domain_Name");
                            _d.Login = DataExtensions.GetColumnValue<String>(_read, "Domain_Name");                            
                            _d.Name = DataExtensions.GetColumnValue<String>(_read, "Sub_Domain");
                            _d.UserType = "";
                            _d.Password = "";

                            _tmp.Add(_d);
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

            using (OdbcConnection _conn = new OdbcConnection(connectionString))
            {
                _conn.Open();
                using (OdbcCommand _cmd = new OdbcCommand(@"SELECT Username AS uname, [Password] AS pass, Name_Sirname, Company_Name, 
                        Contact_Name, EMail, Phone, Fax, WUrl, Msn, ICQ, 
                         Address, City, State, PostCode, Country, Account_Disable, 
                        Limit_Host, Limit_WebSpace, Limit_Traffic, Limit_Stats, Limit_FrontPage, Limit_MailAccounts, 
                         Limit_ODBCDSN, Limit_MySQL, Limit_DomainAlias, HostAccountsDisable, Limit_SubDomain
                                                        FROM Reseller_Accounts", _conn))
                {
                    using (OdbcDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new Reseller();
                            _d.Address1 = DataExtensions.GetColumnValue<String>(_read, "Address");
                            _d.City = DataExtensions.GetColumnValue<String>(_read, "City");
                            _d.Country = DataExtensions.GetColumnValue<String>(_read, "Country");
                            _d.Email = DataExtensions.GetColumnValue<String>(_read, "EMail");
                            _d.fax = DataExtensions.GetColumnValue<String>(_read, "Fax");
                            _d.FirstName = DataExtensions.GetColumnValue<String>(_read, "Name_Sirname");
                            _d.Organization = DataExtensions.GetColumnValue<String>(_read, "Company_Name");                            
                            _d.Phone = DataExtensions.GetColumnValue<String>(_read, "Phone");
                            _d.PostalCode = DataExtensions.GetColumnValue<String>(_read, "PostCode");
                            _d.Province = DataExtensions.GetColumnValue<String>(_read, "State");

                            _d.Password = DataExtensions.GetColumnValue<String>(_read, "pass");
                            _d.Username = DataExtensions.GetColumnValue<String>(_read, "uname");                            
                            
                            var limits = new HostLimit();
                            limits.DiskSpace = DataExtensions.GetColumnValue<int>(_read, "Limit_WebSpace");
                            limits.MaxDomain = DataExtensions.GetColumnValue<int>(_read, "Limit_Host");
                            limits.MaxDomainAlias = DataExtensions.GetColumnValue<int>(_read, "Limit_DomainAlias");
                            limits.MaxFtpTraffic = -1;
                            limits.MaxFtpUser = 1;
                            limits.MaxMailBox = DataExtensions.GetColumnValue<int>(_read, "Limit_MailAccounts");
                            limits.MaxMailTraffic = -1;
                            
                            limits.MaxMsSqlDb = 0;
                            limits.MaxMsSqlDbSpace = 0;
                            limits.MaxMsSqlDbUser = 0;

                            limits.MaxMySqlDb = DataExtensions.GetColumnValue<int>(_read, "Limit_MySQL");
                            limits.MaxMySqlDbSpace = -1;
                            limits.MaxMySqlUser = DataExtensions.GetColumnValue<int>(_read, "Limit_MySQL");

                            limits.MaxSubDomain = DataExtensions.GetColumnValue<int>(_read, "Limit_SubDomain");
                            limits.MaxWebTraffic = -1;
                            limits.TotalMailBoxQuota = -1;

                            _d.Limits = limits;                            

                            _tmp.Add(_d);

                        }
                    }
                }

                _conn.Close();
            }

            return _tmp;
        }

        public override PanelStats GetPanelStats()
        {
            var _tmp = new PanelStats();

            _tmp.TotalDatabaseCount = GetCountFromDatabase("SELECT COUNT(*) AS Cnt FROM MySQL_Details");
            _tmp.TotalDatabaseDiskSpace = 0;
            _tmp.TotalDomainAliasCount = GetCountFromDatabase("SELECT COUNT(*) AS Cnt FROM Alias_Details");
            _tmp.TotalDomainCount = GetCountFromDatabase("SELECT COUNT(*) AS Cnt FROM Host_Accounts");
            _tmp.TotalDomainDiskSpace = GetCountFromDatabase("SELECT ROUND(SUM(WebSpace_Usage)) AS Cnt FROM Host_Accounts");
            _tmp.TotalEmailCount = 0;
            _tmp.TotalEmailDiskSpace = 0;
            _tmp.TotalResellerCount = GetCountFromDatabase("SELECT COUNT(*) AS Cnt FROM Reseller_Accounts");
            _tmp.TotalSubdomainCount = GetCountFromDatabase("SELECT COUNT(*) AS Cnt FROM SubDomain_List");
            _tmp.TotalSubdomainDiskSpace = 0;

            return _tmp;
        }

        public override HostLimit ResellerLimits(string clientName)
        {
            return new HostLimit();
        }

        public override void LoadConnectionString(string connectionString)
        {            
            this.connectionString = connectionString;
        }

        public override bool SecurePasswords()
        {
            return false;
        }

        private int GetCountFromDatabase(string textQuery)
        {
            var _count = 0;
            object resultObj = null;

            try
            {            
                using (OdbcConnection _conn = new OdbcConnection(connectionString))
                {
                    _conn.Open();
                    using (OdbcCommand _cmd = new OdbcCommand(textQuery, _conn))
                    {
                        resultObj = _cmd.ExecuteScalar();
                        _count = resultObj == null ? 0 : Convert.ToInt32(resultObj);
                    }

                    _conn.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message  +" "+ resultObj.GetType().Name +" "+ textQuery, ex);
            }

            return _count;
        }
    }
}
