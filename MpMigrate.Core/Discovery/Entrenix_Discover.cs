namespace MpMigrate.Core.Discovery
{
    using System;
    using System.Data.Odbc;

    public class Entrenix_Discover : IDiscovery
    {
        public string Version()
        {
            if(Environment.Is64BitOperatingSystem)
                return "v" + CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Wow6432Node\Entrenix\ServerSettings", "UpdateV");
            else
                return "v" + CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Entrenix\ServerSettings", "UpdateV");
        }

        public string VhostPath()
        {
            var iis_root = GetServerSettings("IIS_Root");
            var vhosts = iis_root +"\\{RESELLER}\\{DOMAIN}\\www";
                
            return vhosts;
        }

        public DatabaseProviders GetDatabaseProvider()
        {
            return DatabaseProviders.ACCESS;
        }

        public string GetDatabaseHost()
        {
            return "";
        }

        public int GetDatabasePort()
        {
            return 0;
        }

        public string GetDatabaseUsername()
        {
            return String.Empty;
        }

        public string GetDatabasePassword()
        {
            return String.Empty;
        }

        public string GetDatabaseName()
        {
            if (Environment.Is64BitOperatingSystem)
                return CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Wow6432Node\Entrenix\ServerSettings", "DBPath");
            else
                return CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Entrenix\ServerSettings", "DBPath");
        }

        public string GetDatabaseFile()
        {
            if (Environment.Is64BitOperatingSystem)
                return CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Wow6432Node\Entrenix\ServerSettings", "DBPath");
            else
                return CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Entrenix\ServerSettings", "DBPath");
        }

        public string InstallPath()
        {
            if (Environment.Is64BitOperatingSystem)
                return CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Wow6432Node\Entrenix\ServerSettings", "Entrenix_Path");
            else
                return CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Entrenix\ServerSettings", "Entrenix_Path");
        }

        public string GetPanelPassword()
        {
            return GetServerSettings("Sa_Password");
        }

        public string GetEmailPath()
        {
            var emailPath = Environment.Is64BitOperatingSystem ? CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Wow6432Node\Mail Enable\Mail Enable", "Mail Root") :
                                                                CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Mail Enable\Mail Enable", "Mail Root"); ;

            return emailPath +"\\{DOMAIN}\\MAILROOT\\{MAILBOX}";
        }

        public bool isInstalled()
        {
            return Environment.Is64BitOperatingSystem ? CoreHelper.isRegistryKeyExists(@"SOFTWARE\Wow6432Node\Entrenix\ServerSettings") :
                                                            CoreHelper.isRegistryKeyExists(@"SOFTWARE\Entrenix\ServerSettings");
        }

        private string GetServerSettings(string fieldName)
        {
            var returnValue = String.Empty;

            var connStr = CreateConnectionString();            

            using (OdbcConnection conn = new OdbcConnection(connStr))
            {
                conn.Open();

                using (OdbcCommand cmd = new OdbcCommand(String.Format("SELECT TOP 1 {0} FROM Server_Settings", fieldName), conn))
                {
                    var resturnObj = cmd.ExecuteScalar();
                    returnValue = resturnObj == null ? "" : resturnObj.ToString();
                }
                     
                conn.Close();
            }

            return returnValue;
        }

        private string CreateConnectionString()
        {
            var mdb_path = Environment.Is64BitOperatingSystem ?
                CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Wow6432Node\Entrenix\ServerSettings", "DBPath") :
                CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Entrenix\ServerSettings", "DBPath");            

            var connStr = "DRIVER={Microsoft Access Driver (*.mdb)}; DBQ=" + mdb_path;

            return connStr;
        }
    }
}
