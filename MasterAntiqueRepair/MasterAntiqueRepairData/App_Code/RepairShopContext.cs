using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace MasterAntiqueRepair
{
    // IdentityDbContext (not a plain DbContext) so Users/Roles/UserRoles/UserClaims/
    // UserLogins are all managed by ASP.NET Identity's own UserStore/RoleStore rather
    // than hand-rolled tables - see AccountService/AuthService/IdentityConfig, which
    // drive everything identity-related through UserManager/RoleManager instead of
    // querying these tables directly.
    public class RepairShopContext : IdentityDbContext<User, Role, int, UserLogin, UserRole, UserClaim>
    {
        public RepairShopContext() : base("DefaultConnection")
        {
            DisableIdentityUsernameValidation();
        }

        public RepairShopContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
            DisableIdentityUsernameValidation();
        }

        // Standard OWIN per-request-context factory signature, used by Startup.Auth.cs's
        // app.CreatePerOwinContext(RepairShopContext.Create).
        public static RepairShopContext Create()
        {
            return new RepairShopContext();
        }

        // IdentityDbContext.ValidateEntity enforces case-insensitive UserName uniqueness
        // across every row, including soft-deleted ones - that would break the app-level
        // rule (enforced by the dbo.Users filtered unique index from the
        // AddUniqueActiveUsername migration, and by IdentityConfig's ActiveUsernameValidator)
        // that a soft-deleted user's old username stays reusable by a new account.
        // Uniqueness is already guaranteed at the DB and UserManager layers, so the
        // redundant EF-level check is simply turned off rather than fought.
        private void DisableIdentityUsernameValidation()
        {
            Configuration.ValidateOnSaveEnabled = false;
        }

        public DbSet<Ticket> Tickets { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Keep the existing dbo.Users table/column names instead of Identity's
            // AspNetUsers/UserName defaults - Tickets/Comments/AuditLogs already have
            // foreign keys into dbo.Users, and "Name" is what every query/view in this
            // app already reads as the login name.
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<User>().Property(u => u.UserName).HasColumnName("Name");

            // New tables - no existing data/naming to preserve, so named to match this
            // app's plain PascalCase table style (Users/Tickets/Comments) rather than
            // Identity's AspNetRoles/AspNetUserRoles/AspNetUserClaims/AspNetUserLogins.
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<UserRole>().ToTable("UserRoles");
            modelBuilder.Entity<UserClaim>().ToTable("UserClaims");
            modelBuilder.Entity<UserLogin>().ToTable("UserLogins");
        }
    }
}
