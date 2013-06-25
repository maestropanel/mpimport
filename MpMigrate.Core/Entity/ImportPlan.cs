using System.Collections.Generic;
namespace MpMigrate.Core.Entity
{
    public class ImportPlan
    {
        public bool Domains { get; set; }
        public bool Resellers { get; set; }
        public bool Emails { get; set; }
        public bool Subdomains { get; set; }
        public bool DnsRecords { get; set; }
        public bool HostLimits { get; set; }
        public bool DomainAliases { get; set; }
        public bool Databases { get; set; }

        public CopyFileMethods CopyMethod { get; set; }

        public bool CopyHttpFiles { get; set; }
        public bool CopyEmailFiles { get; set; }
        public bool CopyDatabase { get; set; }

        public bool DeletePackageAfterMoving { get; set; }

        public bool Filter { get; set; }

        public FilterTypes FilterType { get; set; }
        public List<string> FilterDomains { get; set; }
        public List<string> FilterResellers { get; set; }
        
        public SourceHost Source { get; set; }
        public MaestroPanelHost Destination { get; set; }

        public bool DatabaseConnectionTest { get; set; }
        public bool ApiConnectionTest { get; set; }
        public bool UncConnectionTest { get; set; }

        public ImportPlan()
        {
            Source = new SourceHost();
            Destination = new MaestroPanelHost();
            FilterDomains = new List<string>();
            FilterResellers = new List<string>();
        }
    }


    public enum CopyFileMethods
    {
        Raw,
        Zip
    }

    public enum FilterTypes
    {
        Domain,
        Reseller
    }
}
