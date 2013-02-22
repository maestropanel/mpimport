namespace PleskImport
{
    using System;
    using System.Collections.Generic;
    using MySql.Data.MySqlClient;
    using PleskImport.Entity;
    using PleskImport.Properties;

    public class ImportMySQLPlesk10 : DboFactory
    {
        public override List<Domain> GetDomains()
        {
            var _tmp = new List<Domain>();

            using (MySqlConnection _conn = new MySqlConnection(Settings.Default.connectionString))
            {
                _conn.Open();

                using (MySqlCommand _cmd = new MySqlCommand(@"SELECT 
                                                                    domains.id, 
                                                                    domains.name, 
                                                                    domains.name as fp_adm, 
                                                                    accounts.password, 
                                                                    clients.login, 
                                                                    clients.passwd, 
                                                                    dom_level_usrs.passwd As DomainPass, 
                                                                    domains.status As Status, 
                                                                    limits.value as expiration
			                                            FROM domains 
                                                LEFT JOIN hosting ON hosting.dom_id = domains.id 
				                                LEFT JOIN sys_users ON hosting.sys_user_id = sys_users.id 
				                                LEFT JOIN accounts ON accounts.id = sys_users.account_id 
				                                LEFT JOIN clients ON clients.id = domains.cl_id 
				                                LEFT JOIN dom_level_usrs ON dom_level_usrs.dom_id = domains.id 
                                                LEFT JOIN limits ON limits.id = domains.limits_id AND limits.limit_name = 'expiration'
                                            WHERE 
                                                domains.htype = 'vrt_hst'", _conn))
                {
                    using (MySqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new Domain();
                            _d.Id = Convert.ToInt32(_read["id"]);
                            _d.Name = _read["name"].ToString();
                            _d.Username = _read["fp_adm"].ToString();
                            _d.Password = _read["password"].ToString();
                            _d.ClientName = _read["login"].ToString();                            
                            _d.Status = Convert.ToInt32(_read["Status"]);
                            
                            if (!_read.IsDBNull(6))
                                _d.DomainPassword = _read["DomainPass"].ToString();

                            if (!_read.IsDBNull(8))
                                _d.Expiration = _d.FromUnixTime(_read["expiration"].ToString());

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

            using (MySqlConnection _conn = new MySqlConnection(Settings.Default.connectionString))
            {
                _conn.Open();
                using (MySqlCommand _cmd = new MySqlCommand(@"SELECT     
	                                                        mail.mail_name, domains.name, 
	                                                        accounts.password, domains.status, 
	                                                        mail.redirect, mail.redir_addr, mail.mbox_quota
                                                        FROM domains 
	                                                        LEFT OUTER JOIN mail ON mail.dom_id = domains.id 
	                                                        LEFT OUTER JOIN accounts ON accounts.id = mail.account_id
                                                        WHERE     
                                                        (mail.mail_name <> '') AND (domains.name = @NAME)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (MySqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new Email();
                            _d.Name = _read["mail_name"].ToString();
                            _d.DomainName = _read["name"].ToString();
                            _d.Password = _read["password"].ToString();
                            _d.Redirect = _read["redirect"].ToString();
                            _d.RedirectedEmail = _read["redir_addr"].ToString();
                            _d.Quota = Convert.ToDouble(_read["mbox_quota"]);

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

            using (MySqlConnection _conn = new MySqlConnection(Settings.Default.connectionString))
            {
                _conn.Open();
                using (MySqlCommand _cmd = new MySqlCommand(@"SELECT data_bases.id as db_id, domains.name AS domain, data_bases.name, data_bases.type
                                                            FROM data_bases LEFT OUTER JOIN
                                                        domains ON domains.id = data_bases.dom_id LEFT OUTER JOIN
                                                        db_users ON db_users.id = data_bases.default_user_id
                                                    WHERE  (domains.name = @NAME)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (MySqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new Database();
                            _d.Id = Convert.ToInt32(_read["db_id"]);
                            _d.Name = _read["name"].ToString();
                            _d.Domain = _read["domain"].ToString();
                            _d.DbType = _read["type"].ToString();
                            _d.Users = GetDatabaseUsers(_d.Id);

                            _tmp.Add(_d);
                        }
                    }
                }
                _conn.Close();
            }

            return _tmp;
        }

        private List<DatabaseUser> GetDatabaseUsers(int databaseId)
        {
            var _tmp = new List<DatabaseUser>();

            using (MySqlConnection _conn = new MySqlConnection(Settings.Default.connectionString))
            {
                _conn.Open();
                using (MySqlCommand _cmd = new MySqlCommand(@"SELECT login, passwd FROM  db_users WHERE (db_id = @ID) AND (status = 'normal')", _conn))
                {
                    _cmd.Parameters.AddWithValue("@ID", databaseId);

                    using (MySqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new DatabaseUser();
                            _d.Username = _read["login"].ToString();
                            _d.Password = _read["passwd"].ToString();

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

            using (MySqlConnection _conn = new MySqlConnection(Settings.Default.connectionString))
            {
                _conn.Open();
                using (MySqlCommand _cmd = new MySqlCommand(@"SELECT  subdomains.name, domains.name AS domain, sys_users.login, accounts.password
                                                                FROM accounts RIGHT OUTER JOIN
                                                        sys_users ON accounts.id = sys_users.account_id RIGHT OUTER JOIN
                                                        subdomains ON sys_users.id = subdomains.sys_user_id LEFT OUTER JOIN
                                                        domains ON subdomains.dom_id = domains.id 
                                                    WHERE domains.name = @NAME", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (MySqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new Subdomain();
                            _d.Domain = _read["domain"].ToString();
                            _d.Login = _read["login"].ToString();
                            _d.Password = _read["password"].ToString();
                            _d.Name = _read["name"].ToString();
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

            using (MySqlConnection _conn = new MySqlConnection(Settings.Default.connectionString))
            {
                _conn.Open();
                using (MySqlCommand _cmd = new MySqlCommand(@"SELECT domain_aliases.name as alias, domains.name AS domain, domain_aliases.status
                                                                FROM  domain_aliases 
                                                            INNER JOIN domains ON domain_aliases.dom_id = domains.id 
                                                            WHERE domain_aliases.status = 0 AND domains.name = @NAME", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (MySqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new DomainAlias();
                            _d.Domain = _read["domain"].ToString();
                            _d.Alias = _read["alias"].ToString();

                            _tmp.Add(_d);
                        }
                    }
                }
                _conn.Close();
            }

            return _tmp;
        }
    }
}
