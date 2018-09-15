using System;
using Microsoft.AspNetCore.Builder;
using OrchardCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace CustomHCMS
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
           // if (string.IsNullOrEmpty(_configuration["Sample"]))
            //{
             //   throw new Exception(":(");
           //}

            routes.MapAreaRoute
            (
                name: "CustomHCMS",
                areaName: "CustomHCMS",
                template: "",
                defaults: new { controller = "CustomHCMS", action = "Index" }
            );
        }
    }
}

