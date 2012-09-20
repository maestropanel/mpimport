
using System.Collections.Generic;
namespace PleskImport
{
    public class Database
    {
        public int Id { get; set; }
        public string Domain { get; set; }
        public string Name { get; set; }        
        public string DbType { get; set; }

        public List<DatabaseUser> Users { get; set; }

        public Database()
        {
            Users = new List<DatabaseUser>();
        }
    }

    public class DatabaseUser
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
