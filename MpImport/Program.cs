namespace MpMigrate
{
    using System;
    using MpMigrate.Properties;

    class Program
    {
        private static Import _import;

        static void Main(string[] args)
        {                           
            Console.WriteLine(@"# Desc : MaestroPanel Import Tool - www.maestropanel.com");
            Console.WriteLine(@"# Date : 2012-08-22");
            Console.WriteLine(@"# Version: 1.0.0");

            Console.Write("\nAre you sure to start MaestroPanel import? (Yes/No): ");
            var answer = Console.ReadLine().Trim();

            if (String.IsNullOrEmpty(answer))
                answer = "Yes";

            if (answer.Equals("Yes"))
            {
                _import = new Import();

                if (Settings.Default.CopyHttpFiles)
                    _import.AuthenticationUnc();
                
                _import.ImportDomains();
                _import.ImportResellers();
            }            
        }
    }
}
