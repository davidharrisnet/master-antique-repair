using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace MasterAntiqueRepair
{
    public static class IdentityConfig
    {
        public const string CustomerRole = "Customer";
        public const string EmployeeRole = "Employee";
        public const string ManagerRole = "Manager";

        // Shared by AccountService (which builds its own UserManager, no OWIN needed
        // for admin-driven account management) and the website's ApplicationUserManager
        // (built per-request via OWIN so AuthService/RepairAuthHelper can sign users in)
        // so both enforce identical password/lockout rules regardless of which one
        // created or edited the account.
        public static UserManager<User, int> CreateUserManager(RepairShopContext db)
        {
            var manager = new UserManager<User, int>(
                new UserStore<User, Role, int, UserLogin, UserRole, UserClaim>(db));
            Configure(manager, db);
            return manager;
        }

        public static void Configure(UserManager<User, int> manager, RepairShopContext db)
        {
            manager.UserValidator = new ActiveUsernameValidator(new UserRepository(db));
            manager.PasswordValidator = new PasswordPolicyValidator();
            manager.UserLockoutEnabledByDefault = true;
            manager.MaxFailedAccessAttemptsBeforeLockout = User.MaxFailedLoginAttempts;
            manager.DefaultAccountLockoutTimeSpan = User.LockoutDuration;
        }

        public static RoleManager<Role, int> CreateRoleManager(RepairShopContext db)
        {
            return new RoleManager<Role, int>(
                new RoleStore<Role, int, UserRole>(db));
        }

        // Idempotent - safe to call from Migrations Configuration.Seed on every
        // Update-Database, and from anywhere else that just wants to be sure the three
        // roles exist before assigning one.
        public static void EnsureRolesExist(RoleManager<Role, int> roleManager)
        {
            foreach (var name in new[] { CustomerRole, EmployeeRole, ManagerRole })
            {
                if (!roleManager.RoleExists(name))
                {
                    roleManager.Create(new Role(name));
                }
            }
        }

        // Mirrors the old User.SetPassword composition rule exactly (letters + digit +
        // one non-alphanumeric char, 8-128 length) so switching to UserManager.Create/
        // ResetPassword/AddPassword doesn't change the validation message shown to users.
        private class PasswordPolicyValidator : IIdentityValidator<string>
        {
            private const int MinPasswordLength = 8;
            private const int MaxPasswordLength = 128;
            private const string PasswordRulesMessage = "Passwords must have 8 to 128 characters and one or more letters, digits, and special characters.";

            public Task<IdentityResult> ValidateAsync(string password)
            {
                bool isValid = !string.IsNullOrEmpty(password)
                    && password.Length >= MinPasswordLength
                    && password.Length <= MaxPasswordLength
                    && password.Any(char.IsLetter)
                    && password.Any(char.IsDigit)
                    && password.Any(c => !char.IsLetterOrDigit(c));

                return Task.FromResult(isValid ? IdentityResult.Success : IdentityResult.Failed(PasswordRulesMessage));
            }
        }

        // The stock UserValidator<TUser,TKey> treats every row (including soft-deleted
        // ones) as taking its username permanently, which conflicts with this app's rule
        // (enforced at the DB level by a filtered unique index) that a soft-deleted
        // user's username becomes reusable. This reimplements the same active-only,
        // exclude-self check UserRepository.ExistsActiveByName already used pre-Identity.
        private class ActiveUsernameValidator : IIdentityValidator<User>
        {
            private readonly UserRepository _users;

            public ActiveUsernameValidator(UserRepository users)
            {
                _users = users;
            }

            public Task<IdentityResult> ValidateAsync(User item)
            {
                if (string.IsNullOrWhiteSpace(item.UserName))
                {
                    return Task.FromResult(IdentityResult.Failed("The user name field is required."));
                }

                var excludeId = item.Id == 0 ? (int?)null : item.Id;
                if (_users.ExistsActiveByName(item.UserName, excludeId))
                {
                    return Task.FromResult(IdentityResult.Failed("That username is already taken."));
                }

                return Task.FromResult(IdentityResult.Success);
            }
        }
    }
}
