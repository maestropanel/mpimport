﻿namespace MpMigrate.Core.Discovery
{

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class Plesk_17_Discover : IDiscovery
    {
        public string Version()
        {
            if (Environment.Is64BitOperatingSystem)
                return CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Wow6432Node\PLESK\PSA Config\Config", "PRODUCT_VERSION");
            else
                return CoreHelper.GetRegistryKeyValue(@"SOFTWARE\PLESK\PSA Config\Config", "PRODUCT_VERSION");  
        }

        public string VhostPath()
        {
            return Path.Combine(Environment.GetEnvironmentVariable("plesk_vhosts", EnvironmentVariableTarget.Machine), "{DOMAIN}", "httpdocs");
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

        public DatabaseProviders GetDatabaseProvider()
        {
            var providerName = String.Empty;

            if (Environment.Is64BitOperatingSystem)
                providerName = CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Wow6432Node\PLESK\PSA Config\Config", "PLESK_DATABASE_PROVIDER_NAME");
            else
                providerName = CoreHelper.GetRegistryKeyValue(@"SOFTWARE\PLESK\PSA Config\Config", "PLESK_DATABASE_PROVIDER_NAME");

            return PleskDatabaseProvider(providerName);
        }

        public string GetDatabaseHost()
        {
            if (Environment.Is64BitOperatingSystem)
                return CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Wow6432Node\PLESK\PSA Config\Config", "MySQL_DB_HOST");
            else
                return CoreHelper.GetRegistryKeyValue(@"SOFTWARE\PLESK\PSA Config\Config", "MySQL_DB_HOST");
        }

        public int GetDatabasePort()
        {
            if (Environment.Is64BitOperatingSystem)
                return int.Parse(CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Wow6432Node\PLESK\PSA Config\Config", "MySQL_DB_PORT"));
            else
                return int.Parse(CoreHelper.GetRegistryKeyValue(@"SOFTWARE\PLESK\PSA Config\Config", "MySQL_DB_PORT"));
        }

        public string GetDatabaseUsername()
        {
            if (Environment.Is64BitOperatingSystem)
                return CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Wow6432Node\PLESK\PSA Config\Config", "PLESK_DATABASE_LOGIN");
            else
                return CoreHelper.GetRegistryKeyValue(@"SOFTWARE\PLESK\PSA Config\Config", "PLESK_DATABASE_LOGIN");
        }

        public string GetDatabasePassword()
        {
            return GetPanelPassword();
        }

        public string GetDatabaseName()
        {
            if (Environment.Is64BitOperatingSystem)
                return CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Wow6432Node\PLESK\PSA Config\Config", "mySQLDBName");
            else
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

            var plesk_version = String.Empty;

            if (Environment.Is64BitOperatingSystem)
                plesk_version = CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Wow6432Node\PLESK\PSA Config\Config", "PRODUCT_VERSION");
            else
                plesk_version = CoreHelper.GetRegistryKeyValue(@"SOFTWARE\PLESK\PSA Config\Config", "PRODUCT_VERSION");

            if (Directory.Exists(check_environment))
                if (plesk_version.StartsWith("17"))
                    installed = true;

            return installed;
        }

        public string GetPanelPassword()
        {
            var plesk_bin = Environment.GetEnvironmentVariable("plesk_bin", EnvironmentVariableTarget.Machine);

            if (!String.IsNullOrEmpty(plesk_bin))
            {
                var plesksrvclient_exe = Path.Combine(plesk_bin, "plesksrvclient.exe");
                CoreHelper.Exec(plesksrvclient_exe, "-get  -nogui");

                return System.Windows.Forms.Clipboard.GetText();
            }
            else
            {
                throw new Exception("Could not determine password: Plesk 17.0");
            }
        }

        public string GetEmailPath()
        {
            var mailproviderPath = String.Empty;

            var mailprovider = Environment.Is64BitOperatingSystem
                ? CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Wow6432Node\PLESK\PSA Config\Config", "MAIL_PROVIDERW_DLL")
                : CoreHelper.GetRegistryKeyValue(@"SOFTWARE\PLESK\PSA Config\Config", "MAIL_PROVIDERW_DLL");

            var isMailEnable = mailprovider.EndsWith("mailenableproviderw.dll");

            if (isMailEnable)
                mailproviderPath = Path.Combine(CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Wow6432Node\Mail Enable\Mail Enable", "Mail Root"), "{DOMAIN}", "MAILROOT", "{MAILBOX}");

            return mailproviderPath;
        }

        public string InstallPath()
        {
            return Environment.GetEnvironmentVariable("plesk_dir", EnvironmentVariableTarget.Machine);
        }
    }
}
