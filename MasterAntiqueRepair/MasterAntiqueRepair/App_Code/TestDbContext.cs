using System.Data.Entity;

namespace MasterAntiqueRepair
{
    public class TestDbContext : DbContext
    {
        public TestDbContext() : base("TestConnection") { }

        public DbSet<TestItem> TestItems { get; set; }
    }
}
