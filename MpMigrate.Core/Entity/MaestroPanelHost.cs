namespace MpMigrate.Core.Entity
{

    public class MaestroPanelHost
    {
        public string ApiKey { get; set; }
        public string ApiHost { get; set; }
        public int ApiPort { get; set; }
        public bool UseHttps { get; set; }

        public string DefaultPlan { get; set; }

        public UncLocation DestinationWeb { get; set; }
        public UncLocation DestinationEmail { get; set; }
    }

    public class UncLocation
    {
        public string Host { get; set; }
        public string Path { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
