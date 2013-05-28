namespace MpMigrate.Core
{
    using Microsoft.Win32;
    using System;
    using System.Diagnostics;
    using System.IO;

    public class CoreHelper
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

        public static void Exec(string executable, string arguments)
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
    }
}
