using System.Data.Entity;

namespace MasterAntiqueRepair
{
    public class RepairShopContext : DbContext
    {
        public RepairShopContext() : base("DefaultConnection") { }

        public DbSet<User> Users { get; set; }

        public DbSet<Order> Orders { get; set; }
    }
}
