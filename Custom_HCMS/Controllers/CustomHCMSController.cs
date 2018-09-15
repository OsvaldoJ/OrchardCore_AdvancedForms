using Microsoft.AspNetCore.Mvc;

namespace CustomHCMS.Controllers
{
    public class CustomHCMSController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
