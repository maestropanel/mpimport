namespace MpMigrate
{
    using Microsoft.Win32;
    using MpMigrate.Properties;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;


    public class ImportHelper
    {
        public static void Zip(string aguments)
        {
            var Zip7 = Path.Combine(Environment.CurrentDirectory, "7za.exe");
            Exec(Zip7, aguments);
        }

        public static void Robocopy(string arguments)
        {
            var robocopy_exe = Path.Combine(Environment.GetEnvironmentVariable("windir", EnvironmentVariableTarget.Machine), "System32", "robocopy.exe");
            Exec(robocopy_exe, arguments);
        }

        public static void Net(string arguments)
        {
            var net_exe = Path.Combine(Environment.GetEnvironmentVariable("windir", EnvironmentVariableTarget.Machine), "System32", "net.exe");
            Exec(net_exe, arguments);
        }

        private static void Exec(string executable, string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = String.Format("\"{0}\"", executable);
            startInfo.Arguments = arguments;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = false;
            startInfo.RedirectStandardOutput = false;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            

            using (var execute = Process.Start(startInfo))
            {                
                execute.WaitForExit();
            }
        }

        public static string GetRegistryKeyValue(string registryKey, string name)
        {
            var _value = "";
            var _key = RegistryKey
            .OpenBaseKey(RegistryHive.LocalMachine,
                            Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32)
                                .OpenSubKey(registryKey);

            if (_key != null)
                _value = _key.GetValue(name, "").ToString();

            return _value;
        }

        public static bool isRegistryKeyExists(string registryKey)
        {
            return (RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default).OpenSubKey(registryKey) != null);
        }

        public static string PleskMicrosoftAccessConnectionString()
        {
            return String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}",Settings.Default.pleskMsAccessMdbPath);
        }

        public static string EntrenixMicrosoftAccessConnectionString()
        {
            return String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}", Settings.Default.entrenixMsAccessMdbPath);
        }

        public static string PleskMsSqlConnectionString(string database = "psa")
        {
            return String.Format("Server={0};Database={1};User Id={2};Password={3};",
                Settings.Default.pleskMsSqlHost,
                database,
                Settings.Default.pleskMsSqlUser,
                Settings.Default.pleskMsSqlPassword);
        }

        public static string PleskMySqlConnectionString(string database = "psa")
        {
            return String.Format("Server={0};Port={1};Database={2};Uid={3};Pwd={4};",
                Settings.Default.pleskMySQLHost,
                Settings.Default.pleskMySQLPort,
                database,
                Settings.Default.pleskMySQLUser,
                Settings.Default.pleskMySQLPassword);
        }

        public static string MaestroPanelMsSqlConnectionString()
        {
            return String.Format("Server={0};Database={1};User Id={2};Password={3};",
                Settings.Default.MaestroPanelMsSqlHost,
                Settings.Default.MaestroPanelDatabaseName,
                Settings.Default.MaestroPanelMsSqlUser,
                Settings.Default.MaestroPanelMsSqlPassword);
        }

        public static string MaestroPanelSQLiteConnectionString()
        {
            return String.Format("Data Source={0};Version=3;BinaryGUID=False",
                Settings.Default.MaestroPanelSQLitePath);
        }
    }
}
