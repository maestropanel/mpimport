namespace PleskImport
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using MaestroPanelApi;
    using PleskImport.Entity;
    using PleskImport.Properties;
    using PleskImport.Dbo;
        
    public class Import
    {
        private DboFactory _db;
        private readonly string LOG_FILENAME = "import.log";
        private readonly string FILTER_FILENAME = "filter.txt";

        public Import()
        {
            _db = SelectFactory(Settings.Default.dbtype);

            if (_db == null)
                throw new NullReferenceException("Unknown dbtype: "+ Settings.Default.dbtype);
            
            if (Settings.Default.logging)
                CreateLogFile();
        }

        public void ImportDomains()
        {
            var _sb = new StringBuilder();

            _sb.AppendFormat("API Key: {0}", Settings.Default.apiKey).AppendLine();
            _sb.AppendFormat("Reseller Name: {0}",Settings.Default.resellerName).AppendLine();
            _sb.AppendFormat("Start Time: {0}", DateTime.Now).AppendLine().AppendLine();

            var client = new Client(Settings.Default.apiKey, Settings.Default.host, Settings.Default.port, false);

            var domains = String.IsNullOrEmpty(Settings.Default.resellerName) ?
                                                    _db.GetDomains() :
                                                    _db.GetDomains().Where(m => m.ClientName == Settings.Default.resellerName);
            
            var domainFilter = GetDomainFilter();

            if (domainFilter.Any())
                domains = domains.Where(m => domainFilter.Contains(m.Name)).ToList();

            foreach (var item in domains)
            {
                var result = Settings.Default.importDomains ?
                    client.DomainCreate(item.Name, Settings.Default.domainPlanName, item.Username, item.Password, 
                                            !String.IsNullOrEmpty(item.DomainPassword), expiration: item.Expiration) :
                    new ApiResult() { Code = 0, Message = "Domain Import is Disabled" };
                    
                if (result.Code == 0)
                {                    
                    if(Settings.Default.importDomains)
                        PrintAndLog(String.Format("Domain Added: {0}", item.Name), ref _sb);
                    
                    ImportEmails(item.Name, ref _sb);

                    if (Settings.Default.importDatabases)
                        ImportDatabases(item.Name, ref _sb);

                    if (Settings.Default.importSubdomains)
                        ImportSubdomain(item.Name, ref _sb);

                    if (Settings.Default.importDomainAlias)
                        ImportDomainAlias(item.Name, ref _sb);

                    if (Settings.Default.CopyFiles)                    
                        CopyHttpFiles(item.Name, ref _sb);
                }
                else
                {
                    PrintAndLog(String.Format("Domain Error: {0} - {1}", item.Name, result.Message), ref _sb);
                }
            }

            _sb.AppendLine().AppendFormat("End Time Time: {0}", DateTime.Now);

            WriteLog(_sb.ToString());
        }

        public void ImportEmails(string domainName, ref StringBuilder _sb)
        {
            var _quota = -1D;            
            
            var client = new Client(Settings.Default.apiKey, Settings.Default.host, Settings.Default.port, false);
            var EmailList = _db.GetEmails(domainName);

            foreach (var item in EmailList.Where(m => m.DomainName == domainName))
            {
                if (Settings.Default.importEmails)
                {
                    _quota = item.Quota > 0 ? ((item.Quota / 8) / 1024) : item.Quota;
                    var result = client.AddMailBox(item.DomainName, item.Name, item.Password, item.Quota, item.Redirect, item.RedirectedEmail);

                    if (result.Code == 0)
                        PrintAndLog(String.Format("\tEmail Added: {0}", item.Name), ref _sb);
                    else
                        PrintAndLog(String.Format("\tEmail Error: {0} - {1}", item.Name, result.Message), ref _sb);
                }

                if (Settings.Default.CopyEmailFiles)
                    CopyEmailFiles(domainName, item.Name, ref _sb);
            }
        }

        public void ImportDatabases(string domainName, ref StringBuilder _sb)
        {
            var client = new Client(Settings.Default.apiKey, Settings.Default.host, Settings.Default.port, false);
            var databaseList = _db.GetDatabases(domainName);

            foreach (var item in databaseList)
            {
                var result = client.AddDatabase(item.Domain, item.DbType, item.Name, "", "", -1);

                if (result.Code == 0)
                {
                    PrintAndLog(String.Format("\tDatabase Added: {0} ({1})", item.Name, item.DbType), ref _sb);

                    foreach (var user in item.Users)
                    {
                        var createUserResult = client.AddDatabaseUser(domainName, item.DbType, item.Name, user.Username, user.Password);

                        if(createUserResult.Code == 0)
                            PrintAndLog(String.Format("\t\tUser Added: {0}", item.Name), ref _sb);
                        else
                            PrintAndLog(String.Format("\t\tUser Add Error: {0} - {1}", item.Name, createUserResult.Message), ref _sb);
                    }
                }
                else
                {
                    PrintAndLog(String.Format("\tDatabase Error: {0} - {1}", item.Name, result.Message), ref _sb);
                }
            }            
        }

        public void ImportSubdomain(string domainName, ref StringBuilder _sb)
        {            
            var subdomains = _db.GetSubdomains(domainName);
            var client = new Client(Settings.Default.apiKey, Settings.Default.host, Settings.Default.port, false);

            foreach (var item in subdomains)
            {
                var subDomainResult = client.AddSubDomain(item.Domain, item.Name, item.Login, item.Password);

                if (subDomainResult.Code == 0)                
                    PrintAndLog(String.Format("\tSubdomain Added: {0}.{1}", item.Name, item.Domain), ref _sb);                
                else                
                    PrintAndLog(String.Format("\tSubdomain Error: {0}.{1}", item.Name, item.Domain), ref _sb);                
            }
        }

        public void ImportDomainAlias(string domainName, ref StringBuilder _sb)
        {           
            var aliasList = _db.GetDomainAliases(domainName);
            var client = new Client(Settings.Default.apiKey, Settings.Default.host, Settings.Default.port, false);

            foreach (var item in aliasList)
            {
                var subdomainResult = client.AddAlias(item.Domain, item.Alias);

                if (subdomainResult.Code == 0)                
                    PrintAndLog(String.Format("\tDomain Alias Added: {0}", item.Alias), ref _sb);                
                else                
                    PrintAndLog(String.Format("\tDomain Alias Error: {0} - {1}", item.Alias, subdomainResult.Message), ref _sb);                
            }
        }

        public void ImportClients()
        {
            // Comin Soon...
        }

        private List<string> IncludedDomains()
        {
            var i_domains = new List<string>();
            var IncludeDomainFile = Path.Combine(Environment.CurrentDirectory, "include.txt");

            if (File.Exists(IncludeDomainFile))
                i_domains = File.ReadAllLines(IncludeDomainFile).ToList();

            return i_domains;
        }

        private void WriteLog(string log)
        {
            var _logPath = Path.Combine(Directory.GetCurrentDirectory(), LOG_FILENAME);

            if (Settings.Default.logging)
                if(File.Exists(_logPath))
                    File.AppendAllText(_logPath, log);
        }

        private void PrintAndLog(string outputText, ref StringBuilder sb)
        {
            sb.AppendLine(outputText);
            Console.WriteLine(outputText);
        }

        private DboFactory SelectFactory(string dbtype)
        {
            var _table = new Dictionary<string, DboFactory>();
            _table.Add("mssql", new ImportMsSQL());
            _table.Add("mysql", new ImportMySQL());
            _table.Add("access", new ImportAccess());
            _table.Add("mpsqlite", new MpImportSQLite());
            _table.Add("mpmssql", new MpImportMsSQL());
            _table.Add("entrenix", new ImportEntrenix());

            return _table[dbtype];
        }

        private void CreateLogFile()
        {
            var _logPath = Path.Combine(Directory.GetCurrentDirectory(), LOG_FILENAME);
            File.CreateText(_logPath).Close();
        }

        public void AuthenticationUnc()
        {
            var net_exe = Path.Combine(Environment.GetEnvironmentVariable("windir", EnvironmentVariableTarget.Machine), "System32", "net.exe");
            var arguments = String.Format(@"use \\{0}\c$ /user:{1} {2}", Settings.Default.DestinationServerIp,
                                                                                Settings.Default.DestinationServerUsername, 
                                                                                Settings.Default.DestinationServerPassword);

            Execute(net_exe, arguments);
        }

        private void CopyEmailFiles(string domainName, string mailbox, ref StringBuilder _sb)
        {
            PrintAndLog("Copying Mailbox Files... (" + mailbox +")", ref _sb);

            var robocopy_exe = Path.Combine(Environment.GetEnvironmentVariable("windir", EnvironmentVariableTarget.Machine), "System32", "robocopy.exe");

            var _source = Settings.Default.SourceDirEmailPattern.Replace("{DOMAIN}", domainName).Replace("{MAILBOX}",mailbox);
            var _destination = Settings.Default.DestinationDirEmailPatter
                                    .Replace("{DOMAIN}", domainName)
                                    .Replace("{DESTINATION}", Settings.Default.DestinationServerIp)
                                    .Replace("{MAILBOX}", mailbox);

            var arguments = String.Format(@"""{0}"" ""{1}"" /Z /PURGE /E", _source, _destination);

            Execute(robocopy_exe, arguments);
        }

        private void CopyHttpFiles(string domainName, ref StringBuilder _sb)
        {
            PrintAndLog("Copying HTTP Files...", ref _sb);

            var robocopy_exe = Path.Combine(Environment.GetEnvironmentVariable("windir", EnvironmentVariableTarget.Machine), "System32", "robocopy.exe");
            var _source = Settings.Default.SourceDirPattern.Replace("{DOMAIN}", domainName);
            var _destination = Settings.Default.DestinationDirPattern
                                    .Replace("{DOMAIN}", domainName)
                                    .Replace("{DESTINATION}", Settings.Default.DestinationServerIp);

            var arguments = String.Format(@"""{0}"" ""{1}"" /Z /PURGE /E /MT:10", _source, _destination);

            Execute(robocopy_exe, arguments);
        }


        private void Execute(string executable, string arguments)
        {
            var _pinfo = new ProcessStartInfo();
            _pinfo.FileName = executable;
            _pinfo.Arguments = arguments;
            _pinfo.UseShellExecute = false;
            _pinfo.CreateNoWindow = true;
            _pinfo.WindowStyle = ProcessWindowStyle.Normal;
            
            var _process = Process.Start(_pinfo);
            _process.WaitForExit();
        }

        private List<string> GetDomainFilter()
        {
            var _list = new List<string>();

            var _filter_file = Path.Combine(Environment.CurrentDirectory, FILTER_FILENAME);

            if (File.Exists(_filter_file))            
                _list = File.ReadAllLines(_filter_file).ToList();

            return _list;
        }
    }
}
