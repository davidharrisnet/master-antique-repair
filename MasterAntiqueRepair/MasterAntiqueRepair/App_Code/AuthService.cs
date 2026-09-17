using System;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace MasterAntiqueRepair
{
    // Consolidates the login/signup/password-reset business logic that used to be
    // duplicated across Login.aspx.cs/CustomerSignUp.aspx.cs/ForgotPassword.aspx.cs/
    // ResetPassword.aspx.cs. Lives in the website's App_Code (not MasterAntiqueRepairData
    // like the other Services) because it needs the per-request ApplicationUserManager/
    // ApplicationSignInManager (OWIN/HttpContext), neither of which the class library has
    // access to. RepairAuthHelper's RequireRole/GetCurrentUserId/SignOut stay exactly
    // where they are - they're stateless, cross-cutting guards used by every protected
    // page, not business logic specific to the auth flows themselves.
    public class AuthService : IDisposable
    {
        private readonly RepairShopContext _db;
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationSignInManager _signInManager;

        public AuthService()
        {
            var owinContext = HttpContext.Current.GetOwinContext();
            _db = owinContext.Get<RepairShopContext>();
            _userManager = owinContext.GetUserManager<ApplicationUserManager>();
            _signInManager = owinContext.Get<ApplicationSignInManager>();
        }

        // Throws InvalidOperationException with the exact friendly message the page
        // already showed for every failure case; signs the user in via RepairAuthHelper
        // and returns them on success.
        public User Login(string username, string password, string ipAddress)
        {
            if (IpThrottle.IsBlocked("login", ipAddress))
            {
                throw new InvalidOperationException("Too many login attempts from this location. Please try again later.");
            }

            var user = _userManager.FindByName(username);

            if (user != null && user.IsDeleted)
            {
                IpThrottle.RecordAttempt("login", ipAddress);
                throw new InvalidOperationException("Invalid username or password.");
            }

            if (user != null && _userManager.IsLockedOut(user.Id))
            {
                IpThrottle.RecordAttempt("login", ipAddress);
                throw new InvalidOperationException("This account is temporarily locked due to repeated failed login attempts. Please try again later.");
            }

            if (user != null && _userManager.CheckPassword(user, password))
            {
                _userManager.ResetAccessFailedCount(user.Id);

                AuditLogger.Log(_db, user, AuditLog.ActionType.Login, AuditLog.EntityKind.User, user.Id);
                _db.SaveChanges();

                RepairAuthHelper.SignIn(user, isPersistent: false);
                return user;
            }

            IpThrottle.RecordAttempt("login", ipAddress);
            if (user != null)
            {
                // Increments AccessFailedCount and locks the account out once it reaches
                // IdentityConfig's MaxFailedAccessAttemptsBeforeLockout.
                _userManager.AccessFailed(user.Id);
            }

            throw new InvalidOperationException("Invalid username or password.");
        }

        public Customer SignUp(string username, string password, string ipAddress)
        {
            if (IpThrottle.IsBlocked("signup", ipAddress))
            {
                throw new InvalidOperationException("Too many sign-up attempts from this location. Please try again later.");
            }

            var customer = new Customer { UserName = username, CreatedAt = DateTime.Now };
            var result = _userManager.Create(customer, password);
            if (!result.Succeeded)
            {
                // Rate-limited (not message-obscured): usernames in this app aren't secret
                // (visible in Manager views and the Audit Log), so the meaningful defense
                // is capping how fast an IP can sweep through candidate usernames.
                IpThrottle.RecordAttempt("signup", ipAddress);
                throw new ArgumentException(string.Join(" ", result.Errors));
            }

            _userManager.AddToRole(customer.Id, IdentityConfig.CustomerRole);

            AuditLogger.Log(_db, customer, AuditLog.ActionType.CreateUser, AuditLog.EntityKind.User, customer.Id);
            _db.SaveChanges();

            RepairAuthHelper.SignIn(customer, isPersistent: false);
            return customer;
        }

        // Returns null if no account exists with that username (caller shows a generic
        // "no account found" message rather than the token going missing silently).
        public PasswordResetRequest RequestPasswordReset(string username, string ipAddress)
        {
            if (IpThrottle.IsBlocked("forgotpassword", ipAddress))
            {
                throw new InvalidOperationException("Too many requests from this location. Please try again later.");
            }
            IpThrottle.RecordAttempt("forgotpassword", ipAddress);

            var user = _userManager.FindByName(username);
            if (user == null)
            {
                return null;
            }

            var code = _userManager.GeneratePasswordResetToken(user.Id);

            AuditLogger.Log(_db, user, AuditLog.ActionType.RequestPasswordReset, AuditLog.EntityKind.User, user.Id);
            _db.SaveChanges();

            return new PasswordResetRequest { UserId = user.Id, Code = code };
        }

        public bool IsResetTokenValid(int userId, string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return false;
            }
            return _userManager.FindById(userId) != null && _userManager.VerifyUserToken(userId, "ResetPassword", code);
        }

        // Throws InvalidOperationException for an invalid/expired token, ArgumentException
        // for a password that fails composition rules.
        public void ResetPassword(int userId, string code, string newPassword)
        {
            var user = _userManager.FindById(userId);
            if (user == null)
            {
                throw new InvalidOperationException("This password reset link is invalid or has expired.");
            }

            var result = _userManager.ResetPassword(userId, code, newPassword);
            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.IndexOf("token", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    throw new InvalidOperationException("This password reset link is invalid or has expired.");
                }
                throw new ArgumentException(string.Join(" ", result.Errors));
            }

            _userManager.ResetAccessFailedCount(userId); // proving ownership via the shown link also clears any existing account lockout

            AuditLogger.Log(_db, user, AuditLog.ActionType.ResetPassword, AuditLog.EntityKind.User, userId);
            _db.SaveChanges();
        }

        public void Dispose()
        {
            // _db/_userManager/_signInManager are all owned per-request by the OWIN
            // pipeline (Startup.Auth.cs's app.CreatePerOwinContext calls) - nothing to
            // dispose here.
        }
    }

    public class PasswordResetRequest
    {
        public int UserId { get; set; }
        public string Code { get; set; }
    }
}
