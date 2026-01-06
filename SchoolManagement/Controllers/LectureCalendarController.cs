using Microsoft.AspNetCore.Mvc;

namespace SchoolManagement.Controllers
{
    public class LectureCalendarController : Controller
    {
        public LectureCalendarController() { }
        public IActionResult Index()
        {
            return View();
        }
    }
}
