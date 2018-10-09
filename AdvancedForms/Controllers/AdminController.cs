using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;

namespace AdvancedForms.Controllers
{
    [Admin]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            //if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOwnAdvancedForms))
            //{
            //    return Unauthorized();
            //}

            return View();
        }
    }
}
