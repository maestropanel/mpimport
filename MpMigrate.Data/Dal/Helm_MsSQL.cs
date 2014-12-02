namespace MpMigrate.Data.Dal
{
    using MpMigrate.Data.Entity;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;

    public class Helm_MsSQL : DboFactory
    {
        private string connectionString;

        public override List<Domain> GetDomains()
        {
            var _tmp = new List<Domain>();

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT HostDomain.DomainName, Account.AccountNumber, HostDomain.DomainId, HostDomain.DomainStatus, HostDomain.SubDomainId, HostDomain.DomainAliasId
                                                            FROM Account INNER JOIN
                                                                                  Package ON Account.AccountNumber = Package.AccountNumber RIGHT OUTER JOIN
                                                                                  HostDomain ON Package.PackageId = HostDomain.PackageId
                                                            WHERE (HostDomain.SubDomainId = 0) AND (HostDomain.DomainAliasId = 0)", _conn))
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

                            var currentFtpUser = GetFirstFtpAccount(_d.Id);
                            _d.Username = String.IsNullOrEmpty(currentFtpUser) ? _d.Name : currentFtpUser;
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
            var _tmp = new List<Email>();

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT POP3Account.POP3UserName, POP3Account.ForwardsTo, HostDomain.DomainName
                                                            FROM POP3Account INNER JOIN
                                                                HostDomain ON POP3Account.DomainId = HostDomain.DomainId 
                                                            WHERE (HostDomain.DomainName = @NAME)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var da = new Email();
                            da.DomainName = domainName;
                            da.Name = DataExtensions.GetColumnValue<string>(_read, "POP3UserName");
                            da.Password = DataHelper.GetPassword();
                            da.Quota = -1;

                            var forwardEmail = DataExtensions.GetColumnValue<string>(_read, "ForwardsTo");

                            da.Redirect = String.IsNullOrEmpty(forwardEmail) ? "false" : "true";
                            da.RedirectedEmail = forwardEmail;

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

                using (SqlCommand _cmd = new SqlCommand(@"SELECT LimitProperty.LimitPropertyName, Limit.LimitValue, Limit.LimitId, Limit.LevelId, Limit.ItemId, HostDomain.DomainName
                                                            FROM Limit INNER JOIN
                                                          LimitProperty ON Limit.LimitPropertyId = LimitProperty.LimitPropertyId INNER JOIN
                                                          Package ON Limit.ItemId = Package.PackageTypeId INNER JOIN
                                                          HostDomain ON Package.PackageId = HostDomain.PackageId
                                                        WHERE (HostDomain.DomainName = @NAME)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new LimitRow();

                            _d.Name = DataExtensions.GetColumnValue<String>(_read, "LimitPropertyName");
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
            var _tmp = new DnsZone();
            _tmp.Name = domainName;

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT DomainName FROM HostDomain 
                                                                WHERE (SubDomainId = 0) AND 
                                                                    (DomainAliasId = 0) AND 
                                                                    (DomainName = @NAME)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (SqlDataReader _read = _cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                    {
                        if (_read.Read())
                        {
                            _tmp.Name = domainName;
                            _tmp.mininum = 3600;
                            _tmp.refresh = 36000;
                            _tmp.retry = 600;
                            _tmp.expire = 86400;
                            _tmp.ttl = 3600;

                            _tmp.serial = int.Parse(DateTime.Now.ToString("yyyyMMddmm"));                            
                            _tmp.Email = String.Format("hostmaster.{0}", domainName);
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

                using (SqlCommand _cmd = new SqlCommand(@"SELECT HostDomain.DomainName, 
                                                            CASE WHEN DNSRecord.RecordType = 3 
                                                            THEN 'A' WHEN DNSRecord.RecordType = 5 
                                                            THEN 'CNAME' WHEN DNSRecord.RecordType = 4 
                                                            THEN 'MX' WHEN DNSRecord.RecordType = 6 
                                                            THEN 'CNAME'END AS RecordType, 
                                                                DNSRecord.RecordName, DNSRecord.RecordData, DNSRecord.RecordPreference
                                                        FROM  DNSRecord INNER JOIN HostDomain ON DNSRecord.DomainId = HostDomain.DomainId 
                                                            WHERE (HostDomain.DomainName = @NAME)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var da = new DnsZoneRecord();
                            da.name = DataExtensions.GetColumnValue<string>(_read, "RecordName");
                            da.type = DataExtensions.GetColumnValue<string>(_read, "RecordType");
                            da.value = DataExtensions.GetColumnValue<string>(_read, "RecordData");
                            da.priority = DataExtensions.GetColumnValue<int>(_read, "RecordPreference");

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
            return new Forwarding();
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
            var _tmp = new List<Subdomain>();

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT   HostDomain_1.DomainName AS Name
                                                        FROM HostDomain INNER JOIN
                                                    HostDomain AS HostDomain_1 ON HostDomain.DomainId = HostDomain_1.SubDomainId
                                                    WHERE (HostDomain.DomainName = @NAME)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var currentFtpUser = GetFirstFtpAccount(domainName);

                            var da = new Subdomain();
                            da.Domain = domainName;
                            da.Login = String.IsNullOrEmpty(currentFtpUser) ? domainName : currentFtpUser;
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

                using (SqlCommand _cmd = new SqlCommand(@"SELECT AccountNumber, AccountStatus, FirstName, LastName, CompanyName, 
                                                            AccountPassword, PrimaryEmail, SecondaryEmail, Address1, Address2,
                                                            Town, County, CountryCode, PostCode, Address3, 
                                                            AccountType, HomePhone, WorkPhone, MobilePhone, FaxNumber 
                                                        FROM Account WHERE (AccountType <> 0)", _conn))
                {
                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var da = new Reseller();
                            da.Id = 0;
                            da.Username = DataExtensions.GetColumnValue<String>(_read, "AccountNumber");
                            da.Password = DataHelper.GetPassword();
                            da.Address1 = DataExtensions.GetColumnValue<String>(_read, "Address1");
                            da.Address2 = DataExtensions.GetColumnValue<String>(_read, "Address2");
                            da.City = DataExtensions.GetColumnValue<String>(_read, "Town");
                            da.Country = DataExtensions.GetColumnValue<String>(_read, "CountryCode");
                            da.Email = DataExtensions.GetColumnValue<String>(_read, "PrimaryEmail");
                            da.fax = DataExtensions.GetColumnValue<String>(_read, "FaxNumber");
                            da.FirstName = DataExtensions.GetColumnValue<String>(_read, "FirstName");
                            da.LastName = DataExtensions.GetColumnValue<String>(_read, "LastName");
                            da.Organization = DataExtensions.GetColumnValue<String>(_read, "CompanyName");
                            da.Phone = DataExtensions.GetColumnValue<String>(_read, "HomePhone");
                            da.PostalCode = DataExtensions.GetColumnValue<String>(_read, "PostCode");                            
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
            var stats = new PanelStats();

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT COUNT(*) FROM UserDatabase;", _conn))
                {
                    stats.TotalDatabaseCount = GetScalarValue(_cmd.ExecuteScalar());
                    stats.TotalDatabaseDiskSpace = 0;
                }

                using (SqlCommand _cmd = new SqlCommand(@"SELECT COUNT(*) FROM HostDomain WHERE DomainAliasId <> 0", _conn))
                {
                    stats.TotalDomainAliasCount = GetScalarValue(_cmd.ExecuteScalar());
                }

                using (SqlCommand _cmd = new SqlCommand(@"SELECT COUNT(*) FROM HostDomain WHERE DomainAliasId = 0 AND SubDomainId = 0", _conn))
                {
                    stats.TotalDomainCount = GetScalarValue(_cmd.ExecuteScalar());
                }

                using (SqlCommand _cmd = new SqlCommand(@"SELECT SUM(CurrentDiskspaceUsage) FROM HostDomain", _conn))
                {
                    stats.TotalDomainDiskSpace = GetScalarValue(_cmd.ExecuteScalar());
                }

                using (SqlCommand _cmd = new SqlCommand(@"SELECT COUNT(*) FROM POP3Account", _conn))
                {
                    stats.TotalEmailCount = GetScalarValue(_cmd.ExecuteScalar());
                    stats.TotalEmailDiskSpace= 0;
                }

                using (SqlCommand _cmd = new SqlCommand(@"SELECT COUNT(*) FROM Account WHERE AccountType <> 0", _conn))
                {
                    stats.TotalResellerCount = GetScalarValue(_cmd.ExecuteScalar());                    
                }

                using (SqlCommand _cmd = new SqlCommand(@"SELECT COUNT(*) FROM HostDomain WHERE SubDomainId <> 0", _conn))
                {
                    stats.TotalSubdomainCount = GetScalarValue(_cmd.ExecuteScalar());
                    stats.TotalSubdomainDiskSpace = 0;
                }
                
                _conn.Close();
            }

            return stats;
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

                using (SqlCommand _cmd = new SqlCommand(@"SELECT LimitProperty.LimitPropertyName, Limit.LimitValue, Limit.LimitId, Limit.LevelId, Limit.ItemId, HostDomain.DomainName
                                                                FROM Limit INNER JOIN
                                                  LimitProperty ON Limit.LimitPropertyId = LimitProperty.LimitPropertyId INNER JOIN
                                                  Package ON Limit.ItemId = Package.PackageTypeId INNER JOIN
                                                  HostDomain ON Package.PackageId = HostDomain.PackageId WHERE (HostDomain.DomainName = @NAME)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", clientName);

                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var da = new LimitRow();
                            da.Name = DataExtensions.GetColumnValue<String>(_read, "LimitPropertyName");
                            da.Value = DataExtensions.GetColumnValue<int>(_read, "LimitValue");

                            _tmp_limits.Add(da);
                        }
                    }
                }

                _conn.Close();
            }

            var limits = new HostLimit();
            limits.LoadHelmLimits(_tmp_limits);

            return limits;
        }

        public override void LoadConnectionString(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public override bool SecurePasswords()
        {
            return true;
        }


        private string GetFirstFtpAccount(int domainId)
        {
            var ftpUserName = String.Empty;

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT TOP 1 FTPPassword, FTPUserName FROM FTPAccount WHERE DomainId = @ID", _conn))
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

            return ftpUserName;
        }

        private string GetFirstFtpAccount(string domainName)
        {
            var ftpUserName = String.Empty;

            using (SqlConnection _conn = new SqlConnection(connectionString))
            {
                _conn.Open();

                using (SqlCommand _cmd = new SqlCommand(@"SELECT TOP 1 FTPAccount.FTPUserName
                                                                FROM FTPAccount INNER JOIN
                                                            HostDomain ON FTPAccount.DomainId = HostDomain.DomainId
                                                                WHERE (HostDomain.DomainName = @NAME)", _conn))
                {
                    _cmd.Parameters.AddWithValue("@NAME", domainName);

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

            return ftpUserName;
        }
    }
}
