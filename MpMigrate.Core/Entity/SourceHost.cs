namespace MpMigrate.Core.Entity
{
    using MpMigrate.Data.Entity;

    public class SourceHost
    {
        public string DomainPath { get; set; }
        public string EmailPath { get; set; }

        public PanelStats Stats { get; set; }
    }
}
