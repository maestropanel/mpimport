namespace MpMigrate.Data.Entity
{
    public class Email
    {
        public string Name { get; set; }
        public string DomainName { get; set; }
        public string Password { get; set; }
        public string Redirect { get; set; }
        public string RedirectedEmail { get; set; }
        public double Quota { get; set; }
    }
}
