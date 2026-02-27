using SchoolManagement.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Models.ViewModels
{
    public class CreateNotificationViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn ít nhất 1 người nhận.")]
        public List<string> SelectedUserIds { get; set; } = new List<string>();

        [Required(ErrorMessage = "Vui lòng chọn loại thông báo.")]
        public NotificationType Type { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung.")]
        public string Content { get; set; }

        public string? RedirectUrl { get; set; }
    }
}
