using Microsoft.AspNetCore.Mvc;

namespace SchoolManagement.Controllers
{
    public class CommonController : Controller
    {
        public IActionResult FeatureInDevelopment()
        {
            return View();
        }
    }

}
