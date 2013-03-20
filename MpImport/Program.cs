namespace PleskImport
{
    using System;
    using PleskImport.Properties;

    class Program
    {
        private static Import _import;

        static void Main(string[] args)
        {            
            Console.WriteLine(@"# _______ _______ _______ _______ _______  ______  _____   _____  _______ __   _ _______        ");
            Console.WriteLine(@"# |  |  | |_____| |______ |______    |    |_____/ |     | |_____] |_____| | \  | |______ |      ");
            Console.WriteLine(@"# |  |  | |     | |______ ______|    |    |    \_ |_____| |       |     | |  \_| |______ |_____ ");
            Console.WriteLine(@"#");
            Console.WriteLine(@"# Desc : MaestroPanel Import Tool - www.maestropanel.com");
            Console.WriteLine(@"# Date : 2012-08-22");
            Console.WriteLine(@"# Version: 1.0.0");

            Console.Write("\nAre you sure to start MaestroPanel import? (Yes/No): ");
            var answer = Console.ReadLine().Trim();

            if (answer.Equals("Yes"))
            {
                _import = new Import();

                if (Settings.Default.CopyFiles)
                    _import.AuthenticationUnc();
                
                _import.ImportDomains();
                _import.ImportResellers();
            }
        }
    }
}
