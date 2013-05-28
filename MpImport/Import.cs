namespace MpMigrate
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using MaestroPanelApi;
    using MpMigrate.Entity;
    using MpMigrate.Properties;
    using MpMigrate.Dbo;
        
    public class Import :IDisposable
    {
        private DboFactory _db;
        private MySqlManager mysqlmgr = new MySqlManager();
        private MpMigrate.SqlManager.MsSqlManager mssqlmgr = new MpMigrate.SqlManager.MsSqlManager();
        private ZipManager zipmgr = new ZipManager();

        private readonly string LOG_FILENAME = "import.log";
        private readonly string FILTER_FILENAME = "filter.txt";

        private string currentDirectory;
        private string mysqlScriptDirectory;
        private string mssqlScriptDirectory;

        private StreamWriter _sb;

        public Import()
        {            
            currentDirectory = Environment.CurrentDirectory;

            mysqlScriptDirectory = Path.Combine("sqlscripts", "mysql");
            mssqlScriptDirectory = Path.Combine("sqlscripts", "mssql");

            if (Settings.Default.exportDatabaseScript)
            {
                if(!Directory.Exists(mysqlScriptDirectory))
                    Directory.CreateDirectory(mysqlScriptDirectory);

                if (!Directory.Exists(mssqlScriptDirectory))
                    Directory.CreateDirectory(mssqlScriptDirectory);
            }
            
            _db = SelectFactory(Settings.Default.dbtype);

            if (_db == null)
                throw new NullReferenceException("Unknown dbtype: "+ Settings.Default.dbtype);
                        
            CreateLogFile();
        }

        public void ImportDomains()
        {            
            var client = new Client(Settings.Default.MaestroPanelApiKey, Settings.Default.MaestroPanelHost, Settings.Default.MaestroPanelPort, false);

            var domains = String.IsNullOrEmpty(Settings.Default.pleskResellerName) ?
                                                    _db.GetDomains() :
                                                    _db.GetDomains().Where(m => m.ClientName == Settings.Default.pleskResellerName);
            
            var domainFilter = GetDomainFilter();

            Console.WriteLine("Domain Filter:"+ domainFilter.Count);

            if (domainFilter.Any())
                domains = domains.Where(m => domainFilter.Contains(m.Name)).ToList();

            foreach (var item in domains)
            {
                var result = Settings.Default.importDomains ?
                    client.DomainCreate(item.Name, Settings.Default.domainPlanName, item.Username, item.Password, !String.IsNullOrEmpty(item.DomainPassword), expiration: item.Expiration) : 
                    new ApiResult() { Code = 0, Message = "Domain Import is Disabled" };


                //Geçici
                //var ftpResult = client.AddFtpUser(item.Name, item.Username, item.Password);
                //PrintAndLog(String.Format("{1} FTP: {0}", ftpResult.Message, item.Name));

                if (result.Code == 0)
                {                    
                    if(Settings.Default.importDomains)
                        PrintAndLog(String.Format("Domain Added: {0}", item.Name));                    

                    if (Settings.Default.importSubdomains)
                        ImportSubdomain(item.Name);

                    if (Settings.Default.importDomainAlias)
                        ImportDomainAlias(item.Name);

                    ImportEmails(item.Name);

                    ImportDatabases(item.Name);

                    if (Settings.Default.CreateDomainPackage)
                        CreateWebSitePackage(item.Name);

                    if (Settings.Default.CopyHttpFiles)
                        CopyHttpFiles(item.Name);
                }
                else
                {                    
                    PrintAndLog(String.Format("Domain Error: {0} - {1}", item.Name, result.Message));
                }
            }
        }

        public void ImportEmails(string domainName)
        {
            var _quota = -1D;            
            
            var client = new Client(Settings.Default.MaestroPanelApiKey, Settings.Default.MaestroPanelHost, Settings.Default.MaestroPanelPort, false);
            var EmailList = _db.GetEmails(domainName);

            foreach (var item in EmailList.Where(m => m.DomainName == domainName))
            {
                if (Settings.Default.importEmails)
                {
                    _quota = item.Quota > 0 ? ((item.Quota / 8) / 1024) : item.Quota;
                    var result = client.AddMailBox(item.DomainName, item.Name, item.Password, item.Quota, item.Redirect, item.RedirectedEmail);

                    if (result.Code == 0)
                        PrintAndLog(String.Format("\tEmail Added: {0}@{1}", item.Name, domainName));
                    else
                        PrintAndLog(String.Format("\tEmail Error: {0}@{2} - {1}", item.Name, result.Message, domainName));
                }

                if (Settings.Default.CopyEmailFiles)
                    CopyEmailFiles(domainName, item.Name);
            }
        }

        public void ImportDatabases(string domainName)
        {
            var client = new Client(Settings.Default.MaestroPanelApiKey, Settings.Default.MaestroPanelHost, Settings.Default.MaestroPanelPort, false);
            var databaseList = _db.GetDatabases(domainName);

            foreach (var item in databaseList)
            {
                if (Settings.Default.importDatabases)
                {
                    var result = client.AddDatabase(item.Domain, item.DbType, item.Name, "", "", -1);

                    if (result.Code == 0)
                    {
                        PrintAndLog(String.Format("\tDatabase Added: {0} ({1})", item.Name, item.DbType));                        
                    }
                    else
                    {
                        PrintAndLog(String.Format("\tDatabase Error: {0} - {1}", item.Name, result.Message));
                    }

                    if (item.Users.Any())
                    {
                        foreach (var user in item.Users)
                        {
                            var createUserResult = client.AddDatabaseUser(domainName, item.DbType, item.Name, user.Username, user.Password);

                            if (createUserResult.Code == 0)
                                PrintAndLog(String.Format("\t\tUser Added: {0}", item.Name));
                            else
                                PrintAndLog(String.Format("\t\tUser Add Error: {0} - {1}", item.Name, createUserResult.Message));
                        }
                    }
                }

                if (Settings.Default.exportDatabaseScript)                
                    ExportDatabase(item);                
            }            
        }

        public void ImportSubdomain(string domainName)
        {            
            var subdomains = _db.GetSubdomains(domainName);
            var client = new Client(Settings.Default.MaestroPanelApiKey, Settings.Default.MaestroPanelHost, Settings.Default.MaestroPanelPort, false);

            foreach (var item in subdomains)
            {
                var subDomainResult = client.AddSubDomain(item.Domain, item.Name, item.Login, item.Password);

                if (subDomainResult.Code == 0)                
                    PrintAndLog(String.Format("\tSubdomain Added: {0}.{1}", item.Name, item.Domain));
                else                
                    PrintAndLog(String.Format("\tSubdomain Error: {0}.{1}", item.Name, subDomainResult.Message));
            }
        }

        public void ImportDomainAlias(string domainName)
        {           
            var aliasList = _db.GetDomainAliases(domainName);
            var client = new Client(Settings.Default.MaestroPanelApiKey, Settings.Default.MaestroPanelHost, Settings.Default.MaestroPanelPort, false);

            foreach (var item in aliasList)
            {
                var subdomainResult = client.AddAlias(item.Domain, item.Alias);

                if (subdomainResult.Code == 0)                
                    PrintAndLog(String.Format("\tDomain Alias Added: {0}", item.Alias));                
                else                
                    PrintAndLog(String.Format("\tDomain Alias Error: {0} - {1}", item.Alias, subdomainResult.Message));                
            }
        }

        public void ImportResellers()
        {
            if (!Settings.Default.ImportReseller)
                return;

            var _resellerList = _db.GetResellers();

            var client = new Client(Settings.Default.MaestroPanelApiKey, Settings.Default.MaestroPanelHost, Settings.Default.MaestroPanelPort, false);

            foreach (var item in _resellerList)
            {
                var result = client.ResellerCreate(item, Settings.Default.domainPlanName);

                if (result.Code == 0)
                    Console.WriteLine(String.Format("Reseller Added: {0}", item.Username));
                else
                    Console.WriteLine(String.Format("Reseller Error: {0} - {1}", item.Username, result.Message));
            }
        }

        private List<string> IncludedDomains()
        {
            var i_domains = new List<string>();
            var IncludeDomainFile = Path.Combine(Environment.CurrentDirectory, "include.txt");

            if (File.Exists(IncludeDomainFile))
                i_domains = File.ReadAllLines(IncludeDomainFile).ToList();

            return i_domains;
        }        

        private void PrintAndLog(string outputText)
        {         
            if(_sb != null)
                _sb.WriteLine(outputText); _sb.Flush();

            Console.WriteLine(outputText);
        }

        private DboFactory SelectFactory(string dbtype)
        {
            var _table = new Dictionary<string, DboFactory>();
            _table.Add("mssql", new ImportMsSQL());
            _table.Add("mysql", new ImportMySQL());
            _table.Add("mysqlPlesk10", new ImportMySQLPlesk10());
            _table.Add("mysqlPlesk11", new ImportMySQLPlesk11());
            _table.Add("access", new ImportAccess());
            _table.Add("mpsqlite", new MpImportSQLite());
            _table.Add("mpmssql", new MpImportMsSQL());
            _table.Add("mpmssqlicewarp", new MpImportMsSQLIceWarp());
            _table.Add("entrenix", new ImportEntrenix());

            return _table[dbtype];
        }

        private void CreateLogFile()
        {
            if (Settings.Default.logging)
            {
                var _logPath = Path.Combine(Directory.GetCurrentDirectory(), LOG_FILENAME);
                File.CreateText(_logPath).Close();

                _sb = new StreamWriter(_logPath);
            }
        }

        public void AuthenticationUnc()
        {            
            var arguments = String.Format(@"use \\{0}\c$ /user:{1} {2}", Settings.Default.DestinationServerIp,
                                                                                Settings.Default.DestinationServerUsername, 
                                                                                Settings.Default.DestinationServerPassword);
            ImportHelper.Net(arguments);
        }

        private void CopyEmailFiles(string domainName, string mailbox)
        {            
            var _source = Settings.Default.SourceDirEmailPattern.Replace("{DOMAIN}", domainName).Replace("{MAILBOX}",mailbox);

            var _destination = Settings.Default.DestinationDirEmailPatter
                                    .Replace("{DOMAIN}", domainName)
                                    .Replace("{DESTINATION}", Settings.Default.DestinationServerIp)
                                    .Replace("{MAILBOX}", mailbox);

            var arguments = String.Format(@"""{0}"" ""{1}"" /Z /PURGE /E", _source, _destination);
            
            PrintAndLog(String.Format("Copying Mailbox Files: {0}@{1}", mailbox, domainName));
            //PrintAndLog("Arguments: " + arguments);

            ImportHelper.Robocopy(arguments);
        }

        private void CopyHttpFiles(string domainName)
        {                                           
            var _source = Settings.Default.SourceDirPattern.Replace("{DOMAIN}", domainName);
            var _destination = Settings.Default.DestinationDirPattern.Replace("{DOMAIN}", domainName).Replace("{DESTINATION}", Settings.Default.DestinationServerIp);

            var arguments = String.Format(@"""{0}"" ""{1}"" /Z /PURGE /E", _source, _destination);

            PrintAndLog(String.Format("Copying Http Files: {0}", domainName));
            //PrintAndLog("Arguments: " + arguments);

            ImportHelper.Robocopy(arguments);
        }

        private List<string> GetDomainFilter()
        {
            var _list = new List<string>();

            var _filter_file = Path.Combine(Environment.CurrentDirectory, FILTER_FILENAME);

            if (File.Exists(_filter_file))            
                _list = File.ReadAllLines(_filter_file).ToList();

            return _list;
        }

        private void ExportDatabase(Database db)
        {
            var result = new MethodResult();

            if (db.DbType == "mysql" && Settings.Default.exportMySQL)
            {
                result = mysqlmgr.Backup(db.Name, mysqlScriptDirectory);
            }

            if (db.DbType == "mssql" && Settings.Default.exportMsSQL)
                result = mssqlmgr.Backup(db.Name, mssqlScriptDirectory);            

            if (result.Status)            
                PrintAndLog("Database Exported: " + db.Name);            
            else            
                PrintAndLog(String.Format("Database Export Error: {0} {1}",db.Name, result.Msg));
        }

        private void CreateWebSitePackage(string domainName)
        {
            PrintAndLog(String.Format("Create Package: {0}", domainName));            
            zipmgr.CreatePackage(domainName);

            if (Settings.Default.CopyDomainPackage)
            {
                var sourcePackageFile = String.Format("{0}.7z", domainName);
                var destinationPackageDir = Settings.Default.DomainPackageDestinationDir
                                                    .Replace("{DESTINATION}", Settings.Default.DestinationServerIp);


                var arguments = String.Format(@"""{0}"" ""{1}"" {2}", Settings.Default.DomainPackageLocalDir,
                                                                                destinationPackageDir,
                                                                                sourcePackageFile);

                PrintAndLog("Transfer Package : " + destinationPackageDir);
                PrintAndLog("Arguments: " + arguments);

                ImportHelper.Robocopy(arguments);
            }
        }

        private void StartLogging()
        {
            if (Settings.Default.logging)
            {
                _sb.WriteLine(String.Format("API Key: {0}", Settings.Default.MaestroPanelApiKey));
                _sb.WriteLine(String.Format("Reseller Name: {0}", Settings.Default.pleskResellerName));
                _sb.WriteLine(String.Format("Start Time: {0}", DateTime.Now));
            }
        }

        private void EndLogging()
        {
            if (Settings.Default.logging)
            {
                _sb.WriteLine(String.Format("End Time: {0}", DateTime.Now));
                _sb.Flush();
                _sb.Close();
                _sb.Dispose();
            }
        }

        public void Dispose()
        {
            if (_sb != null)
                _sb.Close(); _sb.Dispose();
        }
    }
}
