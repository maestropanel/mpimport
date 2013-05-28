
namespace MpMigrate.Data.Entity
{
    using System.Collections.Generic;
    using System;
    using System.Linq;

    public class DomainLimit
    {
        public int DiskSpace { get; set; }
        public int MaxMailBox { get; set; }
        
        public int MaxMySqlDb { get; set; }
        public int MaxMySqlDbSpace { get; set; }

        public int MaxMsSqlDb { get; set; }
        public int MaxMsSqlDbSpace { get; set; }
        
        public int MaxDomainAlias { get; set; }
        public int MaxSubDomain { get; set; }
        public int TotalWebTraffic { get; set; }
        public int TotalMailBoxQuota { get; set; }

        public DateTime Expiration { get; set; }


        public DomainLimit()
        {

        }

        public DomainLimit(List<LimitRow> limitRows)
        {
            DiskSpace = GetLimitValue(limitRows, "disk_space");

            MaxMailBox = GetLimitValue(limitRows, "max_box");

            MaxMySqlDb = GetLimitValue(limitRows, "max_mysql_db");            
            MaxMySqlDbSpace = GetLimitValue(limitRows, "mysql_dbase_space");

            MaxMsSqlDb = GetLimitValue(limitRows, "max_mssql_db");
            MaxMsSqlDbSpace = GetLimitValue(limitRows, "mssql_dbase_space");

            MaxDomainAlias = GetLimitValue(limitRows, "max_dom_aliases");
            MaxSubDomain = GetLimitValue(limitRows, "max_subdom");

            TotalMailBoxQuota = GetLimitValue(limitRows, "total_mboxes_quota");
            TotalWebTraffic = GetLimitValue(limitRows, "max_traffic");
        }

        private int GetLimitValue(List<LimitRow> limitRows, string name)
        {
            int val = 0;

            if (limitRows.Where(m => m.Name == name).Any())
            {
                var limitval = limitRows.FirstOrDefault(m => m.Name == name).Value;
                int.TryParse(limitval, out val);
            }

            return val;
        }
    }

    public struct LimitRow
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
