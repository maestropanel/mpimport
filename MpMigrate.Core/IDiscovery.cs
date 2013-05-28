namespace MpMigrate.Core
{
    public interface IDiscovery
    {
        string Version();
        string VhostPath();
        PanelDatabase GetDatabase();
        string InstallPath();
        string GetPanelPassword();
        string GetEmailPath();

    }
}
