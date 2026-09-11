using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

namespace MasterAntiqueRepair
{
    public partial class Startup {

        public void ConfigureAuth(IAppBuilder app)
        {
            // Cookie middleware backing RepairAuthHelper.SignIn - the app's actual
            // authentication mechanism, sourced from the domain User/Customer/
            // Employee/Manager model rather than ASP.NET Identity.
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login")
            });
        }
    }
}
