using Microsoft.AspNetCore.Identity;

namespace SchoolManagement.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string? Avatar { get; set; }   // URL hoặc path ảnh
        public string? Address { get; set; }  // Địa chỉ
    }
}
