namespace MpMigrate.Data.Entity
{
    using System.Collections.Generic;

    public class DnsZone
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public int ttl { get; set; }
        public int refresh { get; set; }
        public int retry { get; set; }
        public int expire { get; set; }
        public int mininum { get; set; }
        public int serial { get; set; }

        public List<DnsZoneRecord> Records { get; set; }

        public DnsZone()
        {
            Records = new List<DnsZoneRecord>();
        }
    }

    public class DnsZoneRecord
    {
        public string type { get; set; }
        public string name { get; set; }
        public string value { get; set; }
        public int priority { get; set; }
    }
}
