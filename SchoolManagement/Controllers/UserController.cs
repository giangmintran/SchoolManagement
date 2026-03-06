
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
            int pageSize = 10;

            // 1. Tạo Query
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(u => u.Email.Contains(keyword) || u.UserName.Contains(keyword));
            }

            // 2. Dùng Extension Method để phân trang trên Entity (ApplicationUser) trước
            //    Lợi ích: Tự động Count và Skip/Take tối ưu tại Database
            var pagedUsers = await query.ToPagedResultAsync(pageNumber, pageSize);

            // 3. Mapping dữ liệu từ Entity sang ViewModel
            //    Vì cần gọi hàm async GetRolesAsync nên phải loop thủ công ở đây
            var resultItems = new List<UserRolesViewModel>();

            foreach (var user in pagedUsers.Items)
            {
                if (user.UserName == "admin@gmail.com") continue;

                var thisViewModel = new UserRolesViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Avatar = user.Avatar,
                    Roles = await _userManager.GetRolesAsync(user)
                };
                resultItems.Add(thisViewModel);
            }

            // 4. Tạo PagedResult cho ViewModel dựa trên thông tin trang của Entity
            var viewModel = new PagedResult<UserRolesViewModel>
            {
                Items = resultItems,
                PageIndex = pagedUsers.PageIndex,
                TotalPages = pagedUsers.TotalPages,
                TotalRecords = pagedUsers.TotalRecords
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, EmailConfirmed = true };
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

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Lấy role hiện tại của user
            var userRoles = await _userManager.GetRolesAsync(user);
            var currentRole = userRoles.FirstOrDefault();

            // Tạo ViewModel
            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                SelectedRole = currentRole
            };

            // Đổ dữ liệu vào ViewBag cho Dropdown Role
            ViewBag.Roles = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name", currentRole);

            return View(model);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null) return NotFound();

                // 1. Cập nhật Email (và UserName nếu muốn đồng bộ)
                user.Email = model.Email;
                user.UserName = model.Email; // Thường Email là UserName

                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    // Reload lại Role nếu lỗi
                    ViewBag.Roles = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name", model.SelectedRole);
                    return View(model);
                }

                // 2. Cập nhật Role
                var userRoles = await _userManager.GetRolesAsync(user);
                var currentRole = userRoles.FirstOrDefault();

                if (model.SelectedRole != currentRole)
                {
                    // Xóa role cũ (nếu có)
                    if (!string.IsNullOrEmpty(currentRole))
                    {
                        await _userManager.RemoveFromRoleAsync(user, currentRole);
                    }
                    // Thêm role mới (nếu có chọn)
                    if (!string.IsNullOrEmpty(model.SelectedRole))
                    {
                        await _userManager.AddToRoleAsync(user, model.SelectedRole);
                    }
                }

                TempData["Message"] = "Cập nhật thông tin thành công!";
                return RedirectToAction(nameof(Index)); // Hoặc quay lại trang Edit
            }

            // Nếu Model không valid, load lại ViewBag
            ViewBag.Roles = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name", model.SelectedRole);
            return View(model);
        }

        // POST: Users/ResetPassword
        // Action này nhận dữ liệu từ Modal trong View Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string userId, string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                TempData.ToastError("Mật khẩu phải dài ít nhất 6 ký tự.");
                return RedirectToAction("Edit", new { id = userId });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Cách 1: Dùng GeneratePasswordResetTokenAsync (Chuẩn)
            // Tạo token reset mật khẩu
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            // Thực hiện đổi mật khẩu
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                TempData.ToastSuccess("Đã đặt lại mật khẩu thành công!");
            }
            else
            {
                // Nối các lỗi lại thành chuỗi để hiển thị
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                TempData.ToastError($"Lỗi đổi mật khẩu: {errors}");
            }

            return RedirectToAction("Edit", new { id = userId });
        }

        // 6. Xóa người dùng
        [HttpPost]
        [ValidateAntiForgeryToken]
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