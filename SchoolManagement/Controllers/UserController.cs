using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Models;

namespace SchoolManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // 1. Danh sách người dùng
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRolesViewModel = new List<UserRolesViewModel>();

            foreach (var user in users)
            {
                var thisViewModel = new UserRolesViewModel();
                thisViewModel.UserId = user.Id;
                thisViewModel.UserName = user.UserName;
                thisViewModel.Email = user.Email;
                thisViewModel.Roles = await _userManager.GetRolesAsync(user);
                userRolesViewModel.Add(thisViewModel);
            }

            return View(userRolesViewModel);
        }

        // 2. Xóa người dùng
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction("Index");
        }

        // Bạn có thể thêm Action Edit để đổi Role tại đây...
    }
}
