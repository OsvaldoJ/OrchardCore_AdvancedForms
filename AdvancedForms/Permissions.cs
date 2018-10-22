using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace AdvancedForms
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageAdvancedForms = new Permission("ManageAdvancedFormsContent", "Manage AdvancedForms");
        public static readonly Permission ManageOwnAdvancedForms = new Permission("ManageOwnAdvancedForms", "Manage Own AdvancedForms", new[] { ManageAdvancedForms });
        public static readonly Permission ViewContent = new Permission("ViewContent", "View all content", new[] { ManageAdvancedForms });
        public static readonly Permission ViewOwnContent = new Permission("ViewOwnContent", "View own content", new[] { ViewContent });

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] {
                ManageAdvancedForms,
                ManageOwnAdvancedForms,
                ViewContent,
                ViewOwnContent
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ManageAdvancedForms }
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] { ManageAdvancedForms }
                },
                new PermissionStereotype {
                    Name = "Moderator",
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] { ManageOwnAdvancedForms }
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new[] { ManageOwnAdvancedForms }
                },
                 new PermissionStereotype {
                    Name = "Authenticated",
                    Permissions = new[] {ViewContent}
                },
                new PermissionStereotype {
                    Name = "Anonymous",
                    Permissions = new[] {ViewContent}
                }
            };
        }
    }
}