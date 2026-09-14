using System.Data.Entity;

namespace MasterAntiqueRepair
{
    public class RepairShopContext : DbContext
    {
        public RepairShopContext() : base("DefaultConnection") { }

        public DbSet<User> Users { get; set; }

        public DbSet<Ticket> Tickets { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }
    }
}
