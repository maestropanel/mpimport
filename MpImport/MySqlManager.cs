namespace MpMigrate
{
    using MySql.Data.MySqlClient;
    using MpMigrate.Entity;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using MpMigrate.Properties;
    using System.Diagnostics;

    public class MySqlManager
    {
        private const string MYSQL51_REGISTRY_KEY = "SOFTWARE\\Wow6432Node\\MySQL AB\\MySQL Server 5.1";

        public MethodResult Backup(string databaseName, string destinationPath)
        {
            if (CheckMySQL51())
            {
                return BackupWithMySqlDump(databaseName, destinationPath);
            }
            else
            {
                return BackupWithMySQLBackup(databaseName, destinationPath);
            }
        }

        public MethodResult Restore(string databaseName, string sqlScriptPath)
        {
            var _result = new MethodResult();

            if (File.Exists(sqlScriptPath))
            {
                _result.Status = false;
                _result.Msg = "Script file not found: " + sqlScriptPath;

                return _result;
            }


            try
            {
                //using (MySqlBackup mb = new MySqlBackup(GetConnectionSting(databaseName)))
                //{
                //    mb.ImportInfo.SetTargetDatabase(databaseName);
                //    mb.ImportInfo.IgnoreSqlError = true;
                //    mb.ImportInfo.FileName = sqlScriptPath;
                //    mb.ImportInfo.AutoCloseConnection = true;
                //    mb.Import();
                //}

                _result.Status = true;
                _result.Msg = "Database Import Success: " + databaseName;
            }
            catch (Exception ex)
            {
                _result.Status = false;
                _result.Msg = String.Format("Database Import Error ({0}): {1}", databaseName, ex.Message);         
            }


            return _result;
        }


        private MethodResult BackupWithMySQLBackup(string databaseName, string destinationPath)
        {
            var _result = new MethodResult();

            try
            {
                var backupSqlfile = Path.Combine(destinationPath, String.Format("{0}.sql", databaseName));

                //using (MySqlBackup mb = new MySqlBackup(GetConnectionSting(databaseName)))
                //{
                //    mb.ExportInfo.FileName = backupSqlfile;
                //    mb.ExportInfo.ExportStoredProcedures = true;
                //    mb.ExportInfo.ExportTableStructure = true;
                //    mb.ExportInfo.ExportTriggers = true;
                //    mb.ExportInfo.ExportViews = true;
                //    mb.ExportInfo.RecordDumpTime = true;
                //    mb.ExportInfo.AutoCloseConnection = true;
                //    mb.Export();
                //}

                _result.Status = true;
                _result.Msg = "Database Export Success: " + databaseName;


            }
            catch (Exception ex)
            {
                _result.Status = false;
                _result.Msg = String.Format("Database Export Error ({0}): {1}", databaseName, ex.Message);
            }

            return _result;
        }

        private MethodResult BackupWithMySqlDump(string databaseName, string destinationPath)
        {            
            var backupSqlfile = Path.Combine(destinationPath, String.Format("{0}.sql", databaseName));
            return mysqldump(databaseName, backupSqlfile);
        }

        private string GetConnectionSting(string databaseName)
        {
            return String.Format("Server={4};Database={0};Uid={1};Pwd={2};Port={3}",
                databaseName, 
                Settings.Default.mysqlUser,
                Settings.Default.mysqlPassword,
                Settings.Default.mysqlPort,
                Settings.Default.mysqlHost);
        }


        private string GetMySQLDumpPath()
        {
            var Location = ImportHelper.GetRegistryKeyValue(MYSQL51_REGISTRY_KEY, "Location");
            return Path.Combine(Location, "bin", "mysqldump.exe");
        }

        private bool CheckMySQL51()
        {
            return ImportHelper.isRegistryKeyExists(MYSQL51_REGISTRY_KEY);
        }


        private MethodResult mysqldump(string databaseName, string destinationSqlScriptFile)
        {
            var result = new MethodResult();

            var Location = Path.Combine(ImportHelper.GetRegistryKeyValue(MYSQL51_REGISTRY_KEY, "Location"),"bin", "mysqldump.exe");            

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = String.Format("\"{0}\"", Location);
            startInfo.Arguments = String.Format("--host={0} --user={1}  --password={2} --routines --triggers --events --add-drop-table --dump-date --opt --no-create-db --set-charset --database {3} -r \"{4}\"",
                Settings.Default.mysqlHost,
                Settings.Default.mysqlUser,
                Settings.Default.mysqlPassword,
                databaseName,
                destinationSqlScriptFile);

            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = false;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            
            using (var execute = Process.Start(startInfo))
            {                
                execute.WaitForExit();                

                if(execute.ExitCode != 0)
                {
                    result.Status = false;
                    result.Msg = execute.StandardError.ReadToEnd();
                }
                else
                    result.Status = true;                
            }

            return result;
        }

    }
}
