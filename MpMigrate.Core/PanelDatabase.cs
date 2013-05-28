namespace MpMigrate.Core
{
    using System;

    public class PanelDatabase
    {
        public DatabaseProviders Provider { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public string DataseFile { get; set; }

        public string ConnectionString()
        {
            var connectionStr = String.Empty;

            switch (Provider)
            {
                case DatabaseProviders.Unknown:
                    break;
                case DatabaseProviders.MSSQL:
                    connectionStr = MsSqlConnectionString();
                    break;
                case DatabaseProviders.MYSQL:
                    connectionStr = MySqlConnectionString();
                    break;
                case DatabaseProviders.SQLITE:
                    connectionStr = SQLiteConnectionString();
                    break;
                case DatabaseProviders.SQLCE:
                    break;
                case DatabaseProviders.ACCESS:
                    connectionStr = MicrosoftAccessConnectionString();
                    break;
            }

            return connectionStr;
        }

        private string MicrosoftAccessConnectionString()
        {
            return String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}", DataseFile);
        }       

        private string MsSqlConnectionString()
        {
            return String.Format("Server={0};Database={1};User Id={2};Password={3};",
                Host,
                Database,
                Username,
                Password);
        }

        private string MySqlConnectionString()
        {
            return String.Format("Server={0};Port={1};Database={2};Uid={3};Pwd={4};",
                Host,
                Port,
                Database,
                Username,
                Password);
        }
       
        private string SQLiteConnectionString()
        {
            return String.Format("Data Source={0};Version=3;BinaryGUID=False", DataseFile);
        }
    }

    public enum DatabaseProviders
    {
        Unknown,
        MSSQL,
        MYSQL,
        SQLITE,
        SQLCE,
        ACCESS
    }
}
