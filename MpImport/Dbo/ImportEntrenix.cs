namespace PleskImport.Dbo
{
    using MailEnable.Administration;
    using Microsoft.Win32;
    using PleskImport.Entity;
    using PleskImport.Properties;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.OleDb;
    using System.IO;
    using System.Linq;

    public class ImportEntrenix : DboFactory
    {
        public override List<PleskImport.Domain> GetDomains()
        {
            var _tmp = new List<PleskImport.Domain>();

            using (OleDbConnection _conn = new OleDbConnection(Settings.Default.connectionString))
            {
                _conn.Open();
                using (OleDbCommand _cmd = new OleDbCommand(@"SELECT Host_ID, Reseller_Username, Domain_Name, [Password], 
                                                                                AcountDisabled FROM Host_Accounts", _conn))
                {
                    using (OleDbDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new PleskImport.Domain();
                            _d.Id = (int)_read["Host_ID"];
                            _d.Name = _read["Domain_Name"].ToString();
                            _d.Username = _read["Domain_Name"].ToString();
                            _d.Password = _read["Password"].ToString();
                            _d.ClientName = _read["Reseller_Username"].ToString();
                            _d.Status = 1; //AcountDisabled sürekli 0 geliyor.                                                        
                            _d.DomainPassword = _read["Password"].ToString();

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
            var _root_dir = GetMailRoot();            
            
            var domainNames =  System.IO.Directory.GetDirectories(_root_dir).Select(m => Path.GetFileName(m)).ToList();

            foreach (var domain_item in domainNames)
            {                
                    var user_path = Path.Combine(Path.Combine(_root_dir, domain_item), "MAILROOT");

                    foreach (var user_item in System.IO.Directory.GetDirectories(user_path).Select(m => Path.GetFileName(m)).ToList())
                    {
                        var mailenableMailBox = GetMailBox(domain_item, user_item);

                        var _d = new Email();
                        _d.Name = Path.GetFileName(user_item);
                        _d.DomainName = domain_item;
                        _d.Password = mailenableMailBox.Password;
                        _d.Redirect = "";
                        _d.RedirectedEmail = "";
                        _d.Quota = mailenableMailBox.MailBoxQuota;

                        _tmp.Add(_d);
                    }
           }
          
            return _tmp;
        }

        public override List<Database> GetDatabases(string domainName)
        {
            var _tmp = new List<Database>();

            using (OleDbConnection _conn = new OleDbConnection(Settings.Default.connectionString))
            {
                _conn.Open();
                using (OleDbCommand _cmd = new OleDbCommand(@"SELECT Host_ID, DomainName, DatabaseName, Username, [Password] FROM MySQL_Details WHERE (DomainName = ?)", _conn))
                {
                    _cmd.CommandType = CommandType.Text;
                    _cmd.Parameters.AddWithValue("NAME", domainName);

                    using (OleDbDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new Database();
                            _d.Id = (int)_read["Host_ID"];
                            _d.Name = _read["DatabaseName"].ToString();
                            _d.Domain = _read["DomainName"].ToString();
                            _d.DbType = "mysql";

                            var userList = new List<DatabaseUser>();
                            userList.Add(new DatabaseUser() { Username = _read["Username"].ToString(), Password = _read["Password"].ToString() });

                            _d.Users = userList;

                            _tmp.Add(_d);
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

            using (OleDbConnection _conn = new OleDbConnection(Settings.Default.connectionString))
            {
                _conn.Open();
                using (OleDbCommand _cmd = new OleDbCommand(@"SELECT Domain_Name, Sub_Domain, Host_ID, Reseller_Username FROM SubDomain_List 
                                                                WHERE (Domain_Name = ?)", _conn))
                {
                    _cmd.CommandType = CommandType.Text;
                    _cmd.Parameters.AddWithValue("NAME", domainName);

                    using (OleDbDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new Subdomain();
                            _d.Domain = _read["Domain_Name"].ToString();
                            _d.Login = _read["Domain_Name"].ToString();
                            _d.Password = "";
                            _d.Name = _read["Sub_Domain"].ToString();
                            _d.UserType = "";

                            _tmp.Add(_d);
                        }
                    }
                }

                _conn.Close();
            }

            return _tmp;
        }

        public override List<DomainAlias> GetDomainAliases(string domainName)
        {
            var _tmp = new List<DomainAlias>();

            using (OleDbConnection _conn = new OleDbConnection(Settings.Default.connectionString))
            {
                _conn.Open();
                using (OleDbCommand _cmd = new OleDbCommand(@"SELECT Host_Accounts.Domain_Name, Alias_Details.Alias_Domainname
                                                                        FROM (Host_Accounts INNER JOIN
                                                                                  Alias_Details ON Host_Accounts.Host_ID = Alias_Details.Host_ID)
                                                                    WHERE (Host_Accounts.Domain_Name = ?)", _conn))
                {
                    _cmd.CommandType = CommandType.Text;
                    _cmd.Parameters.AddWithValue("NAME", domainName);

                    using (OleDbDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new DomainAlias();
                            _d.Domain = _read["Domain_Name"].ToString();
                            _d.Alias = _read["Alias_Domainname"].ToString();

                            _tmp.Add(_d);
                        }
                    }
                }
                _conn.Close();
            }

            return _tmp;
        }

        private string GetMailRoot()
        {
            return GetRegistryKeyValue(@"SOFTWARE\Wow6432Node\Mail Enable\Mail Enable", "Mail Root");
        }

        private string GetRegistryKeyValue(string registryKey, string name)
        {
            var _value = "";
            var _key = RegistryKey
            .OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(registryKey);

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
            _login.UserName = String.Format("{0}@{1}",accountName, domainName);
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
    }


}
