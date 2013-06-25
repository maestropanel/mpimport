using MpMigrate.Data.Entity;
namespace MpMigrate.Core
{
    public interface IDiscovery
    {
        string Version();
        string VhostPath();
        DatabaseProviders GetDatabaseProvider();
        string GetDatabaseHost();
        int GetDatabasePort();
        string GetDatabaseUsername();
        string GetDatabasePassword();
        string GetDatabaseName();
        string GetDatabaseFile();
        string InstallPath();
        string GetPanelPassword();
        string GetEmailPath();
        bool isInstalled();
        
    }
}
