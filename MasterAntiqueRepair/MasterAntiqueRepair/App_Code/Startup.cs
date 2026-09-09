using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MasterAntiqueRepair.Startup))]
namespace MasterAntiqueRepair
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
