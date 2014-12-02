namespace MpMigrate.Core.Discovery
{
    using System;

    public class Helm_Discover : IDiscovery
    {
        public string Version()
        {
            return GetRegistryKey(@"SOFTWARE\WebHostAutomation\Helm\SystemSettings",
                                @"SOFTWARE\Wow6432Node\WebHostAutomation\Helm\SystemSettings", "SoftwareVersion");
        }

        public string VhostPath()
        {
            return "";
        }

        public DatabaseProviders GetDatabaseProvider()
        {
            return DatabaseProviders.MSSQL;
        }

        public string GetDatabaseHost()
        {
            return GetRegistryKey(@"SOFTWARE\WebHostAutomation\Helm\SystemSettings",
                    @"SOFTWARE\Wow6432Node\WebHostAutomation\Helm\SystemSettings", "DBServerName");
        }

        public int GetDatabasePort()
        {
            var serverPort = 1433;
            var serverPortStr = GetRegistryKey(@"SOFTWARE\WebHostAutomation\Helm\SystemSettings", 
                        @"SOFTWARE\Wow6432Node\WebHostAutomation\Helm\SystemSettings", "DBServerPort");

            int.TryParse(serverPortStr, out serverPort);

            return serverPort;
        }

        public string GetDatabaseUsername()
        {
            return GetRegistryKey(@"SOFTWARE\WebHostAutomation\Helm\SystemSettings",
                    @"SOFTWARE\Wow6432Node\WebHostAutomation\Helm\SystemSettings", "DBUserName");
        }

        public string GetDatabasePassword()
        {
            return GetRegistryKey(@"SOFTWARE\WebHostAutomation\Helm\SystemSettings",
                                @"SOFTWARE\Wow6432Node\WebHostAutomation\Helm\SystemSettings", "DBPassword");
        }

        public string GetDatabaseName()
        {
            return GetRegistryKey(@"SOFTWARE\WebHostAutomation\Helm\SystemSettings",
                                @"SOFTWARE\Wow6432Node\WebHostAutomation\Helm\SystemSettings", "DBName");
        }

        public string GetDatabaseFile()
        {
            return String.Empty;
        }

        public string InstallPath()
        {
            return GetRegistryKey(@"SOFTWARE\WebHostAutomation\Helm\SystemSettings",
                                @"SOFTWARE\Wow6432Node\WebHostAutomation\Helm\SystemSettings", "InstalLocation");
        }

        public string GetPanelPassword()
        {
            return String.Empty;
        }

        public string GetEmailPath()
        {
            return String.Empty;
        }

        public bool isInstalled()
        {
            var getVersion = GetRegistryKey(@"SOFTWARE\WebHostAutomation\Helm\SystemSettings", 
                                                @"SOFTWARE\Wow6432Node\WebHostAutomation\Helm\SystemSettings", "SoftwareVersion");

            return !String.IsNullOrEmpty(getVersion);
        }

        private string GetRegistryKey(string regKeyx86, string regKeyx64, string valueKey )
        {
            if (Environment.Is64BitOperatingSystem)
                return CoreHelper.GetRegistryKeyValue(regKeyx64, valueKey);
            else
                return CoreHelper.GetRegistryKeyValue(regKeyx86, valueKey);
        }
    }
}
