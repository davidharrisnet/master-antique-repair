using Microsoft.AspNet.Identity.EntityFramework;

namespace MasterAntiqueRepair
{
    // Concrete wrapper around the raw generic IdentityUserRole<int> - see Role.cs for why
    // this app's EF6 build needs a non-generic subclass for every Identity entity type
    // rather than the open generic used directly.
    public class UserRole : IdentityUserRole<int>
    {
    }
}
