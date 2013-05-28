namespace MpMigrate.SqlManager
{
    using Microsoft.SqlServer.Management.Sdk.Sfc;
    using Microsoft.SqlServer.Management.Smo;
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class MsSqlManager : IDisposable
    {
        internal Urn[] SmoObj;
        internal Scripter SmoScript;
        internal Server mssql;

        private string LogFilePath;

        public MsSqlManager()
        {
            LogFilePath = Path.Combine(Environment.CurrentDirectory, "mssql_backup_error.log");

            SmoObj = new Urn[2];
            mssql = new Server();
            SmoScript = GetScripter(mssql);
        }

        public MpMigrate.Entity.MethodResult Backup(string databaseName, string backupPath)
        {
            var _result = new MpMigrate.Entity.MethodResult();

            if (!Directory.Exists(backupPath))
                Directory.CreateDirectory(backupPath);

            var scriptFile = Path.Combine(backupPath, String.Format("{0}.sql", databaseName));

            StreamWriter fileWriter = File.CreateText(scriptFile);

            try
            {
                var database = GetDatabase(databaseName);

                GenerateTables(database, ref fileWriter);
                GenerateViews(database, ref fileWriter);
                GenerateStoredProcedures(database, ref fileWriter);
                GenerateUserDefinedFunction(database, ref fileWriter);

                _result.Status = true;
                _result.Msg = String.Format("Backup successfully complete: {0}", databaseName);
            }
            catch (Exception ex)
            {
                _result.Status = false;
                _result.Msg = String.Format("Backup Error: {1}, {0}", ex.Message, databaseName);

                WriteLog(databaseName, ex.Message);
            }
            finally
            {
                fileWriter.Close();
                fileWriter.Dispose();
            }

            return _result;
        }

        public MpMigrate.Entity.MethodResult Restore(string databaseName, string backupPath)
        {
            var _result = new MpMigrate.Entity.MethodResult();
            var scriptFile = Path.Combine(backupPath, String.Format("{0}.sql", databaseName));

            SqlCmd(databaseName, scriptFile);

            _result.Status = true;
            _result.Msg = "MS SQL Database Restore Complete: " + databaseName;

            return _result;
        }

        private Database GetDatabase(string databaseName)
        {
            if (mssql.Databases.Contains(databaseName))
                return mssql.Databases[databaseName];
            else
                throw new Exception(String.Format("Database not found: {0}", databaseName));
        }

        private Scripter GetScripter(Server mssql)
        {
            Scripter scrp = new Scripter(mssql);
            scrp.Options.ScriptSchema = true;
            scrp.Options.WithDependencies = false;
            scrp.Options.ScriptData = true;
            scrp.Options.LoginSid = false;
            //scrp.Options.DriAllKeys = true;
            scrp.Options.ContinueScriptingOnError = true;
            scrp.Options.IncludeIfNotExists = true;
            scrp.Options.DriPrimaryKey = true;
            scrp.Options.AnsiPadding = true;
            //scrp.Options.NoIdentities

            return scrp;
        }

        private void GenerateTables(Database database, ref StreamWriter fileWriter)
        {
            SmoScript.Options.ScriptDrops = true;

            foreach (Table tblItem in database.Tables)
            {
                if (!tblItem.IsSystemObject)
                {
                    foreach (string s in SmoScript.EnumScript(new Urn[] { tblItem.Urn }))
                    {
                        fileWriter.WriteLine(s);
                        fileWriter.WriteLine("GO");
                    }
                }
            }

            SmoScript.Options.ScriptDrops = false;

            foreach (Table tblItem in database.Tables)
            {
                if (!tblItem.IsSystemObject)
                {
                    foreach (string s in SmoScript.EnumScript(new Urn[] { tblItem.Urn }))
                    {
                        fileWriter.WriteLine(s);
                        fileWriter.WriteLine("GO");
                    }
                }

                //GenerateIndex(tblItem, ref fileWriter);
                GenerateForeignKeys(tblItem, ref fileWriter);
                GenerateTriggers(tblItem, ref fileWriter);

            }
        }

        private void GenerateIndex(Table databaseTable, ref StreamWriter fileWriter)
        {
            foreach (Index IndxItem in databaseTable.Indexes)
            {
                if (IndxItem.IsSystemObject == false)
                {
                    StringCollection indexScript = IndxItem.Script();

                    foreach (string s in indexScript)
                    {
                        fileWriter.WriteLine(s);
                        fileWriter.WriteLine("GO");
                    }
                }
            }
        }

        private void GenerateTriggers(Table databaseTable, ref StreamWriter fileWriter)
        {
            foreach (Trigger trggItem in databaseTable.Triggers)
            {
                if (!trggItem.IsSystemObject)
                {
                    foreach (string s in SmoScript.EnumScript(new Urn[] { trggItem.Urn }))
                    {
                        fileWriter.WriteLine(s);
                        fileWriter.WriteLine("GO");
                    }
                }
            }
        }

        private void GenerateViews(Database database, ref StreamWriter fileWriter)
        {
            SmoScript.Options.ScriptDrops = true;
            foreach (View vwItem in database.Views)
            {
                if (!vwItem.IsSystemObject)
                {
                    foreach (string s in SmoScript.EnumScript(new Urn[] { vwItem.Urn }))
                    {
                        fileWriter.WriteLine(s);
                        fileWriter.WriteLine("GO");
                    }
                }
            }

            SmoScript.Options.ScriptDrops = false;
            foreach (View vwItem in database.Views)
            {
                if (!vwItem.IsSystemObject)
                {
                    foreach (string s in SmoScript.EnumScript(new Urn[] { vwItem.Urn }))
                    {
                        fileWriter.WriteLine(s);
                        fileWriter.WriteLine("GO");
                    }
                }
            }
        }

        private void GenerateStoredProcedures(Database database, ref StreamWriter fileWriter)
        {
            SmoScript.Options.ScriptDrops = true;
            foreach (StoredProcedure spItem in database.StoredProcedures)
            {
                if (!spItem.IsSystemObject)
                {

                    foreach (string s in SmoScript.EnumScript(new Urn[] { spItem.Urn }))
                    {
                        fileWriter.WriteLine(s);
                        fileWriter.WriteLine("GO");
                    }
                }
            }

            SmoScript.Options.ScriptDrops = false;
            foreach (StoredProcedure spItem in database.StoredProcedures)
            {
                if (!spItem.IsSystemObject)
                {

                    foreach (string s in SmoScript.EnumScript(new Urn[] { spItem.Urn }))
                    {
                        fileWriter.WriteLine(s);
                        fileWriter.WriteLine("GO");
                    }
                }
            }
        }

        private void GenerateUserDefinedFunction(Database database, ref StreamWriter fileWriter)
        {
            SmoScript.Options.ScriptDrops = true;
            foreach (UserDefinedFunction udfItem in database.UserDefinedFunctions)
            {
                if (!udfItem.IsSystemObject)
                {
                    foreach (string s in SmoScript.EnumScript(new Urn[] { udfItem.Urn }))
                    {
                        fileWriter.WriteLine(s);
                        fileWriter.WriteLine("GO");
                    }
                }
            }

            SmoScript.Options.ScriptDrops = false;
            foreach (UserDefinedFunction udfItem in database.UserDefinedFunctions)
            {
                if (!udfItem.IsSystemObject)
                {
                    foreach (string s in SmoScript.EnumScript(new Urn[] { udfItem.Urn }))
                    {
                        fileWriter.WriteLine(s);
                        fileWriter.WriteLine("GO");
                    }
                }
            }
        }

        private void GenerateForeignKeys(Table databaseTable, ref StreamWriter fileWriter)
        {
            foreach (ForeignKey Fktem in databaseTable.ForeignKeys)
            {
                if (Fktem.IsSystemNamed == false)
                {
                    StringCollection FkScript = Fktem.Script();

                    foreach (string s in FkScript)
                    {
                        fileWriter.WriteLine(s);
                        fileWriter.WriteLine("GO");
                    }
                }
            }
        }

        private void GenerateUserDefinedDataTypes(Database database, ref StreamWriter fileWriter)
        {
            SmoScript.Options.ScriptDrops = true;
            foreach (UserDefinedDataType udfItem in database.UserDefinedDataTypes)
            {
                foreach (string s in SmoScript.EnumScript(new Urn[] { udfItem.Urn }))
                {
                    fileWriter.WriteLine(s);
                    fileWriter.WriteLine("GO");
                }
            }

            SmoScript.Options.ScriptDrops = false;
            foreach (UserDefinedDataType udfItem in database.UserDefinedDataTypes)
            {
                foreach (string s in SmoScript.EnumScript(new Urn[] { udfItem.Urn }))
                {
                    fileWriter.WriteLine(s);
                    fileWriter.WriteLine("GO");
                }
            }
        }

        private void GenerateXmlSchema(Database database, ref StreamWriter fileWriter)
        {
            SmoScript.Options.ScriptDrops = true;
            foreach (XmlSchemaCollection item in database.XmlSchemaCollections)
            {
                foreach (string s in SmoScript.EnumScript(new Urn[] { item.Urn }))
                {
                    fileWriter.WriteLine(s);
                    fileWriter.WriteLine("GO");
                }
            }

            SmoScript.Options.ScriptDrops = false;
            foreach (XmlSchemaCollection item in database.XmlSchemaCollections)
            {
                foreach (string s in SmoScript.EnumScript(new Urn[] { item.Urn }))
                {
                    fileWriter.WriteLine(s);
                    fileWriter.WriteLine("GO");
                }
            }
        }

        private void GenerateSchemas(Database database, ref StreamWriter fileWriter)
        {
            SmoScript.Options.ScriptDrops = true;
            foreach (Schema item in database.Schemas)
            {
                if (!item.IsSystemObject)
                {
                    foreach (string s in SmoScript.EnumScript(new Urn[] { item.Urn }))
                    {
                        fileWriter.WriteLine(s);
                        fileWriter.WriteLine("GO");
                    }
                }
            }

            SmoScript.Options.ScriptDrops = false;
            foreach (Schema item in database.Schemas)
            {
                if (!item.IsSystemObject)
                {
                    foreach (string s in SmoScript.EnumScript(new Urn[] { item.Urn }))
                    {
                        fileWriter.WriteLine(s);
                        fileWriter.WriteLine("GO");
                    }
                }
            }
        }

        private void GenerateFullTextCatalog(Database database, ref StreamWriter fileWriter)
        {
            SmoScript.Options.ScriptDrops = true;
            foreach (FullTextCatalog item in database.FullTextCatalogs)
            {
                foreach (string s in SmoScript.EnumScript(new Urn[] { item.Urn }))
                {
                    fileWriter.WriteLine(s);
                    fileWriter.WriteLine("GO");
                }
            }

            SmoScript.Options.ScriptDrops = false;
            foreach (FullTextCatalog item in database.FullTextCatalogs)
            {
                foreach (string s in SmoScript.EnumScript(new Urn[] { item.Urn }))
                {
                    fileWriter.WriteLine(s);
                    fileWriter.WriteLine("GO");
                }
            }
        }

        static void SqlCmd(string databaseName, string scriptFile)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = SqlCmdExePath();
            startInfo.Arguments = String.Format("-S (local) -E -d \"{0}\" -i \"{1}\"", databaseName, scriptFile);
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.ErrorDialog = false;
            startInfo.RedirectStandardError = false;
            startInfo.RedirectStandardOutput = false;
            startInfo.WindowStyle = ProcessWindowStyle.Normal;

            using (var execute = Process.Start(startInfo))
            {
                execute.WaitForExit();
            }
        }

        static string SqlCmdExePath()
        {
            var VerSpecificRootDir = ImportHelper.GetRegistryKeyValue(@"SOFTWARE\Microsoft\Microsoft SQL Server\100", "VerSpecificRootDir");
            var SqlCmdExe = Path.Combine(VerSpecificRootDir, "Tools", "Binn", "SQLCMD.EXE");

            return SqlCmdExe;
        }

        public void Dispose()
        {

        }

        private void WriteLog(string database, string text)
        {
            var errorList = String.Format("{0}\t{2}\t{1}", DateTime.Now, text, database);
            WriteTextFile(LogFilePath, text);
        }



        public static void WriteTextFile(string file, string text)
        {
            if (File.Exists(file))
            {
                File.WriteAllText(file, text);
            }
            else
            {
                using (var tfile = File.AppendText(file))
                {
                    tfile.WriteLine(file);
                }
            }
        }
    }
}
