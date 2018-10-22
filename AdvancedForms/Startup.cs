using System;
using Microsoft.AspNetCore.Builder;
using OrchardCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Navigation;
using OrchardCore.Security.Permissions;
using Microsoft.Extensions.DependencyInjection;

namespace AdvancedForms
{
    public class Startup : StartupBase
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute
            (
                name: "AdvancedForms",
                areaName: "AdvancedForms",
                template: "",
                defaults: new { controller = "AdvancedForms", action = "Index" }
            );

            routes.MapAreaRoute(
                name: "DisplayAdvancedForm",
                areaName: "AdvancedForms",
                template: "AdvancedForms/{alias}",
                defaults: new { controller = "AdvancedForms", action = "Display" }
            );
        }
    }
}
