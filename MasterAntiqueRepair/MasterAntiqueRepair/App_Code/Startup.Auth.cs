using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

namespace MasterAntiqueRepair
{
    public partial class Startup {

        public void ConfigureAuth(IAppBuilder app)
        {
            // One RepairShopContext/ApplicationUserManager/ApplicationSignInManager per
            // request, retrieved via context.Get<T>()/GetUserManager<T>() rather than
            // `new` - the standard ASP.NET Identity OWIN wiring, so AuthService and
            // RepairAuthHelper share the same per-request context instead of each
            // opening their own.
            app.CreatePerOwinContext(RepairShopContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Cookie middleware backing ApplicationSignInManager.SignIn (via
            // RepairAuthHelper.SignIn) - the app's actual authentication mechanism,
            // sourced from the domain User/Customer/Employee/Manager model layered on
            // top of ASP.NET Identity rather than a separate ApplicationUser.
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                CookieHttpOnly = true,
                // SameAsRequest (not Always) so the cookie still works over the plain-HTTP
                // IIS Express dev setup this project documents - it upgrades to Secure-only
                // automatically once the site is actually served over HTTPS.
                CookieSecure = CookieSecureOption.SameAsRequest
            });
        }
    }
}
