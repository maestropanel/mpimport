namespace MpMigrate.Core.Discovery
{
    using Microsoft.Web.Administration;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class WebsitePanel_Discover: IDiscovery
    {
        private string WEBSITEPANEL_PATH = "";
        private readonly string WEBSITEPANEL_WEBSITE_NAME = "WebsitePanel Enterprise Server";
        
        private string _dbhost;
        private string _username;
        private string _password;
        private string _databaseName;

        public WebsitePanel_Discover()
        {

        }

        public string Version()
        {
            WEBSITEPANEL_PATH = GetPhysicalPathByDomainName(WEBSITEPANEL_WEBSITE_NAME);

            var filePath = Path.Combine(WEBSITEPANEL_PATH, "WebsitePanel.EnterpriseServer.Base.dll");

            if (!File.Exists(filePath))
                return "";

            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(filePath);

            return fvi.ProductVersion;
        }

        public string VhostPath()
        {
            return "";
        }

        public DatabaseProviders GetDatabaseProvider()
        {
            SetDatabase();

            return DatabaseProviders.MSSQL;
        }

        public string GetDatabaseHost()
        {
            return _dbhost;
        }

        public int GetDatabasePort()
        {
            return 1433;
        }

        public string GetDatabaseUsername()
        {
            return _username;
        }

        public string GetDatabasePassword()
        {
            return _password;
        }

        public string GetDatabaseName()
        {
            return _databaseName;
        }

        public string GetDatabaseFile()
        {
            return "";
        }

        public string InstallPath()
        {
            return "";
        }

        public string GetPanelPassword()
        {
            return "";
        }

        public string GetEmailPath()
        {
            return "";
        }

        public bool isInstalled()
        {
            return isWebSiteExists();
        }

        private string GetPhysicalPathByDomainName(string siteName)
        {
            var _physicalPath = "";

            using (ServerManager _server = new ServerManager())
            {
                var _site = _server.Sites[siteName];
                var _siteApp = _site.Applications.Where(m => m.Path.Equals("/")).FirstOrDefault();
                var _vdir = _siteApp.VirtualDirectories.Where(m => m.Path == "/").FirstOrDefault();

                if (_vdir != null)
                    _physicalPath = _vdir.PhysicalPath;
            }

            return _physicalPath;
        }

        private bool isWebSiteExists()
        {
            using (ServerManager _server = new ServerManager())
            {
                return _server.Sites.Where(m => m.Name == WEBSITEPANEL_WEBSITE_NAME).Any();
            }
        }

        public void SetDatabase()
        {
            if (isWebSiteExists())
            {

                System.Configuration.Configuration rootWebConfig1 = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/", WEBSITEPANEL_WEBSITE_NAME);
                var connectionStr = rootWebConfig1.ConnectionStrings.ConnectionStrings["EnterpriseServer"].ConnectionString;
                
                //server=localhost\sqlexpress;database=WebsitePanel;uid=WebsitePanel;pwd=kla!kk!;
                System.Data.SqlClient.SqlConnectionStringBuilder builder = new System.Data.SqlClient.SqlConnectionStringBuilder(connectionStr);

                _dbhost = builder.DataSource;
                _username = builder.UserID;
                _password = builder.Password;
                _databaseName = builder.InitialCatalog;
            }
        }
    }
}
