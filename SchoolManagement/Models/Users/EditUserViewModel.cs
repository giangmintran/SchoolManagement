using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Models.Users
{
    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Vai trò")]
        public string SelectedRole { get; set; }
    }
}