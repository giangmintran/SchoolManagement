using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Models.Users
{
    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Role")]
        public string SelectedRole { get; set; } // Role hiện tại hoặc Role mới muốn gán
    }
}
