using Microsoft.Owin;
using MVC.Twitter.Models;
using Owin;

[assembly: OwinStartupAttribute(typeof(MVC.Twitter.Startup))]
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Web.config", Watch = true)]

namespace MVC.Twitter
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            
        }
    }
}
