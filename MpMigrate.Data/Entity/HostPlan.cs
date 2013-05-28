namespace MpMigrate.Data.Entity
{
    public class HostPlan
    {
        public int cpuUsage { get; set; }
        public int diskSpace { get; set; }
        public int maxTraffic { get; set; }
        public int expiration { get; set; }

        public int mailbox { get; set; }
        public int totalMailBoxQuota { get; set; }
        public int maxRedirection { get; set; }
        public int maxResponders { get; set; }
               
        public int maxMsSqlDb { get; set; }
        public int maxMsSqlDbSpace { get; set; }

        public int maxMySqlDb { get; set; }
        public int maxMySqlDbSpace { get; set; }

        public int maxSubDomain { get; set; }
        public int domainAlias { get; set; }

        public bool classicAsp { get; set; }
        public bool AspDotNet { get; set; }
        public bool cgi { get; set; }
        public bool errorDocs { get; set; }
        public bool ftp { get; set; }
        public bool perl { get; set; }
        public bool php { get; set; }
        public bool phyton { get; set; }
        public bool ssl { get; set; }
    }
}
