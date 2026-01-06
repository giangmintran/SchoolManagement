using Microsoft.AspNetCore.Mvc;

namespace SchoolManagement.Controllers
{
    public class StarterBookController : Controller
    {
        public StarterBookController() { }
        public IActionResult Index()
        {
            return View();
        }
    }
}
