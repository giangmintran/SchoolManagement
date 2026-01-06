using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Data;
using SchoolManagement.Models;
using System.Diagnostics;

namespace SchoolManagement.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public HomeController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            // Chưa đăng nhập → giữ nguyên
            if (!User.Identity!.IsAuthenticated)
                return View();

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return View();

            // Nếu là User → redirect DashboardUser
            if (await _userManager.IsInRoleAsync(user, "User"))
            {
                return RedirectToAction("DashboardUser", "Home");
            }

            // Admin vẫn truy cập /
            return View();
        }

        [Authorize(Roles = "User")]
        public IActionResult DashboardUser()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult FeatureOne()
        {
            return RedirectToAction("Common", "FeatureInDevelopment");
        }
        public IActionResult FeatureTwo()
        {
            return RedirectToAction("Common", "FeatureInDevelopment");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
