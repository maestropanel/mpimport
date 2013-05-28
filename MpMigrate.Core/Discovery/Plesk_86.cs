namespace MpMigrate.Core.Discovery
{
    using System;
    using System.IO;
    
    public class Plesk_86 : IDiscovery
    {
        public string Version()
        {
            return CoreHelper.GetRegistryKeyValue(@"SOFTWARE\PLESK\PSA Config\Config","PRODUCT_VERSION");
        }

        public string VhostPath()
        {
            return Path.Combine(Environment.GetEnvironmentVariable("plesk_vhosts", EnvironmentVariableTarget.Machine),"{DOMAIN}","httpdocs");
        }

        public PanelDatabase GetDatabase()
        {
            var providerName = CoreHelper.GetRegistryKeyValue(@"SOFTWARE\PLESK\PSA Config\Config","PLESK_DATABASE_PROVIDER_NAME");

            var pdb = new PanelDatabase();
            pdb.Provider = PleskDatabaseProvider(providerName);
            pdb.Host = CoreHelper.GetRegistryKeyValue(@"SOFTWARE\PLESK\PSA Config\Config", "MySQL_DB_HOST");
            pdb.Port = int.Parse(CoreHelper.GetRegistryKeyValue(@"SOFTWARE\PLESK\PSA Config\Config", "MySQL_DB_PORT"));
            pdb.Username = CoreHelper.GetRegistryKeyValue(@"SOFTWARE\PLESK\PSA Config\Config", "PLESK_DATABASE_LOGIN");
            pdb.Database = CoreHelper.GetRegistryKeyValue(@"SOFTWARE\PLESK\PSA Config\Config", "mySQLDBName");
            
            return pdb;
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
            var plesksrvclient_exe = Path.Combine(Environment.GetEnvironmentVariable("plesk_bin", EnvironmentVariableTarget.Machine),"plesksrvclient.exe");
            CoreHelper.Exec(plesksrvclient_exe, "-get  -nogui");

            return System.Windows.Forms.Clipboard.GetText();
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
    }
}
