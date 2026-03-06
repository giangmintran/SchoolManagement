using DocumentFormat.OpenXml.Wordprocessing;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Data.Entities
{
    public enum NotificationCategory
    {
        [Display(Name = "Thông báo hệ thống")]
        System = 1,
        [Display(Name = "Nhắc nhở")]
        Warning = 2,
        [Display(Name = "Khác")]
        Other = 3
    }
    
    public enum NotificationStatus
    {
        NotSend = 1,
        Sent = 2
    }
    public class NotificationType
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string CreatedBy { get; set; } // ID của Admin tạo thông báo

        public int Type { get; set; } // Ví dụ: "SYSTEM", "PROMOTION"
        public string? RedirectUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
        public NotificationStatus Status { get; set; }

        // Navigation property: 1 Thông báo có thể gửi tới nhiều User
        public virtual ICollection<NotificationUser> UserNotifications { get; set; } = new List<NotificationUser>();
    }
}
