using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

[assembly: OwinStartup(typeof(SignalRGameServer.StartUp))]

namespace SignalRGameServer
{
    public class StartUp
    {
        public void Configuration(IAppBuilder app)
        {
            //app.UseCors(CorsOptions.AllowAll); // only needed for cross origin domains
            app.MapSignalR();
        }
    }
}