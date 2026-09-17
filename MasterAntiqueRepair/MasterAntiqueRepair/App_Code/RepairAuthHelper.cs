using System.Security.Claims;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace MasterAntiqueRepair
{
    public static class RepairAuthHelper
    {
        // ApplicationSignInManager.SignIn builds the ClaimsIdentity itself (NameIdentifier,
        // UserName, SecurityStamp, and a Role claim per AspNetUserRoles row - see
        // AccountService.AddEmployee/AddCustomer and AuthService.SignUp, which call
        // UserManager.AddToRole right after creating the account) and hands it to the
        // OWIN cookie middleware configured in Startup.Auth.cs.
        public static void SignIn(User user, bool isPersistent)
        {
            var signInManager = HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>();
            signInManager.SignIn(user, isPersistent, rememberBrowser: false);
        }

        public static void SignOut()
        {
            HttpContext.Current.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        }

        public static int? GetCurrentUserId()
        {
            var identity = HttpContext.Current.User.Identity as ClaimsIdentity;
            if (identity == null || !identity.IsAuthenticated)
            {
                return null;
            }

            var claim = identity.FindFirst(ClaimTypes.NameIdentifier);
            int id;
            if (claim != null && int.TryParse(claim.Value, out id))
            {
                return id;
            }
            return null;
        }

        /// <summary>
        /// Redirects to Login if the current visitor isn't authenticated in the given role.
        /// Returns false when it redirected, so the caller should stop processing.
        /// </summary>
        public static bool RequireRole(HttpResponse response, string role)
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated || !HttpContext.Current.User.IsInRole(role))
            {
                response.Redirect("~/Account/Login");
                return false;
            }
            return true;
        }
    }
}
