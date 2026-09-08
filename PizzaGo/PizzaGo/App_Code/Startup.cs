using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PizzaGo.Startup))]
namespace PizzaGo
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
