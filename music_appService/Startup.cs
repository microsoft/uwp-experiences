using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(music_appService.Startup))]

namespace music_appService
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMobileApp(app);
            app.MapSignalR();
        }
    }
}