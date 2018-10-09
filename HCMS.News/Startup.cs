using System;
using Microsoft.AspNetCore.Builder;
using OrchardCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;


namespace HCMS.News
{
    public class Startup : StartupBase
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute
            (
                name: "HCMSNews",
                areaName: "HCMSNews",
                template: ""
                //defaults: new { controller = "HCMSNews", action = "Index" }
            );
        }
    }
}
