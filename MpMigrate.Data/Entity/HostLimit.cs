
namespace MpMigrate.Data.Entity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class HostLimit
    {
        public int MaxDomain { get; set; }
        public int DiskSpace { get; set; }
        public int MaxMailBox { get; set; }
        public int MaxFtpUser { get; set; }
        public int MaxFtpTraffic { get; set; }
        public int MaxMailTraffic { get; set; }
        
        public int MaxMySqlDb { get; set; }
        public int MaxMySqlDbSpace { get; set; }
        public int MaxMySqlUser { get; set; }

        public int MaxMsSqlDb { get; set; }
        public int MaxMsSqlDbSpace { get; set; }
        public int MaxMsSqlDbUser { get; set; }
        
        public int MaxDomainAlias { get; set; }
        public int MaxSubDomain { get; set; }


        public int MaxWebTraffic { get; set; }
        public int TotalMailBoxQuota { get; set; }

        public DateTime Expiration { get; set; }

        public HostLimit()
        {

        }

        public HostLimit(List<LimitRow> limitRows)
        {
            MaxDomain = GetLimitValue(limitRows, "max_dom");
            MaxWebTraffic = GetLimitValue(limitRows, "max_traffic");

            DiskSpace = GetLimitValue(limitRows, "disk_space");
            MaxFtpUser = GetLimitValue(limitRows, "max_subftp_users");
            MaxDomainAlias = GetLimitValue(limitRows, "max_dom_aliases");
            MaxSubDomain = GetLimitValue(limitRows, "max_subdom");

            MaxMailBox = GetLimitValue(limitRows, "max_box");
            TotalMailBoxQuota = GetLimitValue(limitRows, "total_mboxes_quota");

            MaxMySqlDb = GetLimitValue(limitRows, "max_db");
            MaxMySqlDbSpace = GetLimitValue(limitRows, "mysql_dbase_space");
            MaxMySqlUser = -1;

            MaxMsSqlDb = GetLimitValue(limitRows, "max_mssql_db");
            MaxMsSqlDbSpace = GetLimitValue(limitRows, "mssql_dbase_space");
            MaxMsSqlDbUser = -1;
            MaxFtpTraffic = -1;
            MaxMailTraffic = -1;
        }

        public HostLimit(List<LimitRow> limitRows, bool isMaestroPanel)
        {
            MaxDomain = GetLimitValue(limitRows, "DOMAINQUOTA");
            MaxWebTraffic = GetLimitValue(limitRows, "IIS_LIMIT_TRAFFIC");

            DiskSpace = GetLimitValue(limitRows, "MSFTP_FTPQUOTA");
            MaxFtpUser = GetLimitValue(limitRows, "MSFTP_FTPACCOUNTCOUNT");
            MaxDomainAlias = GetLimitValue(limitRows, "DOMAIN_ALIAS_LIMIT");
            MaxSubDomain = GetLimitValue(limitRows, "IISSUBDOMAINLIMITI");

            MaxMailBox = GetLimitValue(limitRows, "MAILBOXCOUNT");
            TotalMailBoxQuota = GetLimitValue(limitRows, "MAILBOXSIZEQUOTA");

            MaxMySqlDb = GetLimitValue(limitRows, "MYSQLDATABASECOUNT");
            MaxMySqlDbSpace = GetLimitValue(limitRows, "MYSQLDATABASESIZE");
            MaxMySqlUser = GetLimitValue(limitRows, "MYSQLDATABASEUSERCOUNT");

            MaxMsSqlDb = GetLimitValue(limitRows, "max_mssql_db");
            MaxMsSqlDbSpace = GetLimitValue(limitRows, "MSSQLDATABASESIZE");
            MaxMsSqlDbUser = GetLimitValue(limitRows, "MSSQLDATABASEUSERCOUNT");

            MaxFtpTraffic = GetLimitValue(limitRows, "FTP_TRAFFIC_LIMIT");
            MaxMailTraffic = GetLimitValue(limitRows, "MAILENABLE_TRAFFIC_LIMIT");
        }

        public HostLimit(List<LimitRow> limitRows, int WebSitePanel)
        {            
            MaxDomain = GetLimitValue(limitRows, "OS.Domains");
            MaxWebTraffic = GetLimitValue(limitRows, "OS.Bandwidth");

            DiskSpace = GetLimitValue(limitRows, "OS.Diskspace");
            MaxFtpUser = GetLimitValue(limitRows, "FTP.Accounts");
            MaxDomainAlias = GetLimitValue(limitRows, "OS.DomainPointers");
            MaxSubDomain = GetLimitValue(limitRows, "OS.SubDomains");
            
            MaxMailBox = GetLimitValue(limitRows, "Mail.Accounts");
            TotalMailBoxQuota = GetLimitValue(limitRows, "Mail.MaxBoxSize");

            if (isExists(limitRows, "MySQL4.Databases"))
            {
                MaxMySqlDb = GetLimitValue(limitRows, "MySQL4.Databases");
                MaxMySqlDbSpace = GetLimitValue(limitRows, "MySQL4.MaxDatabaseSize");
                MaxMySqlUser = GetLimitValue(limitRows, "MySQL4.Users");
            }

            if (isExists(limitRows, "MySQL5.Databases"))
            {
                MaxMySqlDb = GetLimitValue(limitRows, "MySQL5.Databases");
                MaxMySqlDbSpace = GetLimitValue(limitRows, "MySQL5.MaxDatabaseSize");
                MaxMySqlUser = GetLimitValue(limitRows, "MySQL5.Users");
            }

            if (isExists(limitRows, "MsSQL2000.Databases"))
            {
                MaxMsSqlDb = GetLimitValue(limitRows, "MsSQL2000.Databases");
                MaxMsSqlDbSpace = GetLimitValue(limitRows, "MsSQL2000.MaxDatabaseSize");
                MaxMsSqlDbUser = GetLimitValue(limitRows, "MsSQL2000.Users");
            }
            
            if (isExists(limitRows, "MsSQL2005.Databases"))
            {
                MaxMsSqlDb = GetLimitValue(limitRows, "MsSQL2005.Databases");
                MaxMsSqlDbSpace = GetLimitValue(limitRows, "MsSQL2005.MaxDatabaseSize");
                MaxMsSqlDbUser = GetLimitValue(limitRows, "MsSQL2005.Users");
            }
            
            if (isExists(limitRows, "MsSQL2008.Databases"))
            {
                MaxMsSqlDb = GetLimitValue(limitRows, "MsSQL2008.Databases");
                MaxMsSqlDbSpace = GetLimitValue(limitRows, "MsSQL2008.MaxDatabaseSize");
                MaxMsSqlDbUser = GetLimitValue(limitRows, "MsSQL2008.Users");
            }
            
            if (isExists(limitRows, "MsSQL2012.Databases"))
            {
                MaxMsSqlDb = GetLimitValue(limitRows, "MsSQL2012.Databases");
                MaxMsSqlDbSpace = GetLimitValue(limitRows, "MsSQL2012.MaxDatabaseSize");
                MaxMsSqlDbUser = GetLimitValue(limitRows, "MsSQL2012.Users");
            }

            MaxFtpTraffic = -1;
            MaxMailTraffic = -1;
        }

        private bool isExists(List<LimitRow> limitRows, string key)
        {
            return limitRows.Where(m => m.Name == key).Any();
        }

        public void LoadHelmLimits(List<LimitRow> limitRows)
        {
            MaxDomain = GetLimitValue(limitRows, "MaxNumDomains");
            MaxWebTraffic = GetLimitValue(limitRows, "MaxBandwidth");

            DiskSpace = GetLimitValue(limitRows, "MaxDiskSpace");
            MaxFtpUser = GetLimitValue(limitRows, "MaxFTP");
            MaxDomainAlias = GetLimitValue(limitRows, "MaxDomainAliases");
            MaxSubDomain = GetLimitValue(limitRows, "MaxSubDomains");

            MaxMailBox = GetLimitValue(limitRows, "MaxPOP3");
            TotalMailBoxQuota = GetLimitValue(limitRows, "MaxDiskSpace");

            MaxMySqlDb = GetLimitValue(limitRows, "MaxMSSQL2KDBs");
            MaxMySqlDbSpace = GetLimitValue(limitRows, "MaxDiskSpace");
            MaxMySqlUser = GetLimitValue(limitRows, "MaxDBUsers");

            MaxMsSqlDb = GetLimitValue(limitRows, "MaxMSSQL2KDBs");
            MaxMsSqlDbSpace = GetLimitValue(limitRows, "MaxDiskSpace");
            MaxMsSqlDbUser = GetLimitValue(limitRows, "MaxDBUsers");

            MaxFtpTraffic = GetLimitValue(limitRows, "MaxBandwidth");
            MaxMailTraffic = GetLimitValue(limitRows, "MaxBandwidth");
        }

        private int GetLimitValue(List<LimitRow> limitRows, string name)
        {
            int val = -1;

            if (limitRows == null)
                return val;

            if (limitRows.Where(m => m.Name == name).Any())
            {
                var _value = limitRows.FirstOrDefault(m => m.Name == name);               
                val = _value.Value;
            }

            return val;
        }
    }

    public struct LimitRow
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }
}
