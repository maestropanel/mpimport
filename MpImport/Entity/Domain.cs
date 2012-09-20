namespace PleskImport
{
    using System;

    public class Domain
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ClientName { get; set; }        
        public string DomainPassword { get; set; }
        public long Status { get; set; }
        public DateTime? Expiration { get; set; }

        public DateTime FromUnixTime(double unixTime)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTime);
        }

        public DateTime FromUnixTime(string unixTime)
        {
            double d;
            
            if (!double.TryParse(unixTime, out d))            
                return FromUnixTime(0D);
            
            return FromUnixTime(d);
        }
    }
}
