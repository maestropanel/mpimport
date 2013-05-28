namespace MpMigrate.Data.Dal
{
    using MpMigrate.Data.Entity;
    using MySql.Data.MySqlClient;
    using System;
    using System.Collections.Generic;
    using System.Data;

    public class Plesk_86_MySql : DboFactory
    {
        private string connectionString;
        
        public Plesk_86_MySql(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public override List<Domain> GetDomains()
        {
            var tmp = new List<Domain>();

            using (MySqlConnection _conn = new MySqlConnection(connectionString))
            {
                _conn.Open();

                using (MySqlCommand _cmd = new MySqlCommand(@"SELECT 
                                                                    domains.id, 
                                                                    domains.name, 
                                                                    hosting.fp_adm, 
                                                                    accounts.password, 
                                                                    clients.login, 
                                                                    clients.passwd, 
                                                                    dom_level_usrs.passwd As DomainPass, 
                                                                    domains.status As Status, 
                                                                    limits.value as expiration,
                                                                    domains.htype
			                                            FROM domains 
                                                LEFT JOIN hosting ON hosting.dom_id = domains.id 
				                                LEFT JOIN sys_users ON hosting.sys_user_id = sys_users.id 
				                                LEFT JOIN accounts ON accounts.id = sys_users.account_id 
				                                LEFT JOIN clients ON clients.id = domains.cl_id 
				                                LEFT JOIN dom_level_usrs ON dom_level_usrs.dom_id = domains.id 
                                                LEFT JOIN limits ON limits.id = domains.limits_id AND limits.limit_name = 'expiration'", _conn))
                {
                    using (MySqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {                            
                            var _d = new Domain();
                            _d.Id = Convert.ToInt32(DataExtensions.GetColumnValue<uint>(_read, "id"));
                            _d.Name = DataExtensions.GetColumnValue<String>(_read, "name");
                            _d.ClientName = DataExtensions.GetColumnValue<String>(_read, "login");
                            _d.DomainPassword = DataExtensions.GetColumnValue<String>(_read, "DomainPass");
                            _d.Username = DataExtensions.GetColumnValue<String>(_read, "fp_adm");
                            _d.Password = DataExtensions.GetColumnValue<String>(_read, "password");
                            _d.Status = Convert.ToInt64(DataExtensions.GetColumnValue<ulong>(_read, "Status"));
                                                        
                            var hostingType = DataExtensions.GetColumnValue<String>(_read, "htype");
                            _d.isForwarding = (hostingType == "std_fwd" || hostingType == "frm_fwd");

                            if(_d.isForwarding)
                            {
                                var frw = GetForwarding(_d.Name);
                                _d.ForwardUrl = frw.ForwardUrl;
                            }

                            _d.Aliases = GetDomainAliases(_d.Name);
                            _d.Databases = GetDatabases(_d.Name);
                            _d.Limits = GetDomainLimits(_d.Name);
                            _d.Subdomains = GetSubdomains(_d.Name);
                            _d.Zone = GetDnsZone(_d.Name);

                            tmp.Add(_d);
                        }
                    }
                }
                _conn.Close();
            }
            
            return tmp;
        }

        public override List<Reseller> GetResellers()
        {
            var tmp = new List<Reseller>();

            return tmp;
            
        }

        public override List<DomainAlias> GetDomainAliases(string domainName)
        {
            var _tmp = new List<DomainAlias>();

            using (MySqlConnection _conn = new MySqlConnection(connectionString))
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
                            _d.Domain = DataExtensions.GetColumnValue<String>(_read, "domain");
                            _d.Alias = DataExtensions.GetColumnValue<String>(_read, "alias");
                            _d.Status = DataExtensions.GetColumnValue<long>(_read, "status");

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

            using (MySqlConnection _conn = new MySqlConnection(connectionString))
            {
                _conn.Open();
                using (MySqlCommand _cmd = new MySqlCommand(@"SELECT data_bases.id as db_id, domains.name AS domain, data_bases.name, data_bases.type,
                                                                data_bases.db_server_id
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
                            _d.Id = Convert.ToInt32(DataExtensions.GetColumnValue<uint>(_read, "db_id"));
                            _d.Name = DataExtensions.GetColumnValue<string>(_read, "name");
                            _d.Domain = DataExtensions.GetColumnValue<string>(_read, "domain");
                            _d.DbType = DataExtensions.GetColumnValue<string>(_read, "type");
                            _d.ServerId = Convert.ToInt32(DataExtensions.GetColumnValue<uint>(_read, "db_server_id"));
                            _d.Users = GetDatabaseUsers(_d.Id);                            

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
            var _tmp = new List<DatabaseUser>();

            using (MySqlConnection _conn = new MySqlConnection(connectionString))
            {
                _conn.Open();
                using (MySqlCommand _cmd = new MySqlCommand(@"SELECT login, passwd FROM  db_users WHERE (db_id = @ID) AND (status = 'normal')", _conn))
                {
                    _cmd.Parameters.AddWithValue("@ID", database_id);

                    using (MySqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new DatabaseUser();
                            _d.Username = DataExtensions.GetColumnValue<string>(_read, "login");
                            _d.Password = DataExtensions.GetColumnValue<string>(_read, "passwd");

                            _tmp.Add(_d);
                        }
                    }
                }
                _conn.Close();
            }

            return _tmp;
        }

        public override DomainLimit GetDomainLimits(string domainName)
        {
            var _tmp_limits = new List<LimitRow>();

            using (MySqlConnection _conn = new MySqlConnection(connectionString))
            {
                _conn.Open();
                using (MySqlCommand _cmd = new MySqlCommand(@"SELECT L.limit_name, L.value FROM domains D LEFT JOIN limits L ON L.id = D.limits_id WHERE D.name = @NAME", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (MySqlDataReader _read = _cmd.ExecuteReader())
                    {                        
                        while (_read.Read())
                        {
                            var _d = new LimitRow();
                            _d.Name = DataExtensions.GetColumnValue<string>(_read, "limit_name");
                            _d.Value = DataExtensions.GetColumnValue<string>(_read, "value");

                            _tmp_limits.Add(_d);
                            
                        }
                    }
                }
                _conn.Close();
            }
            
            return new DomainLimit(_tmp_limits); 
        }

        public override List<Subdomain> GetSubdomains(string domainName)
        {
            var _tmp = new List<Subdomain>();

            using (MySqlConnection _conn = new MySqlConnection(connectionString))
            {
                _conn.Open();
                using (MySqlCommand _cmd = new MySqlCommand(@"SELECT  subdomains.name, domains.name AS domain, sys_users.login, accounts.password, subdomains.sys_user_type
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
                            _d.Domain = DataExtensions.GetColumnValue<string>(_read, "domain");
                            _d.Login = DataExtensions.GetColumnValue<string>(_read, "login");
                            _d.Password = DataExtensions.GetColumnValue<string>(_read, "password");
                            _d.Name = DataExtensions.GetColumnValue<string>(_read, "name");
                            _d.UserType = DataExtensions.GetColumnValue<string>(_read, "sys_user_type");

                            _tmp.Add(_d);
                        }
                    }
                }
                _conn.Close();
            }

            return _tmp;
        }

        public override DnsZone GetDnsZone(string domainName)
        {
            var _tmp = new DnsZone();

            using (MySqlConnection _conn = new MySqlConnection(connectionString))
            {
                _conn.Open();
                using (MySqlCommand _cmd = new MySqlCommand(@"SELECT dns_zone.* FROM dns_zone 
                                                                        LEFT JOIN domains ON dns_zone.name = domains.name WHERE domains.name = @NAME", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (MySqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            _tmp.Name = DataExtensions.GetColumnValue<string>(_read, "name");
                            _tmp.mininum = Convert.ToInt32(DataExtensions.GetColumnValue<uint>(_read, "minimum"));
                            _tmp.refresh = Convert.ToInt32(DataExtensions.GetColumnValue<uint>(_read, "refresh"));
                            _tmp.retry = Convert.ToInt32(DataExtensions.GetColumnValue<uint>(_read, "retry"));
                            _tmp.expire = Convert.ToInt32(DataExtensions.GetColumnValue<uint>(_read, "expire"));

                            var serial= DataExtensions.GetColumnValue<string>(_read, "serial");
                            _tmp.serial = Convert.ToInt32(serial);

                            _tmp.ttl = Convert.ToInt32(DataExtensions.GetColumnValue<uint>(_read, "ttl"));
                            _tmp.Email = DataExtensions.GetColumnValue<string>(_read, "email");                            
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

            using (MySqlConnection _conn = new MySqlConnection(connectionString))
            {
                _conn.Open();
                using (MySqlCommand _cmd = new MySqlCommand(@"SELECT dns_recs.* FROM dns_zone LEFT JOIN dns_recs ON dns_recs.dns_zone_id = dns_zone.id WHERE dns_zone.name = @NAME", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (MySqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new DnsZoneRecord();
                            _d.name = DataExtensions.GetColumnValue<string>(_read, "host");
                            _d.type = DataExtensions.GetColumnValue<string>(_read, "type");
                            _d.value = DataExtensions.GetColumnValue<string>(_read, "val");

                            var options = DataExtensions.GetColumnValue<string>(_read, "opt");
                            if (!String.IsNullOrEmpty(options))
                                _d.priority = Convert.ToInt32(options);
                            else
                                _d.priority = 0;
                            _tmp.Add(_d);
                        }
                    }
                }
                _conn.Close();
            }

            return _tmp;
        }

        public override Forwarding GetForwarding(string domainName)
        {
            var _tmp = new Forwarding();

            using (MySqlConnection _conn = new MySqlConnection(connectionString))
            {
                _conn.Open();
                using (MySqlCommand _cmd = new MySqlCommand(@"SELECT D.name, F.redirect FROM domains D JOIN forwarding F ON F.dom_id = D.Id WHERE D.name = @NAME", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (MySqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            _tmp.Name = DataExtensions.GetColumnValue<string>(_read, "name");
                            _tmp.ForwardUrl = DataExtensions.GetColumnValue<string>(_read, "redirect");                            
                        }
                    }
                }
                _conn.Close();
            }

            return _tmp;
        }
    }
}
