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
            var pdicover = new Plesk_86();
            var PleskDb = pdicover.GetDatabase();

            var plesk = new Plesk_86_MySql(PleskDb.ConnectionString());


            
        }
    }
}
