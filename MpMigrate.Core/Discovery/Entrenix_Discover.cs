namespace MpMigrate.Core.Discovery
{
    using System;
    using System.Data.Odbc;

    public class Entrenix_Discover : IDiscovery
    {
        public string Version()
        {
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
            return DatabaseProviders.ACCESS_ODBC;
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
            return CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Entrenix\ServerSettings", "DBPath");
        }

        public string GetDatabaseFile()
        {
            return CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Entrenix\ServerSettings", "DBPath");
        }

        public string InstallPath()
        {
            return CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Entrenix\ServerSettings", "Entrenix_Path");
        }

        public string GetPanelPassword()
        {
            return GetServerSettings("Sa_Password");
        }

        public string GetEmailPath()
        {
            var emailPath = CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Mail Enable\Mail Enable", "Mail Root");

            return emailPath +"\\{DOMAIN}\\MAILROOT\\{MAILBOX}";
        }

        public bool isInstalled()
        {
            return CoreHelper.isRegistryKeyExists(@"SOFTWARE\Entrenix\ServerSettings");
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
            var mdb_path = CoreHelper.GetRegistryKeyValue(@"SOFTWARE\Entrenix\ServerSettings", "DBPath");            
            var connStr = "DRIVER={Microsoft Access Driver (*.mdb)}; DBQ=" + mdb_path;

            return connStr;
        }
    }
}
