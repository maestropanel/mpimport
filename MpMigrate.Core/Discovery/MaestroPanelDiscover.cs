namespace MpMigrate.Core.Discovery
{
    using System;
    using System.IO;

    public class MaestroPanelDiscover : IDiscovery
    {
        public string Version()
        {
            var agent_exe = Path.Combine(Environment.GetEnvironmentVariable("MaestroPanelPath", EnvironmentVariableTarget.Machine),"Agent","MstrSvc.exe");           
            return CoreHelper.FileVersion(agent_exe);            
        }

        public string VhostPath()
        {
            
            return "";
        }

        public string InstallPath()
        {
            return "";
        }

        public string GetPanelPassword()
        {
            return null;
        }

        public string GetEmailPath()
        {
            return null;
        }


        public DatabaseProviders GetDatabaseProvider()
        {
            return DatabaseProviders.Unknown;
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
            return "";
        }

        public string GetDatabasePassword()
        {
            return "";
        }

        public string GetDatabaseName()
        {
            return "";
        }


        public string GetDatabaseFile()
        {
            return "";
        }


        public bool isInstalled()
        {
            return false;
        }



    }
}
