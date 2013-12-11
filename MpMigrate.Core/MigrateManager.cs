namespace MpMigrate.Core
{
    using MpMigrate.Core.Discovery;
    using MpMigrate.Core.Entity;
    using MpMigrate.Data.Dal;
    using MpMigrate.Data.Entity;
    using MpMigrate.MaestroPanel.Api;
    using MpMigrate.MaestroPanel.Api.Entity;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public enum PanelTypes
    {
        Unknown,
        Plesk_86,
        Plesk_95,
        Plesk_11,
        Plesk_10,
        MaestroPanel,
        Entrenix
    }

    public class ApiAction : EventArgs
    {
        private string domainName;
        private string message;
        private int count;
        private int errorCode;
        private int errorCount;

        public int ErrorCount
        {
            get { return errorCount; }
            set { errorCount = value; }
        }
        
        public int ErrorCode
        {
            get { return errorCode; }
            set { errorCode = value; }
        }        
        
        public int Count
        {
            get { return count; }
            set { count = value; }
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
        private int errorCount;

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
                plan.Destination.UseHttps,
                generatePassword: plan.MiscGeneratePassword);
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
                    var connectionString = SourceDatabase.ConnectionString();                    
                    PanelData.LoadConnectionString(connectionString);

                    break;
                }
            }
        }

        public void Execute()
        {
            var domainList = new List<Domain>();
            var resellerList = new List<Reseller>();                       

            #region Filter
            if (Plan.Filter)
            {
                if (Plan.FilterType == FilterTypes.Domain)
                    domainList = PanelData.GetDomains().Where(m => Plan.FilterDomains.Contains(m.Name)).ToList();

                if (Plan.FilterType == FilterTypes.Reseller)
                {
                    domainList = PanelData.GetDomains().Where(m => Plan.FilterResellers.Contains(m.ClientName)).ToList();
                    resellerList = PanelData.GetResellers().Where(m => Plan.FilterResellers.Contains(m.Username)).ToList();
                }
            }
            else        
            {
                domainList = PanelData.GetDomains();
                resellerList = PanelData.GetResellers();
            }
            #endregion

            #region Reseller
            //Import Reseller
            if (Plan.Resellers)
            {
                foreach (var resItem in resellerList)
                {
                    Action(this,CreateActionMessage("Creating Reseller", resItem.Username));

                    var resellerResult = Api.ResellerCreate(resItem.Username, resItem.Password, Plan.Destination.DefaultPlan,
                        resItem.FirstName, resItem.LastName, resItem.Email, resItem.Country, resItem.Organization, resItem.Address1, resItem.Address2,
                        resItem.City, resItem.Province, resItem.PostalCode, resItem.Phone, resItem.fax);

                    Action(this,CreateActionEventAndLogging(resellerResult));                    
                }
            }
            #endregion

            #region Domains
            foreach (var item in domainList)
            {                
                 totalCount++;

                //Create Domain
                if (Plan.Domains)
                {
                    Action(this, CreateActionMessage("Creating Domain", item.Name));

                    var activeDomainUser = !String.IsNullOrEmpty(item.Password);
                    var domainResult = Api.DomainCreate(item.Name, Plan.Destination.DefaultPlan, item.Username, item.Password, activeDomainUser,
                        "", "", "", item.Expiration);

                    Action(this, CreateActionEventAndLogging(domainResult));
                }

                if (Plan.CopyHttpFiles)
                {

                }

                //Set Frowarding
                if (Plan.Domains && item.isForwarding)
                {
                    Action(this, CreateActionMessage("Enable Forwarding...", item.Name));

                    var forwardResult = Api.SetForwarding(item.Name, true, item.ForwardUrl, true, false, "Found");
                    Action(this, CreateActionEventAndLogging(forwardResult));
                }
                
                //Create Subdomains
                if (Plan.Subdomains)
                {
                    foreach (var subItem in item.Subdomains)
                    {
                        Action(this, CreateActionMessage("Creating Subdomain: " + subItem.Name, item.Name));

                        var subdomainResult = Api.AddSubDomain(item.Name, subItem.Name, subItem.Login, subItem.Password);
                        Action(this, CreateActionEventAndLogging(subdomainResult));

                        if (Plan.CopyHttpFiles && subdomainResult.ErrorCode == 0)
                        {

                        }
                    }
                }

                //Create Aliases
                if (Plan.DomainAliases)
                {
                    foreach (var aliasItem in item.Aliases)
                    {
                        Action(this, CreateActionMessage("Creating Domain Alias: " + aliasItem.Alias, item.Name));

                        var aliasResult = Api.AddAlias(item.Name, aliasItem.Alias);
                        Action(this, CreateActionEventAndLogging(aliasResult));
                    }
                }

                if (Plan.Emails)
                {
                    foreach (var mailItem in item.Emails)
                    {
                        Action(this, CreateActionMessage("Creating MailBox: " + mailItem.Name, item.Name));

                        var addMailboxResult = Api.AddMailBox(item.Name, mailItem.Name, mailItem.Password, mailItem.Quota, mailItem.Redirect, mailItem.RedirectedEmail);
                        Action(this, CreateActionEventAndLogging(addMailboxResult));

                        if (Plan.CopyEmailFiles && addMailboxResult.ErrorCode == 0)
                        {

                        }
                    }
                }

                //Create Database
                if (Plan.Databases)
                {
                    foreach (var dbItem in item.Databases)
                    {
                        Action(this, CreateActionMessage("Creating Database: " + dbItem.Name, item.Name));

                        var dbResult = Api.AddDatabase(item.Name, dbItem.DbType, dbItem.Name, -1);
                        Action(this, CreateActionEventAndLogging(dbResult));

                        //Add DB Users
                        if (dbResult.ErrorCode == 0)
                        {
                            foreach (var dbUserItem in dbItem.Users)
                            {
                                Action(this, CreateActionMessage(String.Format("Creating Database User {0} on {1} ", dbUserItem.Username, dbItem.Name), item.Name));

                                var userResult = Api.AddDatabaseUser(item.Name, dbItem.DbType, dbItem.Name, dbUserItem.Username, dbUserItem.Password);
                                Action(this, CreateActionEventAndLogging(userResult));
                            }
                        }

                        if (Plan.CopyDatabase)
                        {

                        }
                    }
                }

                if (Plan.DnsRecords)
                {

                    Action(this, CreateActionMessage("Build Dns Zone...", item.Name));

                    var zoneRecord = item.Zone.Records
                        .Select(m => new DnsZoneRecordItem(){ name = m.name, type  = m.type, value = m.value, priority = m.priority}).ToList();

                    var dnsResult = Api.SetDnsZone(item.Name, item.Zone.expire, item.Zone.ttl, item.Zone.refresh, item.Zone.Email, item.Zone.retry,
                        item.Zone.serial, "", zoneRecord);

                    Action(this, CreateActionEventAndLogging(dnsResult));
                }

                //Set Limits
                if (Plan.HostLimits)
                {
                    Action(this, CreateActionMessage("Setting Host Limits...", item.Name));

                    var limitResult = Api.SetLimits(item.Name,
                                                    item.Limits.DiskSpace,
                                                    item.Limits.MaxMailBox,
                                                    item.Limits.MaxFtpUser,
                                                    item.Limits.MaxSubDomain,
                                                    item.Limits.MaxDomainAlias,
                                                    item.Limits.MaxWebTraffic,
                                                    item.Limits.TotalMailBoxQuota,
                                                    item.Limits.MaxWebTraffic,
                                                    item.Limits.MaxFtpTraffic,
                                                    item.Limits.MaxMailTraffic,
                                                    item.Limits.MaxMySqlDb,
                                                    item.Limits.MaxMySqlUser,
                                                    item.Limits.MaxMySqlDbSpace,
                                                    item.Limits.MaxMsSqlDb,
                                                    item.Limits.MaxMsSqlDbUser,
                                                    item.Limits.MaxMsSqlDbSpace);

                    Action(this, CreateActionEventAndLogging(limitResult));
                }

                //Move to reseller.
                if (Plan.Resellers && Plan.Domains)
                {
                    Action(this, CreateActionMessage("Setting Reseller...", item.Name));
                    var restoreOwner = Api.ChangeReseller(item.Name, item.ClientName);
                    Action(this, CreateActionEventAndLogging(restoreOwner));
                }
            }

            #endregion

            #region Copy Reseller Limits
            //Copy Reseller Limits
            if (Plan.Resellers)
            {
                if (Plan.ResellerLimits)
                {
                    

                    foreach (var resItem in resellerList)
                    {
                        Action(this, CreateActionMessage("Setting Reseller Limits...", resItem.Username));

                        var resellerLimitResult = Api.ResellerSetLimit(
                            resItem.Username,
                            resItem.Limits.MaxDomain,
                            resItem.Limits.DiskSpace,
                            resItem.Limits.MaxMailBox,
                            resItem.Limits.MaxFtpUser,
                            resItem.Limits.MaxSubDomain,
                            resItem.Limits.MaxDomainAlias,
                            resItem.Limits.MaxWebTraffic,
                            resItem.Limits.TotalMailBoxQuota,
                            resItem.Limits.MaxWebTraffic,
                            resItem.Limits.MaxFtpTraffic,
                            resItem.Limits.MaxMailTraffic,
                            resItem.Limits.MaxMySqlDb,
                            resItem.Limits.MaxMySqlUser,
                            resItem.Limits.MaxMsSqlDbSpace,
                            resItem.Limits.MaxMsSqlDb,
                            resItem.Limits.MaxMsSqlDbUser,
                            resItem.Limits.MaxMsSqlDbSpace);

                        Action(this, CreateActionEventAndLogging(resellerLimitResult));
                    }
                }
            }
            #endregion
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

            variationList.Add(new Tuple<PanelTypes, DatabaseProviders, DboFactory, IDiscovery>
                                (PanelTypes.Plesk_86, DatabaseProviders.ACCESS, new Plesk_86_Access(), new Plesk_86_Discover()));

            variationList.Add(new Tuple<PanelTypes, DatabaseProviders, DboFactory, IDiscovery> 
                            (PanelTypes.Plesk_95, DatabaseProviders.ACCESS, new Plesk_9_Access(), new Plesk_9_Discover()));

            variationList.Add(new Tuple<PanelTypes, DatabaseProviders, DboFactory, IDiscovery>
                                (PanelTypes.Plesk_11, DatabaseProviders.MYSQL, new Plesk_11_MySql(), new Plesk_11_Discover()));

            variationList.Add(new Tuple<PanelTypes, DatabaseProviders, DboFactory, IDiscovery>
                                (PanelTypes.Plesk_10, DatabaseProviders.MYSQL, new Plesk_10_MySql(), new Plesk_10_Discover()));

            variationList.Add(new Tuple<PanelTypes, DatabaseProviders, DboFactory, IDiscovery>
                                (PanelTypes.Entrenix, DatabaseProviders.ACCESS_ODBC, new Entrenix_Access(), new Entrenix_Discover()));            
        }

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

        private ApiAction CreateActionEventAndLogging(ApiResult<DomainOperationsResult> action)
        {
            if (action.ErrorCode != 0)
                errorCount++;

            if (action == null)
                return new ApiAction() { Count = totalCount, ErrorCode = -1, DomainName = "Unknown", ErrorCount = errorCount, Message = "Action is null" };

            return new ApiAction()
            {                
                Count = totalCount,
                DomainName = action.Details != null ? action.Details.Name : "",
                ErrorCode = action.ErrorCode,
                Message = action.Message,
                ErrorCount = errorCount
            };
        }

        private ApiAction CreateActionEventAndLogging(ApiResult<ResellerOperationResult> action)
        {
            if (action.ErrorCode != 0)
                errorCount++;            

            return new ApiAction()
            {
                Count = totalCount,
                DomainName = action.Details != null ? action.Details.ClientName : "",
                ErrorCode = action.ErrorCode,
                Message = action.Message,
                ErrorCount = errorCount
            };
        }

        private ApiAction CreateActionMessage(string message, string clientOrDomain)
        {

            return new ApiAction()
            {
                Count = totalCount,
                DomainName = clientOrDomain,
                ErrorCode = 0,
                Message = message,
                ErrorCount = errorCount
            };
        }
    }
}
