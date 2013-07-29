namespace MpMigrate.Data.Dal
{
    using MpMigrate.Data.Entity;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.OleDb;
    using System.Linq;
    using System.Text;

    public class Plesk_9_Access : DboFactory
    {
        private string connectionString;

        public override List<Domain> GetDomains()
        {
            var tmp = new List<Domain>();

            using (OleDbConnection _conn = new OleDbConnection(connectionString))
            {
                _conn.Open();
                using (OleDbCommand _cmd = new OleDbCommand(@"SELECT        
                        domains.id, domains.name, hosting.fp_adm, accounts.[password], clients.login, 
                        clients.passwd, dom_level_usrs.passwd AS DomainPass, 
                         domains.status AS Status, NULL AS expiration, domains.limits_id, domains.htype
                                FROM            (((((domains LEFT OUTER JOIN
                                            hosting ON hosting.dom_id = domains.id) 
                            LEFT OUTER JOIN
                         sys_users ON hosting.sys_user_id = sys_users.id) 
                            LEFT OUTER JOIN
                         accounts ON accounts.id = sys_users.account_id) 
                            LEFT OUTER JOIN
                         clients ON clients.id = domains.cl_id) 
                            LEFT OUTER JOIN
                         dom_level_usrs ON dom_level_usrs.dom_id = domains.id)
                                        ORDER BY domains.id", _conn))
                {
                    using (OleDbDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new Domain();
                            _d.Id = DataExtensions.GetColumnValue<int>(_read, "id");
                            _d.Name = DataExtensions.GetColumnValue<String>(_read, "name").ToLower();
                            _d.ClientName = DataExtensions.GetColumnValue<String>(_read, "login");
                            _d.DomainPassword = DataExtensions.GetColumnValue<String>(_read, "DomainPass");

                            _d.Username = DataExtensions.GetColumnValue<String>(_read, "fp_adm");
                            _d.Password = DataExtensions.GetColumnValue<String>(_read, "password");

                            if (String.IsNullOrEmpty(_d.Username))
                                _d.Username = _d.Name;

                            if (String.IsNullOrEmpty(_d.Password))
                                _d.Password = DataHelper.GetPassword();

                            _d.Status = Convert.ToInt64(DataExtensions.GetColumnValue<decimal>(_read, "Status"));

                            
                            var limitId = DataExtensions.GetColumnValue<int>(_read, "limits_id");
                            _d.Expiration = GetExpirationDate(limitId);

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

        public override List<Email> GetEmails(string domainName)
        {
            var _tmp = new List<Email>();

            using (OleDbConnection _conn = new OleDbConnection(connectionString))
            {
                _conn.Open();
                using (OleDbCommand _cmd = new OleDbCommand(@"SELECT
                                                                    mail.mail_name, domains.name, accounts.[password], 
                                                                    domains.status, mail.redirect, mail.redir_addr, mail.mbox_quota
                                                            FROM            ((domains LEFT OUTER JOIN
                                                                                        mail ON mail.dom_id = domains.id) LEFT OUTER JOIN
                                                                                        accounts ON accounts.id = mail.account_id)
                                                            WHERE (domains.name = ?) AND (mail.mail_name <> '')", _conn))
                {
                    _cmd.CommandType = CommandType.Text;
                    _cmd.Parameters.AddWithValue("NAME", domainName);

                    using (OleDbDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new Email();
                            _d.Name = DataExtensions.GetColumnValue<String>(_read, "mail_name").ToLower();
                            _d.DomainName = DataExtensions.GetColumnValue<String>(_read, "name").ToLower();
                            _d.Password = DataExtensions.GetColumnValue<String>(_read, "password");
                            _d.Redirect = DataExtensions.GetColumnValue<String>(_read, "redirect");
                            _d.RedirectedEmail = DataExtensions.GetColumnValue<String>(_read, "redir_addr");

                            var mboxQuota = DataExtensions.GetColumnValue<decimal>(_read, "mbox_quota");

                            if (mboxQuota > 0)
                                _d.Quota = Convert.ToDouble(Math.Round(((mboxQuota / 1024) / 1024), 0));
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

        public override HostLimit GetDomainLimits(string domainName)
        {
            var _tmp_limits = new List<LimitRow>();

            using (OleDbConnection _conn = new OleDbConnection(connectionString))
            {
                _conn.Open();
                using (OleDbCommand _cmd = new OleDbCommand(@"SELECT L.limit_name, L.[value] 
                                                                    FROM (domains D LEFT OUTER JOIN limits L ON L.id = D.limits_id) 
                                                            WHERE (D.name = ?)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (OleDbDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new LimitRow();
                            _d.Name = DataExtensions.GetColumnValue<string>(_read, "limit_name");

                            var LimitValue = DataExtensions.GetColumnValue<decimal>(_read, "value");

                            if (_d.Name == "disk_space" || _d.Name == "max_traffic" || _d.Name == "mbox_quota"
                                || _d.Name == "mssql_dbase_space" || _d.Name == "mysql_dbase_space")
                            {
                                _d.Value = (int)((LimitValue / 1024) / 1024);
                            }
                            else
                            {
                                _d.Value = Convert.ToInt32(LimitValue);
                            }

                            _tmp_limits.Add(_d);

                        }
                    }
                }
                _conn.Close();
            }

            return new HostLimit(_tmp_limits); 
        }

        public override DnsZone GetDnsZone(string domainName)
        {
            var _tmp = new DnsZone();

            using (OleDbConnection _conn = new OleDbConnection(connectionString))
            {
                _conn.Open();
                using (OleDbCommand _cmd = new OleDbCommand(@"SELECT dns_zone.id, dns_zone.name, dns_zone.displayName, dns_zone.email, dns_zone.status, dns_zone.type, dns_zone.ttl, dns_zone.ttl_unit, dns_zone.refresh, 
                         dns_zone.refresh_unit, dns_zone.retry, dns_zone.retry_unit, dns_zone.expire, dns_zone.expire_unit, dns_zone.minimum, dns_zone.minimum_unit, 
                         dns_zone.serial_format, dns_zone.serial
                                            FROM            (dns_zone LEFT OUTER JOIN
                                                                     domains ON dns_zone.name = domains.name)
                                            WHERE        (domains.name = ?)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (OleDbDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            _tmp.Name = DataExtensions.GetColumnValue<string>(_read, "name").ToLower();
                            _tmp.mininum = DataExtensions.GetColumnValue<int>(_read, "minimum");
                            _tmp.refresh = DataExtensions.GetColumnValue<int>(_read, "refresh");
                            _tmp.retry = DataExtensions.GetColumnValue<int>(_read, "retry");
                            _tmp.expire = DataExtensions.GetColumnValue<int>(_read, "expire");

                            var serial = DataExtensions.GetColumnValue<string>(_read, "serial");
                            _tmp.serial = Convert.ToInt32(serial);

                            _tmp.ttl = DataExtensions.GetColumnValue<int>(_read, "ttl");
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

            using (OleDbConnection _conn = new OleDbConnection(connectionString))
            {
                _conn.Open();
                using (OleDbCommand _cmd = new OleDbCommand(@"SELECT dns_recs.id, dns_recs.dns_zone_id, dns_recs.type, dns_recs.host, dns_recs.displayHost, dns_recs.val, dns_recs.displayVal, dns_recs.opt, 
                                                                        dns_recs.time_stamp
                                                                    FROM (dns_zone LEFT OUTER JOIN  dns_recs ON dns_recs.dns_zone_id = dns_zone.id)
                                                                    WHERE (dns_zone.name = ?) AND (dns_recs.type <> 'PTR')", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (OleDbDataReader _read = _cmd.ExecuteReader())
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

                using (OleDbConnection _conn = new OleDbConnection(connectionString))
                {
                    _conn.Open();
                    using (OleDbCommand _cmd = new OleDbCommand(@"SELECT D.name, F.redirect
                                                                        FROM (domains D INNER JOIN
                                                                    forwarding F ON F.dom_id = D.id)
                                                                                WHERE (D.name = ?)", _conn))
                    {
                        _cmd.Parameters.AddWithValue("@NAME", domainName);

                        using (OleDbDataReader _read = _cmd.ExecuteReader())
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

        public override List<DomainAlias> GetDomainAliases(string domainName)
        {
            var _tmp = new List<DomainAlias>();

            using (OleDbConnection _conn = new OleDbConnection(connectionString))
            {
                _conn.Open();
                using (OleDbCommand _cmd = new OleDbCommand(@"SELECT domain_aliases.name AS alias, domains.name AS [domain], domain_aliases.status
                                                                    FROM (domain_aliases INNER JOIN
                                                                                             domains ON domain_aliases.dom_id = domains.id)
                                                                    WHERE        (domains.name = ?) AND (domain_aliases.status = 0)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (OleDbDataReader _read = _cmd.ExecuteReader())
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

            using (OleDbConnection _conn = new OleDbConnection(connectionString))
            {
                _conn.Open();
                using (OleDbCommand _cmd = new OleDbCommand(@"SELECT data_bases.id AS db_id, domains.name AS [domain], data_bases.name, data_bases.type, data_bases.db_server_id
                                                    FROM            ((data_bases LEFT OUTER JOIN
                                                                             domains ON domains.id = data_bases.dom_id) LEFT OUTER JOIN
                                                                             db_users ON db_users.id = data_bases.default_user_id)
                                                    WHERE        (domains.name = ?)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (OleDbDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new Database();
                            _d.Id = DataExtensions.GetColumnValue<int>(_read, "db_id");
                            _d.Name = DataExtensions.GetColumnValue<string>(_read, "name");
                            _d.Domain = DataExtensions.GetColumnValue<string>(_read, "domain");
                            _d.DbType = DataExtensions.GetColumnValue<string>(_read, "type");
                            _d.ServerId = DataExtensions.GetColumnValue<int>(_read, "db_server_id");
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

                using (OleDbConnection _conn = new OleDbConnection(connectionString))
                {
                _conn.Open();
                    using (OleDbCommand _cmd = new OleDbCommand(@"SELECT login, passwd FROM db_users WHERE (db_id = ?) AND (status = 'normal')", _conn))
                    {
                    _cmd.Parameters.AddWithValue("@ID", database_id);

                        using (OleDbDataReader _read = _cmd.ExecuteReader())
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

        public override List<Subdomain> GetSubdomains(string domainName)
        {
            var _tmp = new List<Subdomain>();

            using (OleDbConnection _conn = new OleDbConnection(connectionString))
            {
                _conn.Open();
                using (OleDbCommand _cmd = new OleDbCommand(@"SELECT subdomains.name, domains.name AS [domain], sys_users.login, accounts.[password], subdomains.sys_user_type
                                                                    FROM (((accounts RIGHT OUTER JOIN
                                                                                             sys_users ON accounts.id = sys_users.account_id) RIGHT OUTER JOIN
                                                                                             subdomains ON sys_users.id = subdomains.sys_user_id) LEFT OUTER JOIN
                                                                                             domains ON subdomains.dom_id = domains.id)
                                                                    WHERE (domains.name = ?)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (OleDbDataReader _read = _cmd.ExecuteReader())
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

        public override List<Reseller> GetResellers()
        {
            var tmp = new List<Reseller>();

            using (OleDbConnection _conn = new OleDbConnection(connectionString))
            {
                _conn.Open();
                using (OleDbCommand _cmd = new OleDbCommand(@"SELECT id, cr_date, cname, pname, login, passwd, status, phone, fax, email, address, city, state, pcode, country, locale, limits_id, params_id, perm_id, pool_id, 
                         logo_id, tmpl_id, sapp_pool_id, uid, ownership, [guid], parent_id, type, overuse FROM clients WHERE (type = 'client') OR (type = 'reseller')", _conn))
                {
                    using (OleDbDataReader _read = _cmd.ExecuteReader())
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
                            res.Password = DataExtensions.GetColumnValue<string>(_read, "passwd");
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

        public override PanelStats GetPanelStats()
        {
            var pstats = new PanelStats();

            using (OleDbConnection _conn = new OleDbConnection(connectionString))
            {
                _conn.Open();

                #region Disk Space
                using (OleDbCommand _cmd = new OleDbCommand(@"SELECT SUM(httpdocs) AS httpdocs, 
                                                                        SUM(mysql_dbases) + SUM(mssql_dbases) AS totaldbsize, 
                                                                SUM(mailboxes) AS totalmailboxsize, 
                                                                SUM(subdomains) AS subdomainsize FROM disk_usage", _conn))
                {
                    using (OleDbDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
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
                    pstats.TotalDomainCount = StatsItem<int>("SELECT COUNT(*) FROM domains");
                    pstats.TotalEmailCount = StatsItem<int>("SELECT COUNT(*) FROM mail");
                    pstats.TotalResellerCount = StatsItem<int>("SELECT COUNT(*) FROM clients");
                    pstats.TotalDatabaseCount = StatsItem<int>("SELECT COUNT(*) FROM data_bases");
                    pstats.TotalDomainAliasCount = StatsItem<int>("SELECT COUNT(*) FROM domain_aliases");
                    pstats.TotalSubdomainCount = StatsItem<int>("SELECT COUNT(*) FROM subdomains");                
                #endregion

                _conn.Close();
            }

            return pstats;
        }

        private T StatsItem<T>(string sqltxt)
        {
            var result = default(T);

            using (OleDbConnection _conn = new OleDbConnection(connectionString))
            {
                _conn.Open();

                using (OleDbCommand _cmd = new OleDbCommand(sqltxt, _conn))
                {
                    result = DataExtensions.GetColumnValue<T>(_cmd.ExecuteScalar(), sqltxt);
                }
                
                _conn.Close();
            }

            return result;
        }

        public override HostLimit ResellerLimits(string clientName)
        {
            var _tmp_limits = new List<LimitRow>();

            using (OleDbConnection _conn = new OleDbConnection(connectionString))
            {
                _conn.Open();
                using (OleDbCommand _cmd = new OleDbCommand(@"SELECT L.limit_name, L.[value] 
                                            FROM (clients C LEFT OUTER JOIN limits L ON L.id = C.limits_id) WHERE (C.login = ?)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", clientName);

                    using (OleDbDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new LimitRow();
                            _d.Name = DataExtensions.GetColumnValue<string>(_read, "limit_name");

                            var LimitValue = Convert.ToInt64(DataExtensions.GetColumnValue<string>(_read, "value"));

                            if (_d.Name == "disk_space" || _d.Name == "max_traffic" 
                                || _d.Name == "mbox_quota" || _d.Name == "total_mboxes_quota")
                            {
                                _d.Value = (int)((LimitValue / 1024) / 1024);
                            }
                            else
                            {
                                _d.Value = Convert.ToInt32(LimitValue);
                            }

                            _tmp_limits.Add(_d);

                        }
                    }
                }
                _conn.Close();
            }

            return new HostLimit(_tmp_limits);
        }

        public override void LoadConnectionString(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public override bool SecurePasswords()
        {
            var securePass = false;

            using (OleDbConnection _conn = new OleDbConnection(connectionString))
            {
                _conn.Open();
                using (OleDbCommand _cmd = new OleDbCommand(@"SELECT val FROM misc WHERE param = 'secure_passwords'", _conn))
                {
                    var secure_passwords = _cmd.ExecuteScalar();

                    if (secure_passwords != null)
                        bool.TryParse(secure_passwords.ToString(), out securePass);
                }
                _conn.Close();
            }

            return securePass;
        }

        private DateTime GetExpirationDate(int limit_id)
        {
            DateTime _expiration = DateTime.MaxValue;

            using (OleDbConnection _conn = new OleDbConnection(connectionString))
            {
                _conn.Open();
                using (OleDbCommand _cmd = new OleDbCommand(@"SELECT [value] FROM limits WHERE (limit_name = 'expiration') AND (id = ?)", _conn))
                {
                    _cmd.CommandType = CommandType.Text;
                    _cmd.Parameters.AddWithValue("NAME", limit_id);

                    var obj = _cmd.ExecuteScalar();
                    if (obj != null)
                    {
                        var objValue = Convert.ToDouble(obj);
                        _expiration = DataHelper.UnixTimeStampToDateTime(objValue);
                    }
                }
            }

            return _expiration;
        }
    }
}
