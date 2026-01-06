using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SchoolManagement.Data;
using SchoolManagement.Models; // Nhớ using namespace chứa ApplicationUser
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

namespace SchoolManagement.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        // 1. Đổi IdentityUser thành ApplicationUser
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _environment;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _environment = environment;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Số điện thoại")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Địa chỉ")]
            public string Address { get; set; } // Thêm Address vào Input

            [Display(Name = "Ảnh đại diện")]
            public IFormFile ProfilePicture { get; set; }
        }

        public string CurrentProfilePicture { get; set; }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            // Load ảnh từ DB, nếu null thì lấy ảnh mặc định
            CurrentProfilePicture = !string.IsNullOrEmpty(user.Avatar)
                                    ? user.Avatar
                                    : "/images/default-avatar.png";

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                Address = user.Address // Load địa chỉ từ DB lên form
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // 1. XỬ LÝ UPLOAD ẢNH
            if (Input.ProfilePicture != null)
            {
                // Xóa ảnh cũ nếu không phải ảnh mặc định (Optional - tùy nhu cầu)
                // if (!string.IsNullOrEmpty(user.ProfilePicture) && System.IO.File.Exists(Path.Combine(_environment.WebRootPath, user.ProfilePicture.TrimStart('/')))) ...

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(Input.ProfilePicture.FileName);
                string uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "avatars");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                string filePath = Path.Combine(uploadFolder, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.ProfilePicture.CopyToAsync(fileStream);
                }

                // Cập nhật đường dẫn vào object User
                user.Avatar = "/uploads/avatars/" + fileName;
            }

            // 2. XỬ LÝ ĐỊA CHỈ
            if (Input.Address != user.Address)
            {
                user.Address = Input.Address;
            }

            // 3. LƯU THAY ĐỔI VÀO DATABASE (Address + ProfilePicture)
            // Chỉ cần gọi UpdateAsync một lần là lưu hết các property của user
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                StatusMessage = "Lỗi không mong muốn khi cập nhật hồ sơ.";
                return RedirectToPage();
            }

            // 4. XỬ LÝ SỐ ĐIỆN THOẠI (Logic riêng của Identity)
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Lỗi không mong muốn khi cập nhật số điện thoại.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Hồ sơ của bạn đã được cập nhật";
            return RedirectToPage();
        }
    }
}