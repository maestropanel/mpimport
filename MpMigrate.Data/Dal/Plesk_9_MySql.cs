namespace MpMigrate.Data.Dal
{
    using MpMigrate.Data.Entity;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Plesk_9_MySql : DboFactory
    {
        private string connectionString;

        public override List<Domain> GetDomains()
        {
            throw new NotImplementedException();
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
