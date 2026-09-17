using Microsoft.AspNet.Identity.EntityFramework;

namespace MasterAntiqueRepair
{
    // Microsoft only ships a non-generic IdentityRole convenience class for the default
    // string key - with the int key this app uses, the raw generic IdentityRole<int,
    // IdentityUserRole<int>> has to be wrapped in a concrete subclass (mirroring how User
    // wraps IdentityUser<int,...>), or EF6's DbSetDiscoveryService fails to map the
    // inherited Roles DbSet at RepairShopContext construction time ("type was not mapped").
    public class Role : IdentityRole<int, UserRole>
    {
        public Role()
        {
        }

        public Role(string name) : this()
        {
            Name = name;
        }
    }
}
