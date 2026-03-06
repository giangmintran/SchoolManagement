using SchoolManagement.Data.Entities;

namespace SchoolManagement.Models.ViewModels
{
    public class NotificationDetailViewModel
    {
        // Thông tin từ NotificationUser
        public Guid NotificationUserId { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public string UserId { get; set; } = string.Empty;

        // Thông tin từ NotificationType
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string? RedirectUrl { get; set; }

        // Chuyển đổi Int Type sang Enum để hiển thị tên cho đẹp
        public NotificationCategory Category { get; set; }

        public string GetNotificationCategory (NotificationCategory category)
        {
            return category switch
            {
                NotificationCategory.System => "Thông báo hệ thống",
                NotificationCategory.Warning => "Thông báo nhắc nhở",
                NotificationCategory.Other => "Thông báo khác",
                _ => "Unknown"
            };
        }
    }
}
