using Microsoft.AspNetCore.Mvc;

namespace HCMS.Controllers
{
    public class HCMSNewsController : Controller
    {
        [Route("HCMS.News")]
        [Route("HCMS.News/HCMSNews")]
        [Route("HCMS.News/HCMSNews/Index")]
        [Route("HCMSNews")]        
        [Route("HCMSNews/Index")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("HCMSNews/Movies")]
        public IActionResult Movies()
        {
            return View();
        }
    }
}
