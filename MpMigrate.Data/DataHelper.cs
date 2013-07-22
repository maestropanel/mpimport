using System;
namespace MpMigrate.Data
{
    public class DataHelper
    {
        public static string GetPassword()
        {
            return System.Web.Security.Membership.GeneratePassword(8, 1);
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}
