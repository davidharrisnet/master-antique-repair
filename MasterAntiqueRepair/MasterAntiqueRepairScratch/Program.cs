using System;
using System.IO;
using System.Linq;
using MasterAntiqueRepair;

namespace MasterAntiqueRepairScratch
{
    class Program
    {
        static void Main(string[] args)
        {
            // |DataDirectory| only auto-resolves inside a web app - point it at the
            // website's real App_Data folder so this connects to the same database
            // the site itself uses (not a separate copy).
            var appDataPath = Path.GetFullPath(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\MasterAntiqueRepair\App_Data"));
            AppDomain.CurrentDomain.SetData("DataDirectory", appDataPath);

            // Rewrite anything below this line freely - it's just a scratch pad.
            // "DefaultConnection" is the real app's database; "TestConnection" is the
            // separate scratch database TestDbContext/TestEF.aspx uses.
            using (var db = new RepairShopContext())
            {
                var orders = db.Orders.OrderBy(o => o.Id).ToList();
                Console.WriteLine("Orders: " + orders.Count);
                foreach (var order in orders)
                {
                    Console.WriteLine("  #{0}: {1} ({2})", order.Id, order.Description, order.State);
                }
            }

            Console.WriteLine();
            if (!Console.IsInputRedirected)
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
