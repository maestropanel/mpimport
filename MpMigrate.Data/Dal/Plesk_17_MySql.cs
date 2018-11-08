namespace MpMigrate.Data.Dal
{
    using MpMigrate.Data.Entity;
    using MySql.Data.MySqlClient;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Globalization;

    public class Plesk_17_MySql : DboFactory
    {
        //Sync
        private string connectionString;

        public Plesk_17_MySql()
        {

        }

        public Plesk_17_MySql(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public override List<Domain> GetDomains()
        {
            var tmp = new List<Domain>();
            var securePassword = SecurePasswords();

            using (MySqlConnection _conn = new MySqlConnection(connectionString))
            {
                _conn.Open();

                using (MySqlCommand _cmd = new MySqlCommand(@"SELECT 
                                                             domains.id, 
                                                             domains.name, 
                                                             domains.name AS fp_adm, 
                                                             accounts.password, 
                                                             clients.login, 
                                                             dom_level_usrs.passwd AS DomainPass, 
                                                             domains.status AS STATUS, 
                                                             limits.value AS expiration,
                                                             domains.htype
                                                            FROM domains
                                                                LEFT JOIN hosting ON hosting.dom_id = domains.id
                                                                LEFT JOIN sys_users ON hosting.sys_user_id = sys_users.id
                                                                LEFT JOIN accounts ON accounts.id = sys_users.account_id
                                                                LEFT JOIN clients ON clients.id = domains.cl_id
                                                                LEFT JOIN dom_level_usrs ON dom_level_usrs.dom_id = domains.id
                                                                LEFT JOIN limits ON domains.limits_id = limits.id AND limits.limit_name = 'expiration'", _conn))
                {
                    using (MySqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new Domain();
                            _d.Id = Convert.ToInt32(DataExtensions.GetColumnValue<uint>(_read, "id"));
                            _d.Name = DataExtensions.GetColumnValue<String>(_read, "name").ToLower(CultureInfo.GetCultureInfo("en-US"));
                            _d.ClientName = DataExtensions.GetColumnValue<String>(_read, "login");
                            _d.DomainPassword = securePassword ? DataHelper.GetPassword() : DataExtensions.GetColumnValue<String>(_read, "DomainPass");
                            _d.Username = DataExtensions.GetColumnValue<String>(_read, "fp_adm");
                            _d.Password = securePassword ? DataHelper.GetPassword() : DataExtensions.GetColumnValue<String>(_read, "password");
                            _d.Status = Convert.ToInt64(DataExtensions.GetColumnValue<ulong>(_read, "STATUS"));

                            var expirationUnitTime = DataExtensions.GetColumnValue<Int64>(_read, "expiration");
                            if (expirationUnitTime != -1)
                                _d.Expiration = DataHelper.UnixTimeStampToDateTime(expirationUnitTime);

                            var hostingType = DataExtensions.GetColumnValue<String>(_read, "htype");
                            _d.isForwarding = (hostingType == "std_fwd" || hostingType == "frm_fwd");

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
            var securePassword = SecurePasswords();

            using (MySqlConnection _conn = new MySqlConnection(connectionString))
            {
                _conn.Open();
                using (MySqlCommand _cmd = new MySqlCommand(@"SELECT  C.id, cname, pname, login, A.password, phone, fax, email, address, city, state, pcode, country
                                                                        FROM clients C 
                                                                    LEFT JOIN accounts A ON C.account_id = A.id 
                                                                            WHERE C.`type`='reseller' OR C.`type`='client'", _conn))
                {
                    using (MySqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var res = new Reseller();
                            res.Id = Convert.ToInt32(DataExtensions.GetColumnValue<uint>(_read, "id"));
                            res.Address1 = DataExtensions.GetColumnValue<string>(_read, "address");
                            res.City = DataExtensions.GetColumnValue<string>(_read, "city");
                            res.Country = DataExtensions.GetColumnValue<string>(_read, "country");
                            res.fax = DataExtensions.GetColumnValue<string>(_read, "fax");
                            res.Phone = DataExtensions.GetColumnValue<string>(_read, "phone");
                            res.PostalCode = DataExtensions.GetColumnValue<string>(_read, "pcode");
                            res.Province = DataExtensions.GetColumnValue<string>(_read, "country");

                            res.FirstName = DataExtensions.GetColumnValue<string>(_read, "pname");

                            res.Password = securePassword ? DataHelper.GetPassword() : DataExtensions.GetColumnValue<string>(_read, "password");
                            res.Username = DataExtensions.GetColumnValue<string>(_read, "login");
                            res.Email = DataExtensions.GetColumnValue<string>(_read, "email");
                            res.Organization = DataExtensions.GetColumnValue<string>(_read, "cname");

                            res.Limits = ResellerLimits(res.Username);

                            tmp.Add(res);
                        }
                    }
                }
                _conn.Close();
            }

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
                            //_d.Status = DataExtensions.GetColumnValue<long>(_read, "status");

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
            var securePassword = SecurePasswords();

            using (MySqlConnection _conn = new MySqlConnection(connectionString))
            {
                _conn.Open();
                using (MySqlCommand _cmd = new MySqlCommand(@"SELECT login, accounts.password FROM  db_users JOIN accounts ON db_users.account_id = accounts.Id 
                                                                        WHERE  (db_id = @ID) AND (status = 'normal')", _conn))
                {
                    _cmd.Parameters.AddWithValue("@ID", database_id);

                    using (MySqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new DatabaseUser();
                            _d.Username = DataExtensions.GetColumnValue<string>(_read, "login");
                            _d.Password = securePassword ? DataHelper.GetPassword() : DataExtensions.GetColumnValue<string>(_read, "password");

                            _tmp.Add(_d);
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

                            var LimitValue = DataExtensions.GetColumnValue<Int64>(_read, "value");
                            if (_d.Name == "disk_space" || _d.Name == "max_traffic" || _d.Name == "mbox_quota"
                                || _d.Name == "mssql_dbase_space" || _d.Name == "mysql_dbase_space" || _d.Name == "max_traffic_soft")
                            {
                                _d.Value = (int)((LimitValue / 1024) / 1024);
                            }
                            else
                            {
                                if (LimitValue > int.MaxValue)
                                {
                                    _d.Value = -1;
                                }
                                else
                                {
                                    _d.Value = Convert.ToInt32(LimitValue);
                                }


                            }

                            _tmp_limits.Add(_d);
                        }
                    }
                }
                _conn.Close();
            }

            return new HostLimit(_tmp_limits);
        }

        public override List<Subdomain> GetSubdomains(string domainName)
        {
            var _tmp = new List<Subdomain>();
            var securePassword = SecurePasswords();
            using (MySqlConnection _conn = new MySqlConnection(connectionString))
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
                            _d.Domain = DataExtensions.GetColumnValue<string>(_read, "domain");
                            _d.Login = DataExtensions.GetColumnValue<string>(_read, "login");
                            _d.Password = securePassword ? DataHelper.GetPassword() : DataExtensions.GetColumnValue<string>(_read, "password");
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
                            _tmp.Name = DataExtensions.GetColumnValue<string>(_read, "name").ToLower();
                            _tmp.mininum = Convert.ToInt32(DataExtensions.GetColumnValue<uint>(_read, "minimum"));
                            _tmp.refresh = Convert.ToInt32(DataExtensions.GetColumnValue<uint>(_read, "refresh"));
                            _tmp.retry = Convert.ToInt32(DataExtensions.GetColumnValue<uint>(_read, "retry"));
                            _tmp.expire = Convert.ToInt32(DataExtensions.GetColumnValue<uint>(_read, "expire"));

                            var serial = DataExtensions.GetColumnValue<string>(_read, "serial");
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
            var priority = 0;

            using (MySqlConnection _conn = new MySqlConnection(connectionString))
            {
                _conn.Open();
                using (MySqlCommand _cmd = new MySqlCommand(@"SELECT dns_recs.* FROM dns_zone 
                                                                        LEFT JOIN dns_recs ON dns_recs.dns_zone_id = dns_zone.id 
                                                                            WHERE dns_zone.name = @NAME AND dns_recs.type <> 'PTR'", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (MySqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new DnsZoneRecord();

                            var hostName = DataExtensions.GetColumnValue<string>(_read, "host");

                            if (hostName == domainName || hostName == String.Format("{0}.", domainName))
                                _d.name = "@";
                            else
                                _d.name = hostName.Split('.').FirstOrDefault().ToLower();


                            _d.type = DataExtensions.GetColumnValue<string>(_read, "type");
                            _d.value = DataExtensions.GetColumnValue<string>(_read, "val").ToLower();

                            var options = DataExtensions.GetColumnValue<string>(_read, "opt");

                            if (!String.IsNullOrEmpty(options))
                                if (int.TryParse(options, out priority))
                                    _d.priority = priority;
                                else
                                    _d.priority = 0;
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

        public override PanelStats GetPanelStats()
        {
            var pstats = new PanelStats();

            using (MySqlConnection _conn = new MySqlConnection(connectionString))
            {
                _conn.Open();

                #region Disk Space
                using (MySqlCommand _cmd = new MySqlCommand(@"SELECT  CAST(SUM(httpdocs) AS SIGNED) as httpdocs, 
                                                                        CAST((SUM(mysql_dbases) + SUM(mssql_dbases)) AS SIGNED) as totaldbsize, 
                                                                        CAST(SUM(mailboxes) AS SIGNED) as totalmailboxsize, 
                                                                        CAST(SUM(subdomains) AS SIGNED) as subdomainsize 
                                                            FROM disk_usage", _conn))
                {
                    using (MySqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            //form MySQL 4
                            if (_read["httpdocs"] is System.Int64)
                                pstats.TotalDomainDiskSpace = Convert.ToDecimal(DataExtensions.GetColumnValue<System.Int64>(_read, "httpdocs"));
                            else
                                pstats.TotalDomainDiskSpace = DataExtensions.GetColumnValue<decimal>(_read, "httpdocs");

                            if (_read["totaldbsize"] is System.Int64)
                                pstats.TotalDatabaseDiskSpace = Convert.ToDecimal(DataExtensions.GetColumnValue<Int64>(_read, "totaldbsize"));
                            else
                                pstats.TotalDatabaseDiskSpace = DataExtensions.GetColumnValue<decimal>(_read, "totaldbsize");

                            if (_read["totalmailboxsize"] is System.Int64)
                                pstats.TotalEmailDiskSpace = Convert.ToDecimal(DataExtensions.GetColumnValue<Int64>(_read, "totalmailboxsize"));
                            else
                                pstats.TotalEmailDiskSpace = DataExtensions.GetColumnValue<decimal>(_read, "totalmailboxsize");

                            if (_read["subdomainsize"] is System.Int64)
                                pstats.TotalSubdomainDiskSpace = Convert.ToDecimal(DataExtensions.GetColumnValue<Int64>(_read, "subdomainsize"));
                            else
                                pstats.TotalSubdomainDiskSpace = DataExtensions.GetColumnValue<decimal>(_read, "subdomainsize");
                        }
                    }
                }
                #endregion

                #region Count
                using (MySqlCommand _cmd = new MySqlCommand(@"SELECT 
                                                                (SELECT COUNT(*) FROM domains) as domaincount, 
                                                                (SELECT COUNT(*) FROM mail) as mailcount, 
                                                                (SELECT COUNT(*) FROM clients) as resellercount, 
                                                                (SELECT COUNT(*) FROM data_bases) as databasecount, 
                                                                (SELECT COUNT(*) FROM domain_aliases) as aliascount, 
                                                                (SELECT COUNT(*) FROM subdomains) as subdomaincount", _conn))
                {
                    using (MySqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            pstats.TotalDomainCount = DataExtensions.GetColumnValue<long>(_read, "domaincount");
                            pstats.TotalEmailCount = DataExtensions.GetColumnValue<long>(_read, "mailcount");
                            pstats.TotalResellerCount = DataExtensions.GetColumnValue<long>(_read, "resellercount");
                            pstats.TotalDatabaseCount = DataExtensions.GetColumnValue<long>(_read, "databasecount");
                            pstats.TotalDomainAliasCount = DataExtensions.GetColumnValue<long>(_read, "aliascount");
                            pstats.TotalSubdomainCount = DataExtensions.GetColumnValue<long>(_read, "subdomaincount");
                        }
                    }
                }
                #endregion

                _conn.Close();
            }

            return pstats;
        }

        public override void LoadConnectionString(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public override HostLimit ResellerLimits(string clientName)
        {
            var _tmp_limits = new List<LimitRow>();

            using (MySqlConnection _conn = new MySqlConnection(connectionString))
            {
                _conn.Open();
                using (MySqlCommand _cmd = new MySqlCommand(@"SELECT L.limit_name, L.value FROM clients C LEFT JOIN limits L ON L.id = C.limits_id WHERE C.login = @NAME", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", clientName);

                    using (MySqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new LimitRow();
                            _d.Name = DataExtensions.GetColumnValue<string>(_read, "limit_name");
                            //_d.Value = Convert.ToInt32(DataExtensions.GetColumnValue<Int64>(_read, "value"));

                            long LimitValue = -1;

                            if (_read["value"] is System.Int64)
                                LimitValue = DataExtensions.GetColumnValue<Int64>(_read, "value");
                            else
                                LimitValue = Convert.ToInt64(DataExtensions.GetColumnValue<string>(_read, "value"));


                            if (_d.Name == "disk_space" || _d.Name == "max_traffic" || _d.Name == "mbox_quota" || _d.Name == "total_mboxes_quota")
                            {
                                _d.Value = (int)((LimitValue / 1024) / 1024);
                            }
                            else
                            {
                                if (LimitValue > int.MaxValue)
                                {
                                    _d.Value = -1;
                                }
                                else
                                {
                                    _d.Value = Convert.ToInt32(LimitValue);
                                }
                            }

                            _tmp_limits.Add(_d);

                        }
                    }
                }
                _conn.Close();
            }

            return new HostLimit(_tmp_limits);
        }

        public override bool SecurePasswords()
        {
            var securePass = false;

            using (MySqlConnection _conn = new MySqlConnection(connectionString))
            {
                _conn.Open();
                using (MySqlCommand _cmd = new MySqlCommand(@"SELECT val FROM misc WHERE param = 'secure_passwords'", _conn))
                {
                    var secure_passwords = _cmd.ExecuteScalar();

                    if (secure_passwords != null)
                        bool.TryParse(secure_passwords.ToString(), out securePass);
                }
                _conn.Close();
            }

            return securePass;
        }

        public override List<Email> GetEmails(string domainName)
        {
            var securePassword = SecurePasswords();

            var _tmp = new List<Email>();

            using (MySqlConnection _conn = new MySqlConnection(connectionString))
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
                            _d.Name = DataExtensions.GetColumnValue<String>(_read, "mail_name").ToLower();
                            _d.DomainName = DataExtensions.GetColumnValue<String>(_read, "name").ToLower();
                            _d.Password = securePassword ? DataHelper.GetPassword() : DataExtensions.GetColumnValue<String>(_read, "password");
                            _d.Redirect = DataExtensions.GetColumnValue<String>(_read, "redirect");
                            _d.RedirectedEmail = DataExtensions.GetColumnValue<String>(_read, "redir_addr");

                            var mboxQuota = Convert.ToDouble(DataExtensions.GetColumnValue<Int64>(_read, "mbox_quota"));
                            if (mboxQuota > 0)
                                _d.Quota = Math.Round(((mboxQuota / 1024) / 1024), 0);
                            else
                                _d.Quota = -1;

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
