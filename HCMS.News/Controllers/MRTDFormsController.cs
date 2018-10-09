using Microsoft.AspNetCore.Mvc;


namespace HCMS.Controllers
{
    public class MRTDFormsController : Controller
    {
        [Route("HCMS.Forms")]
        [Route("HCMS.News/HCMSNews")]
        [Route("HCMS.News/HCMSNews/Index")]
        [Route("HCMSNews")]        
        [Route("HCMSNews/Index")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("HCMSNews/ManualForms")]
        public IActionResult ManualForms()
        {
            
            return View();
        }
    }
}
