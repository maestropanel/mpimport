namespace MpMigrate.Core
{
    using MpMigrate.Core.Discovery;
    using MpMigrate.Core.Entity;
    using MpMigrate.Data.Dal;
    using MpMigrate.Data.Entity;
    using MpMigrate.MaestroPanel.Api;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.IO;

    public enum PanelTypes
    {
        Unknown,
        Plesk_86,
        Plesk_95,
        Plesk_11,
        Plesk_10,
        MaestroPanel
    }

    public class ApiAction : EventArgs
    {
        private string domainName;
        private string message;
        private int count;
        private ApiResult _apiResult;

        public int Count
        {
            get { return count; }
            set { count = value; }
        }
                
        public ApiResult ApiResult
        {
            get { return _apiResult; }
            set { _apiResult = value; }
        }

        public string DomainName
        {
            get { return domainName; }
            set { domainName = value; }
        }

        public string Message
        {
            get { return message; }
            set { message = value; }
        }
    }

    public class MigrateManager
    {
        public delegate void ApiActionHandler(MigrateManager m, ApiAction e);
        public event ApiActionHandler Action;

        public ImportPlan Plan { get; set; }
        public ApiClient Api { get; set; }

        public PanelTypes PanelType { get; set; }
        public IDiscovery CurrentPanel { get; set; }
        public DboFactory PanelData { get; set; }
        public PanelDatabase SourceDatabase { get; set; }

        private List<Tuple<PanelTypes, DatabaseProviders, DboFactory, IDiscovery>> variationList;

        private int totalCount;

        public MigrateManager()
        {
            Plan = new ImportPlan();
            SourceDatabase = new PanelDatabase();
            LoadVariations();            
        }

        public MigrateManager(ImportPlan plan)
        {
            Plan = plan;
            SourceDatabase = new PanelDatabase();
            LoadVariations();

            Api = new ApiClient(plan.Destination.ApiKey,
                plan.Destination.ApiHost,
                plan.Destination.ApiPort,
                plan.Destination.UseHttps);
        }

        public void DetermineInstalledPanel()
        {
            PanelType = PanelTypes.Unknown;
            SourceDatabase.Provider = DatabaseProviders.Unknown;

            foreach (var item in variationList)
            {
                var installed = item.Item4.isInstalled();
                var dbProvider = item.Item4.GetDatabaseProvider();

                if (installed && (item.Item2 == dbProvider))
                {
                    PanelType = item.Item1;
                    SourceDatabase.Provider = item.Item2;
                    CurrentPanel = item.Item4;
                    SetSourceDatabaseAutomatically(item.Item4);

                    PanelData = item.Item3;                    
                    PanelData.LoadConnectionString(SourceDatabase.ConnectionString());

                    break;
                }
            }
        }

        public void Execute()
        {
            foreach (var item in PanelData.GetDomains())
            {
                totalCount++;
                
                var action = new ApiAction();
                action.DomainName = item.Name;
                action.Count = totalCount;

                //DEBUG
                action.ApiResult = new ApiResult() { Code = 0, Message = "Oppa" };                
                System.Threading.Thread.Sleep(1000);

                Action(this, action);
                //DEBUG

                if (Plan.Domains)
                {
                    //var activeDomainUser = !String.IsNullOrEmpty(item.Password);
                    //var domainResult = Api.DomainCreate(item.Name, Plan.Destination.DefaultPlan, item.Username, item.Password, activeDomainUser,
                    //    "", "", "", item.Expiration);

                    //action.ApiResult = domainResult;                    
                    //Action(this, action);
                }


                if (item.isForwarding)
                {

                }

                if (Plan.CopyHttpFiles)
                {

                }


                if (Plan.HostLimits)
                {
                        
                }

                if (Plan.Subdomains)
                {
                    if (Plan.CopyHttpFiles)
                    {

                    }
                }

                if (Plan.DomainAliases)
                {

                }

                if (Plan.Emails)
                {
                    if (Plan.CopyEmailFiles)
                    {

                    }

                }

                if (Plan.Databases)
                {
                    if (Plan.CopyDatabase)
                    {

                    }
                }

                if (Plan.DnsRecords)
                {

                }                
            }
        }

        public void LoadInstalledPanel(PanelTypes panelType, PanelDatabase sourceDatabase)
        {
            var variation = variationList.Where(m => m.Item1 == panelType && m.Item2 == sourceDatabase.Provider).FirstOrDefault();
            if (variation != null)
            {
                PanelType = variation.Item1;                
                CurrentPanel = variation.Item4;

                PanelData = variation.Item3;
                PanelData.LoadConnectionString(sourceDatabase.ConnectionString());
            }
            else
            {
                throw new Exception(String.Format("Panel not supported: {0} - {1}", panelType, sourceDatabase.Provider));
            }                
        }  

        private void LoadVariations()
        {
            if (variationList != null)
                return;

            variationList = new List<Tuple<PanelTypes, DatabaseProviders, DboFactory, IDiscovery>>();

            variationList.Add(new Tuple<PanelTypes, DatabaseProviders, DboFactory, IDiscovery>
                                (PanelTypes.Plesk_86, DatabaseProviders.MYSQL, new Plesk_86_MySql(), new Plesk_86_Discover()));            
        }

        //private PanelTypes DetectPlesk()
        //{
        //    PanelTypes plesk_Panel = PanelTypes.Unknown;

        //    var check_environment = Environment.GetEnvironmentVariable("plesk_dir", EnvironmentVariableTarget.Machine);

        //    if (Directory.Exists(check_environment))
        //    {
        //        var plesk_version = CoreHelper.GetRegistryKeyValue(@"SOFTWARE\PLESK\PSA Config\Config", "PRODUCT_VERSION");

        //        if (plesk_version.StartsWith("8.6"))
        //        {
        //            plesk_Panel = PanelTypes.Plesk_86;
        //            CurrentPanel = new Plesk_86_Discover();
        //            SetSourceDatabase(CurrentPanel);

        //            PanelData = new Plesk_86_MySql(SourceDatabase.ConnectionString());
        //        }
        //        else if (plesk_version.StartsWith("9.5"))
        //        {
        //            plesk_Panel = PanelTypes.Plesk_95;
        //        }
        //        else
        //            plesk_Panel = PanelTypes.Unknown;
        //    }
        //    else
        //    {
        //        plesk_Panel = PanelTypes.Unknown;
        //    }

        //    return plesk_Panel;
        //}

        //private PanelTypes DetectMaestroPanel()
        //{
        //    var check_environment = Environment.GetEnvironmentVariable("MaestroPanelPath", EnvironmentVariableTarget.Machine);

        //    if (Directory.Exists(check_environment))
        //    {
        //        return PanelTypes.MaestroPanel;
        //    }
        //    else
        //    {
        //        return PanelTypes.Unknown;
        //    }
        //}

        private void SetSourceDatabaseAutomatically(IDiscovery discover)
        {
            SourceDatabase.Database = discover.GetDatabaseName();
            SourceDatabase.DataseFile = discover.GetDatabaseFile();
            SourceDatabase.Host = discover.GetDatabaseHost();
            SourceDatabase.Password = discover.GetDatabasePassword();
            SourceDatabase.Port = discover.GetDatabasePort();
            SourceDatabase.Provider = discover.GetDatabaseProvider();
            SourceDatabase.Username = discover.GetDatabaseUsername();
        }
    }
}
