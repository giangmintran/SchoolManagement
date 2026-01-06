using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Models.Users
{
    public class CreateUserViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string SelectedRole { get; set; }
    }
}
