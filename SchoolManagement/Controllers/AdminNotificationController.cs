using Microsoft.AspNetCore.Mvc;

namespace SchoolManagement.Controllers
{
    public class AdminNotificationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
