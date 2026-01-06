
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Common;
using SchoolManagement.Data;
using SchoolManagement.Models;
using SchoolManagement.Models.Users;

namespace SchoolManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // 1. Danh sách người dùng
        public async Task<IActionResult> Index(string keyword, int pageNumber = 1)
        {
            ViewData["Keyword"] = keyword;
            int pageSize = 10; // Số dòng trên 1 trang
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                // Lọc theo Email hoặc Username
                query = query.Where(u => u.Email.Contains(keyword) || u.UserName.Contains(keyword));
            }
            var totalRecords = await query.CountAsync();
            List<ApplicationUser> items = [.. query.Skip((pageNumber - 1) * pageSize).Take(pageSize)];
            var resultItems = new List<UserRolesViewModel>();
            foreach (var user in items)
            {
                if (user.UserName == "admin@gmail.com") continue; // Bỏ qua super admin nếu cần
                var thisViewModel = new UserRolesViewModel();
                thisViewModel.UserId = user.Id;
                thisViewModel.UserName = user.UserName;
                thisViewModel.Email = user.Email;
                thisViewModel.Roles = await _userManager.GetRolesAsync(user);
                resultItems.Add(thisViewModel);
            }
            var viewModel = new PagedResult<UserRolesViewModel>
            {
                Items = resultItems,
                PageIndex = pageNumber,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
            };

            return View(viewModel);
        }

        // 2. GET: Tạo người dùng
        public IActionResult Create()
        {
            // Lấy danh sách Role đẩy ra ViewBag để hiển thị Dropdown
            ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name");
            return View();
        }

        // 3. POST: Tạo người dùng
        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Gán Role cho user mới
                    if (!string.IsNullOrEmpty(model.SelectedRole))
                    {
                        await _userManager.AddToRoleAsync(user, model.SelectedRole);
                    }
                    return RedirectToAction("Index");
                }

                // Nếu lỗi (ví dụ mật khẩu yếu), thêm lỗi vào ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            // Load lại Role nếu validate sai để không bị mất dropdown
            ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name");
            return View(model);
        }

        // 4. GET: Sửa người dùng
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                SelectedRole = roles.FirstOrDefault() // Lấy role đầu tiên (nếu có)
            };

            ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name", model.SelectedRole);
            return View(model);
        }

        // 5. POST: Sửa người dùng
        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            if (ModelState.IsValid)
            {
                user.Email = model.Email;
                user.UserName = model.Email;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // Xử lý cập nhật Role
                    var currentRoles = await _userManager.GetRolesAsync(user);

                    // Nếu role chọn khác với role hiện tại
                    if (!currentRoles.Contains(model.SelectedRole))
                    {
                        // Xóa hết role cũ (để đảm bảo chỉ có 1 role - tùy logic của bạn)
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        // Thêm role mới
                        await _userManager.AddToRoleAsync(user, model.SelectedRole);
                    }

                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name", model.SelectedRole);
            return View(model);
        }

        // 6. Xóa người dùng
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
    }
}