using System.Data.Entity;

namespace MasterAntiqueRepair
{
    public class TestDbContext : DbContext
    {
        public TestDbContext() : base("TestConnection") { }

        public DbSet<TestItem> TestItems { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<UserRole> UserRoles { get; set; }

        public DbSet<State> RepairStates { get; set; }


    }
}
