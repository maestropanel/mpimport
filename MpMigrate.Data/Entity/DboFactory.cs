namespace MpMigrate.Data.Entity
{
    using System.Collections.Generic;

    public abstract class DboFactory
    {        
        public abstract List<Domain> GetDomains();
        public abstract List<Email> GetEmails(string domainName);
        public abstract HostLimit GetDomainLimits(string domainName);
        public abstract DnsZone GetDnsZone(string domainName);
        public abstract List<DnsZoneRecord> GetZoneRecords(string domainName);
        public abstract Forwarding GetForwarding(string domainName);
        public abstract List<DomainAlias> GetDomainAliases(string domainName);
        public abstract List<Database> GetDatabases(string domainName);
        public abstract List<DatabaseUser> GetDatabaseUsers(int database_id);
        public abstract List<Subdomain> GetSubdomains(string domainName);        
        public abstract List<Reseller> GetResellers();
        public abstract PanelStats GetPanelStats();
        public abstract HostLimit ResellerLimits(string clientName);
        public abstract void LoadConnectionString(string connectionString);
        public abstract bool SecurePasswords();
    }
}
