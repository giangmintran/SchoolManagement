using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagement.Data.Entities;

namespace SchoolManagement.Models.ViewModels
{
    public class CreateNotificationViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề thông báo.")]
        [MaxLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự.")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập nội dung thông báo.")]
        [Display(Name = "Nội dung")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn loại thông báo.")]
        [Display(Name = "Loại thông báo")]
        public int Type { get; set; }
        [Display(Name = "URL chuyển hướng (nếu có)")]
        public string? RedirectUrl { get; set; }

        [Display(Name = "Gửi đến tất cả người dùng")]
        public bool SendToAll { get; set; } = true;

        // Danh sách ID người dùng được chọn từ giao diện
        [Display(Name = "Chọn người nhận")]
        public List<string> SelectedUserIds { get; set; } = new List<string>();

        // Danh sách hiển thị lên Dropdown (không cần bind khi POST lên)
        public List<SelectListItem> AvailableUsers { get; set; } = new List<SelectListItem>();
    }
}