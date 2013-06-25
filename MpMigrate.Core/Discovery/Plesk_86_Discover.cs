namespace MpMigrate.Core.Discovery
{
    using MpMigrate.Data.Entity;
    using System;
    using System.IO;
    
    public class Plesk_86_Discover : IDiscovery
    {
        public string Version()
        {
            return CoreHelper.GetRegistryKeyValue(@"SOFTWARE\PLESK\PSA Config\Config","PRODUCT_VERSION");
        }

        public string VhostPath()
        {
            return Path.Combine(Environment.GetEnvironmentVariable("plesk_vhosts", EnvironmentVariableTarget.Machine),"{DOMAIN}","httpdocs");
        }        

        public string InstallPath()
        {
            return Environment.GetEnvironmentVariable("plesk_dir", EnvironmentVariableTarget.Machine);
        }

        private DatabaseProviders PleskDatabaseProvider(string databaseProviderName)
        {
            DatabaseProviders provider = DatabaseProviders.Unknown;

            switch (databaseProviderName)
            {
                case "MySQL":
                    provider = DatabaseProviders.MYSQL;
                    break;
                case "MsSQL":
                    provider = DatabaseProviders.MSSQL;
                    break;
            }

            return provider;

        }

        public string GetPanelPassword()
        {
            var plesk_bin = Environment.GetEnvironmentVariable("plesk_bin", EnvironmentVariableTarget.Machine);

            if(!String.IsNullOrEmpty(plesk_bin))
            {
                var plesksrvclient_exe = Path.Combine(plesk_bin, "plesksrvclient.exe");
                CoreHelper.Exec(plesksrvclient_exe, "-get  -nogui");

                return System.Windows.Forms.Clipboard.GetText();
            }
            else
            {
                throw new Exception("Could not determine password: Plesk 8.6");
            }
        }

        public string GetEmailPath()
        {
            var mailproviderPath = String.Empty;
            var mailprovider = CoreHelper.GetRegistryKeyValue(@"SOFTWARE\PLESK\PSA Config\Config", "MAIL_PROVIDERW_DLL");
            var isMailEnable = mailprovider.EndsWith("mailenableproviderw.dll");

            if (isMailEnable)
                mailproviderPath = Path.Combine(CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Mail Enable\Mail Enable", "Mail Root"),"{DOMAIN}","MAILROOT","{MAILBOX}");

            return mailproviderPath;
        }

        public DatabaseProviders GetDatabaseProvider()
        {
            var providerName = CoreHelper.GetRegistryKeyValue(@"SOFTWARE\PLESK\PSA Config\Config", "PLESK_DATABASE_PROVIDER_NAME");
            return PleskDatabaseProvider(providerName);
        }

        public string GetDatabaseHost()
        {
            return CoreHelper.GetRegistryKeyValue(@"SOFTWARE\PLESK\PSA Config\Config", "MySQL_DB_HOST");
        }

        public int GetDatabasePort()
        {
            return int.Parse(CoreHelper.GetRegistryKeyValue(@"SOFTWARE\PLESK\PSA Config\Config", "MySQL_DB_PORT"));
        }

        public string GetDatabaseUsername()
        {
            return CoreHelper.GetRegistryKeyValue(@"SOFTWARE\PLESK\PSA Config\Config", "PLESK_DATABASE_LOGIN");
        }

        public string GetDatabasePassword()
        {
            return GetPanelPassword();
        }

        public string GetDatabaseName()
        {
            return CoreHelper.GetRegistryKeyValue(@"SOFTWARE\PLESK\PSA Config\Config", "mySQLDBName");
        }

        public string GetDatabaseFile()
        {
            return "";
        }

        public bool isInstalled()
        {
            var installed = false;
            var check_environment = Environment.GetEnvironmentVariable("plesk_dir", EnvironmentVariableTarget.Machine);
            var plesk_version = CoreHelper.GetRegistryKeyValue(@"SOFTWARE\PLESK\PSA Config\Config", "PRODUCT_VERSION");

            if (Directory.Exists(check_environment))                            
                if (plesk_version.StartsWith("8.6"))
                    installed = true;
            
            return installed;
        }
    }
}
