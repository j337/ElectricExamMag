using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MyProjectUI.Startup))]
namespace MyProjectUI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
