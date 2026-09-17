namespace MasterAntiqueRepairData.Migrations.RepairShop
{
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Microsoft.AspNet.Identity;

    internal sealed class Configuration : DbMigrationsConfiguration<MasterAntiqueRepair.RepairShopContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"Migrations\RepairShop";
        }

        // Runs after every Update-Database, so this is where the three AspNetIdentity
        // roles are guaranteed to exist, and where any user rows that predate the
        // AddAspNetIdentity migration (created back when role was implied by the TPH
        // discriminator alone) get backfilled into the matching role.
        protected override void Seed(MasterAntiqueRepair.RepairShopContext context)
        {
            using (var roleManager = MasterAntiqueRepair.IdentityConfig.CreateRoleManager(context))
            using (var userManager = MasterAntiqueRepair.IdentityConfig.CreateUserManager(context))
            {
                MasterAntiqueRepair.IdentityConfig.EnsureRolesExist(roleManager);

                BackfillRole(context.Set<MasterAntiqueRepair.Customer>().Select(u => u.Id), userManager, MasterAntiqueRepair.IdentityConfig.CustomerRole);
                BackfillRole(context.Set<MasterAntiqueRepair.Employee>().Select(u => u.Id), userManager, MasterAntiqueRepair.IdentityConfig.EmployeeRole);
                BackfillRole(context.Set<MasterAntiqueRepair.Manager>().Select(u => u.Id), userManager, MasterAntiqueRepair.IdentityConfig.ManagerRole);
            }
        }

        private static void BackfillRole(System.Collections.Generic.IEnumerable<int> userIds, UserManager<MasterAntiqueRepair.User, int> userManager, string role)
        {
            foreach (var userId in userIds.ToList())
            {
                if (!userManager.IsInRole(userId, role))
                {
                    userManager.AddToRole(userId, role);
                }
            }
        }
    }
}
