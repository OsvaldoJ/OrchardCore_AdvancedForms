using Microsoft.Extensions.Localization;
using System;
using System.Threading.Tasks;
using OrchardCore.Environment.Navigation;

namespace AdvancedForms
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public void BuildNavigation(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            builder
                .Add(T["Content"], content => content
                    .Add(T["Advanced Forms"], "6", layers => layers
                        .Permission(Permissions.ManageOwnAdvancedForms)
                        .Action("Create", "Admin", new { area = "AdvancedForms" })
                        .LocalNav()
                    ));

            return;
        }
    }
}
