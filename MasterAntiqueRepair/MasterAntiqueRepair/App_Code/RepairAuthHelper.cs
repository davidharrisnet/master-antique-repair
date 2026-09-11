using System.Data.Entity.Core.Objects;
using System.Security.Claims;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace MasterAntiqueRepair
{
    public static class RepairAuthHelper
    {
        public static void SignIn(User user, bool isPersistent)
        {
            var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;

            // EF6 returns a dynamic proxy subclass for lazy-loading/change-tracking, so
            // user.GetType() alone would give something like "Manager_A1B2C3D4" instead
            // of "Manager" - ObjectContext.GetObjectType unwraps back to the real POCO type.
            var realType = ObjectContext.GetObjectType(user.GetType());

            var identity = new ClaimsIdentity(DefaultAuthenticationTypes.ApplicationCookie);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Name, user.Name));
            identity.AddClaim(new Claim(ClaimTypes.Role, realType.Name));

            authenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, identity);
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
