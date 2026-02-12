using Microsoft.AspNetCore.Mvc;

namespace SchoolManagement.Controllers
{
    public class NotificationManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
