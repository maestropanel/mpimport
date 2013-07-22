namespace MpMigrate.Data.Entity
{
    using System;
    using System.Collections.Generic;

    public class Domain
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ClientName { get; set; }        
        public string DomainPassword { get; set; }
        public long Status { get; set; }
        public DateTime? Expiration { get; set; }

        public HostLimit Limits { get; set; }
        public List<DomainAlias> Aliases { get; set; }
        public List<Subdomain> Subdomains { get; set; }
        public List<Email> Emails { get; set; }
        public List<Database> Databases { get; set; }

        public DnsZone Zone { get; set; }
        public bool isForwarding { get; set; }
        public string ForwardUrl { get; set; }


        public Domain()
        {
            
            Aliases = new List<DomainAlias>();
            Subdomains = new List<Subdomain>();
            Emails = new List<Email>();
        }

        public DateTime FromUnixTime(double unixTime)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTime);
        }

        public DateTime FromUnixTime(string unixTime)
        {
            double d;
            
            if (!double.TryParse(unixTime, out d))            
                return FromUnixTime(0D);
            
            return FromUnixTime(d);
        }
    }
}
