namespace MpMigrate.Data.Dal
{
    using MpMigrate.Data.Entity;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;

    public class Helm_MsSQL : DboFactory
    {
        private string connectionString;

        public override List<Domain> GetDomains()
        {
            var _tmp = new List<Domain>();

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT HostDomain.DomainName, Account.AccountNumber, HostDomain.DomainId
                                                            FROM Account INNER JOIN
                                                                  Package ON Account.AccountNumber = Package.AccountNumber RIGHT OUTER JOIN
                                                                  HostDomain ON Package.PackageId = HostDomain.PackageId", _conn))
                {
                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new Domain();
                            _d.Id = DataExtensions.GetColumnValue<int>(_read, "DomainId");
                            _d.Name = DataExtensions.GetColumnValue<String>(_read, "DomainName").ToLower();
                            _d.ClientName = DataExtensions.GetColumnValue<String>(_read, "AccountNumber");
                            _d.DomainPassword = DataHelper.GetPassword();

                            _d.Username = GetFirstFtpAccount(_d.Id);
                            _d.Password = _d.DomainPassword;

                            _d.Status = DataExtensions.GetColumnValue<int>(_read, "DomainStatus");
                            _d.Expiration = DateTime.Now.AddYears(1);

                            _d.isForwarding = false;

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

        public override List<Email> GetEmails(string domainName)
        {
            throw new NotImplementedException();
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

            var hostLimits = new HostLimit();
            hostLimits.LoadHelmLimits(_tmp_limits);

            return hostLimits;
        }

        public override DnsZone GetDnsZone(string domainName)
        {
            throw new NotImplementedException();
        }

        public override List<DnsZoneRecord> GetZoneRecords(string domainName)
        {
            throw new NotImplementedException();
        }

        public override Forwarding GetForwarding(string domainName)
        {
            throw new NotImplementedException();
        }

        public override List<DomainAlias> GetDomainAliases(string domainName)
        {
            var _tmp = new List<DomainAlias>();
            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT HostDomain_1.DomainName 
                                                            FROM HostDomain INNER JOIN HostDomain AS HostDomain_1 ON HostDomain.DomainId = HostDomain_1.DomainAliasId 
                                                            WHERE (HostDomain.DomainName = @NAME)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var da = new DomainAlias();
                            da.Alias = DataExtensions.GetColumnValue<String>(_read, "DomainName");
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
                
                using (SqlCommand _cmd = new SqlCommand(@"SELECT UserDatabase.DBName, UserDatabase.DataBaseId 
                                                            FROM UserDatabase INNER JOIN HostDomain ON UserDatabase.DomainId = HostDomain.DomainId 
                                                            WHERE (UserDatabase.DBType = 22) AND (HostDomain.DomainName = @NAME)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var da = new Database();
                            da.Id = DataExtensions.GetColumnValue<int>(_read, "DataBaseId");
                            da.Name = DataExtensions.GetColumnValue<String>(_read, "DBName");
                            da.Domain = domainName;
                            da.DbType = "mssql";

                            da.Users = GetDatabaseUsers(da.Id);

                            _tmp.Add(da);
                        }
                    }
                }
            }

            return _tmp;
        }

        public override List<DatabaseUser> GetDatabaseUsers(int database_id)
        {
            var _tmp = new List<DatabaseUser>();
            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT DBUserName, DBPassword FROM DatabaseUser WHERE (DatabaseId = @ID)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@ID", database_id);

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var da = new DatabaseUser();
                            da.Password = DataHelper.GetPassword();
                            da.Username = DataExtensions.GetColumnValue<String>(_read, "DBUserName");

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
            throw new NotImplementedException();
        }

        public override List<Reseller> GetResellers()
        {
            throw new NotImplementedException();
        }

        public override PanelStats GetPanelStats()
        {
            throw new NotImplementedException();
        }

        public override HostLimit ResellerLimits(string clientName)
        {
            throw new NotImplementedException();
        }

        public override void LoadConnectionString(string connectionString)
        {
            throw new NotImplementedException();
        }

        public override bool SecurePasswords()
        {
            throw new NotImplementedException();
        }


        private string GetFirstFtpAccount(int domainId)
        {
            var ftpUserName = String.Empty;

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT FTPPassword, FTPUserName FROM FTPAccount WHERE DomainId = @ID", _conn))
                {
                    _cmd.Parameters.AddWithValue("@ID", domainId);

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        if (_read.Read())
                        {
                            ftpUserName = DataExtensions.GetColumnValue<String>(_read, "FTPUserName");                            
                        }
                    }
                }

                _conn.Close();
            }

            return account;
        }
    }
}
