namespace MpMigrate.Data.Entity
{
    public class PanelStats
    {
        public long TotalDomainCount { get; set; }
        public long TotalEmailCount { get; set; }
        public long TotalResellerCount { get; set; }
        public long TotalDatabaseCount { get; set; }
        public long TotalSubdomainCount { get; set; }
        public long TotalDomainAliasCount { get; set; }

        public decimal TotalDomainDiskSpace { get; set; }
        public decimal TotalEmailDiskSpace { get; set; }
        public decimal TotalDatabaseDiskSpace { get; set; }
        public decimal TotalSubdomainDiskSpace { get; set; }

        public decimal DomainDiscSpaceToGB()
        {
            return DiskSpaceToGB(TotalDomainDiskSpace);
        }

        public decimal EmailDiskSpaceToGB()
        {
            return DiskSpaceToGB(TotalEmailDiskSpace);
        }

        public decimal DatabaseDiskSpaceToGB()
        {
            return DiskSpaceToGB(TotalDatabaseDiskSpace);
        }

        public decimal SubdomainDiskSpaceToGB()
        {
            return DiskSpaceToGB(TotalSubdomainDiskSpace);
        }


        private decimal DiskSpaceToGB(decimal space)
        {
            return (((space / 1024) / 1024) /1024);
        }

    }
}
