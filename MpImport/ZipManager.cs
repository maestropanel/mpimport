namespace MpMigrate
{
    using MpMigrate.Properties;
    using System;
    using System.IO;

    public class ZipManager
    {
        public void CreatePackage(string domainName)
        {
            var sourceDir = Settings.Default.SourceDirPattern.Replace("{DOMAIN}", domainName);
            var destinationFileName = String.Format("{0}.7z",domainName);                        

            var packageFile = Path.Combine(Settings.Default.DomainPackageLocalDir, destinationFileName);

            Console.WriteLine(sourceDir);

            ImportHelper.Zip(String.Format(@"a -mx1 -mmt -y ""{0}"" ""{1}\*""", packageFile, sourceDir));
        }
    }
}
