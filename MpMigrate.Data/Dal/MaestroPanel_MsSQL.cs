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

                using (SqlCommand _cmd = new SqlCommand(@"SELECT Domain.DomainId, 
                                                                        Domain.Name, Domain.ExpirationDate, Domain.Status,
                                                                    DomainMsFTPUser.Username as ftpUserName, DomainMsFTPUser.Password as FtpPassword, 
                                                                    LoginAccount.UserName as DomainUser, LoginAccount.Password as DomainPassword
                                                                FROM Domain 
                                                                    LEFT JOIN DomainMsFtp ON DomainMsFtp.DomainId = Domain.DomainId
                                                                    LEFT JOIN DomainMsFTPUser ON DomainMsFTPUser.FtpId = DomainMsFtp.Id
                                                                    LEFT JOIN LoginAccount ON LoginAccount.LoginId = Domain.OwnerLoginId 
                                                                AND LoginAccount.UserName <> 'admin'", _conn))
                {
                    using (SqlDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new Domain();
                            _d.Id = Convert.ToInt32(DataExtensions.GetColumnValue<uint>(_read, "DomainId"));
                            _d.Name = DataExtensions.GetColumnValue<String>(_read, "Name").ToLower();
                            _d.ClientName = DataExtensions.GetColumnValue<String>(_read, "DomainUser");
                            _d.DomainPassword = DataExtensions.GetColumnValue<String>(_read, "DomainPass");
                            _d.Username = DataExtensions.GetColumnValue<String>(_read, "fp_adm");
                            _d.Password = DataExtensions.GetColumnValue<String>(_read, "password");
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public override List<Database> GetDatabases(string domainName)
        {
            throw new NotImplementedException();
        }

        public override List<DatabaseUser> GetDatabaseUsers(int database_id)
        {
            throw new NotImplementedException();
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
    }
}
