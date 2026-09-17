using System;
using System.Data.Entity.Infrastructure;

namespace MasterAntiqueRepair
{
    // Consolidates the login/signup/password-reset business logic that used to be
    // duplicated across Login.aspx.cs/CustomerSignUp.aspx.cs/ForgotPassword.aspx.cs/
    // ResetPassword.aspx.cs. Lives in the website's App_Code (not MasterAntiqueRepairData
    // like the other Services) because it needs RepairAuthHelper.SignIn (OWIN/HttpContext)
    // and IpThrottle (per-request IP), neither of which the class library has access to.
    // RepairAuthHelper's RequireRole/GetCurrentUserId/SignOut stay exactly where they are -
    // they're stateless, DbContext-free, cross-cutting guards used by every protected
    // page, not business logic specific to the auth flows themselves.
    public class AuthService : IDisposable
    {
        private readonly RepairShopContext _db;
        private readonly UserRepository _users;
        private readonly PasswordResetTokenRepository _resetTokens;

        public AuthService()
        {
            _db = new RepairShopContext();
            _users = new UserRepository(_db);
            _resetTokens = new PasswordResetTokenRepository(_db);
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

            var user = _users.GetByName(username);

            if (user != null && user.IsDeleted)
            {
                IpThrottle.RecordAttempt("login", ipAddress);
                throw new InvalidOperationException("Invalid username or password.");
            }

            if (user != null && user.IsLockedOut())
            {
                IpThrottle.RecordAttempt("login", ipAddress);
                throw new InvalidOperationException("This account is temporarily locked due to repeated failed login attempts. Please try again later.");
            }

            if (user != null && user.VerifyPassword(password))
            {
                user.RecordSuccessfulLogin();
                _db.SaveChanges();

                AuditLogger.Log(_db, user, AuditLog.ActionType.Login, AuditLog.EntityKind.User, user.Id);
                _db.SaveChanges();

                RepairAuthHelper.SignIn(user, isPersistent: false);
                return user;
            }

            IpThrottle.RecordAttempt("login", ipAddress);
            if (user != null)
            {
                user.RecordFailedLogin();
                _db.SaveChanges();
            }

            throw new InvalidOperationException("Invalid username or password.");
        }

        public Customer SignUp(string username, string password, string ipAddress)
        {
            if (IpThrottle.IsBlocked("signup", ipAddress))
            {
                throw new InvalidOperationException("Too many sign-up attempts from this location. Please try again later.");
            }

            if (_users.ExistsActiveByName(username))
            {
                // Rate-limited (not message-obscured): usernames in this app aren't secret
                // (visible in Manager views and the Audit Log), so the meaningful defense
                // is capping how fast an IP can sweep through candidate usernames.
                IpThrottle.RecordAttempt("signup", ipAddress);
                throw new ArgumentException("That username is already taken.");
            }

            var customer = new Customer { Name = username, CreatedAt = DateTime.Now };
            customer.SetPassword(password);

            _users.Add(customer);

            try
            {
                _db.SaveChanges();
            }
            catch (DbUpdateException ex) when (IsDuplicateUsernameViolation(ex))
            {
                IpThrottle.RecordAttempt("signup", ipAddress);
                throw new ArgumentException("That username is already taken.");
            }

            AuditLogger.Log(_db, customer, AuditLog.ActionType.CreateUser, AuditLog.EntityKind.User, customer.Id);
            _db.SaveChanges();

            RepairAuthHelper.SignIn(customer, isPersistent: false);
            return customer;
        }

        // Returns null if no account exists with that username (caller shows a generic
        // "no account found" message rather than the token going missing silently).
        public PasswordResetToken RequestPasswordReset(string username, string ipAddress)
        {
            if (IpThrottle.IsBlocked("forgotpassword", ipAddress))
            {
                throw new InvalidOperationException("Too many requests from this location. Please try again later.");
            }
            IpThrottle.RecordAttempt("forgotpassword", ipAddress);

            var user = _users.GetByName(username);
            if (user == null)
            {
                return null;
            }

            var token = PasswordResetToken.Create(user);
            _resetTokens.Add(token);
            _db.SaveChanges();

            AuditLogger.Log(_db, user, AuditLog.ActionType.RequestPasswordReset, AuditLog.EntityKind.User, user.Id);
            _db.SaveChanges();

            return token;
        }

        public bool IsResetTokenValid(string token)
        {
            var resetToken = _resetTokens.GetByToken(token);
            return resetToken != null && resetToken.IsValid();
        }

        // Throws InvalidOperationException for an invalid/expired token, ArgumentException
        // for a password that fails composition rules.
        public void ResetPassword(string token, string newPassword)
        {
            var resetToken = _resetTokens.GetByToken(token);
            if (resetToken == null || !resetToken.IsValid())
            {
                throw new InvalidOperationException("This password reset link is invalid or has expired.");
            }

            var user = _users.GetById(resetToken.UserId);
            if (user == null)
            {
                throw new InvalidOperationException("This password reset link is invalid or has expired.");
            }

            user.SetPassword(newPassword);

            resetToken.UsedAt = DateTime.Now;
            user.RecordSuccessfulLogin(); // proving ownership via the shown link also clears any existing account lockout
            _db.SaveChanges();

            AuditLogger.Log(_db, user, AuditLog.ActionType.ResetPassword, AuditLog.EntityKind.User, user.Id);
            _db.SaveChanges();
        }

        private static bool IsDuplicateUsernameViolation(DbUpdateException ex)
        {
            var sqlEx = ex.GetBaseException() as System.Data.SqlClient.SqlException;
            return sqlEx != null && (sqlEx.Number == 2601 || sqlEx.Number == 2627);
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
