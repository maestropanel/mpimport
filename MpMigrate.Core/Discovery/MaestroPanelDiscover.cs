namespace MpMigrate.Core.Discovery
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;

    public class MaestroPanelDiscover : IDiscovery
    {
        private string currentConnectionString;
        private Dictionary<string, string> connectionStringKeys;
        private DatabaseProviders currentProvider;

        public MaestroPanelDiscover()
        {
            currentProvider = DatabaseProviders.Unknown;

            if (isInstalled())
            {
                currentConnectionString = GetConnectionString(out currentProvider);
                if (currentConnectionString.EndsWith(";"))
                    currentConnectionString = currentConnectionString.Remove(currentConnectionString.Length-1, 1);

                connectionStringKeys = currentConnectionString.Split(';')
                                                                .Select(t => t.Split(new char[] { '=' }, 2))
                                                                .ToDictionary(t => t[0].Trim(), t => t[1].Trim(), StringComparer.InvariantCultureIgnoreCase);
            }
        }

        public string Version()
        {
            if (isInstalled())
            {
                var agent_exe = Path.Combine(Environment.GetEnvironmentVariable("MaestroPanelPath", EnvironmentVariableTarget.Machine), "Web", "www", "bin", "maestropanel.web.dll");
                return CoreHelper.FileVersion(agent_exe);
            }
            else
            {
                return "";
            }
        }

        public string VhostPath()
        {            
            return "";
        }

        public string InstallPath()
        {
            return Environment.GetEnvironmentVariable("MaestroPanelPath", EnvironmentVariableTarget.Machine);
        }

        public string GetPanelPassword()
        {
            return String.Empty;
        }

        public string GetEmailPath()
        {
            return String.Empty;
        }

        public DatabaseProviders GetDatabaseProvider()
        {
            return currentProvider;
        }

        public string GetDatabaseHost()
        {
            if (currentProvider == DatabaseProviders.MSSQL)
                return connectionStringKeys["Data Source"];
            else if (currentProvider == DatabaseProviders.SQLITE)
                return connectionStringKeys["Data Source"];
            else
                return "";
        }

        public int GetDatabasePort()
        {
            if (currentProvider == DatabaseProviders.MSSQL)
                return 1433;                
            else
                return 0;
        }

        public string GetDatabaseUsername()
        {
            if (currentProvider == DatabaseProviders.MSSQL)
                return connectionStringKeys["User Id"];
            else
                return "";
        }

        public string GetDatabasePassword()
        {
            if (currentProvider == DatabaseProviders.MSSQL)
                return connectionStringKeys["Password"];
            else
                return "";
        }

        public string GetDatabaseName()
        {
            if (currentProvider == DatabaseProviders.MSSQL)
                return connectionStringKeys["Initial Catalog"];
            else if (currentProvider == DatabaseProviders.SQLITE)
                return connectionStringKeys["Data Source"];
            else
                return "";
        }

        public string GetDatabaseFile()
        {
            if (currentProvider == DatabaseProviders.MSSQL)
                return connectionStringKeys["Initial Catalog"];
            else if (currentProvider == DatabaseProviders.SQLITE)
                return connectionStringKeys["Data Source"];
            else
                return "";
        }

        public bool isInstalled()
        {
            if (Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine).Contains("MaestroPanelPath"))
            {
                var agent_exe = Path.Combine(Environment.GetEnvironmentVariable("MaestroPanelPath", EnvironmentVariableTarget.Machine), "Web", "www", "bin", "maestropanel.web.dll");
                return File.Exists(agent_exe);
            }
            else
            {
                return false;
            }
        }

        private string GetConnectionString(out DatabaseProviders provider)
        {
            provider = DatabaseProviders.Unknown;

            var _web_config_file = Path.Combine(Environment.GetEnvironmentVariable("MaestroPanelPath", EnvironmentVariableTarget.Machine), "Web", "www", "Web.config");

            if (!File.Exists(_web_config_file))
                throw new Exception("1. Web.config not found " + _web_config_file);

            var web_config = System.Configuration.ConfigurationManager.
                OpenMappedExeConfiguration(new ExeConfigurationFileMap() { ExeConfigFilename = _web_config_file }, ConfigurationUserLevel.None);

            if (web_config == null)
                throw new Exception("2. Web.config file cannot be read.");


            if (web_config.ConnectionStrings.ConnectionStrings["MaestroConnection"].ProviderName == "System.Data.SqlClient")
                provider = DatabaseProviders.MSSQL;

            if (web_config.ConnectionStrings.ConnectionStrings["MaestroConnection"].ProviderName == "System.Data.SQLite")
                provider = DatabaseProviders.SQLITE;

            return web_config.ConnectionStrings.ConnectionStrings["MaestroConnection"].ConnectionString;
        }        
    }
}
