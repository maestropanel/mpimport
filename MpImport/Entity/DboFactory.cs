namespace PleskImport.Entity
{
    using System.Collections.Generic;

    public abstract class DboFactory
    {
        public abstract List<Domain> GetDomains();
        public abstract List<Email> GetEmails(string domainName);
        public abstract List<Database> GetDatabases(string domainName);
        public abstract List<Subdomain> GetSubdomains(string domainName);
        public abstract List<DomainAlias> GetDomainAliases(string domainName);
    }
}
