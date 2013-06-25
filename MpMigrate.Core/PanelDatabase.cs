namespace MpMigrate.Core
{
    using MySql.Data.MySqlClient;
    using System;
    using System.Data.Odbc;
    using System.Data.SqlClient;
    using System.Data.SQLite;

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

        public bool TestConnection(out string errorMsg)
        {
            errorMsg = String.Empty;
            var result = false;
            var connectionString = ConnectionString();

            switch (Provider)
            {
                case DatabaseProviders.Unknown:
                    break;
                case DatabaseProviders.MSSQL:
                    result = MsSqlConnectionTest(connectionString, out errorMsg);
                    break;
                case DatabaseProviders.MYSQL:
                    result = MySqlConnectionTest(connectionString, out errorMsg);
                    break;
                case DatabaseProviders.SQLITE:
                    result = SQLiteConnectionTest(connectionString, out errorMsg);
                    break;
                case DatabaseProviders.SQLCE:
                    result = false;
                    break;
                case DatabaseProviders.ACCESS:
                    result = MicrosoftAccessConnectionTest(connectionString, out errorMsg);
                    break;
            }

            return result;
        }

        private string MicrosoftAccessConnectionString()
        {
            return String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}", DataseFile);
        }

        private bool MicrosoftAccessConnectionTest(string connectionString, out string errormsg)
        {
            errormsg = String.Empty;
            var result = false;

            try
            {
                using (OdbcConnection myConnection = new OdbcConnection())
                {
                    myConnection.ConnectionString = connectionString;
                    myConnection.Open();

                    myConnection.Close();
                }

                result = true;
            }
            catch (Exception ex)
            {
                errormsg = ex.Message;                
            }

            return result;
        }

        private string MsSqlConnectionString()
        {
            return String.Format("Server={0};Database={1};User Id={2};Password={3};",
                Host,
                Database,
                Username,
                Password);
        }


        private bool MsSqlConnectionTest(string connectionString, out string errormsg)
        {
            errormsg = String.Empty;
            var result = false;

            try
            {
                using (SqlConnection myConnection = new SqlConnection())
                {
                    myConnection.ConnectionString = connectionString;
                    myConnection.Open();

                    myConnection.Close();
                }

                result = true;
            }
            catch (Exception ex)
            {
                errormsg = ex.Message;
            }

            return result;
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

        private bool MySqlConnectionTest(string connectionString, out string errormsg)
        {
            errormsg = String.Empty;
            var result = false;

            try
            {
                using (MySqlConnection myConnection = new MySqlConnection())
                {
                    myConnection.ConnectionString = connectionString;
                    myConnection.Open();

                    myConnection.Close();
                }

                result = true;
            }
            catch (Exception ex)
            {
                errormsg = ex.Message;
            }

            return result;
        }
       
        private string SQLiteConnectionString()
        {
            return String.Format("Data Source={0};Version=3;BinaryGUID=False", DataseFile);
        }

        private bool SQLiteConnectionTest(string connectionString, out string errormsg)
        {
            errormsg = String.Empty;
            var result = false;

            try
            {
                using (SQLiteConnection myConnection = new SQLiteConnection())
                {
                    myConnection.ConnectionString = connectionString;
                    myConnection.Open();

                    myConnection.Close();
                }

                result = true;
            }
            catch (Exception ex)
            {
                errormsg = ex.Message;
            }

            return result;
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
