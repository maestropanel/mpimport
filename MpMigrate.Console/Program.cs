using MpMigrate.Core.Discovery;
using MpMigrate.Data.Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MpMigrate.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var conn = "Server=127.0.0.1;Port=3306;Database=psa;Uid=root;Pwd=osman12!;";

            var plesk = new Plesk_86_MySql(conn);
            var stats = plesk.GetPanelStats();
            
        }
    }
}
