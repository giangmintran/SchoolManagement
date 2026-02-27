using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Data.Entities
{
    public class Notification
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public Guid CreatedBy { get; set; } // ID của Admin tạo thông báo

        [MaxLength(50)]
        public string? Type { get; set; } // Ví dụ: "SYSTEM", "PROMOTION"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property: 1 Thông báo có thể gửi tới nhiều User
        public virtual ICollection<NotificationUser> UserNotifications { get; set; } = new List<NotificationUser>();
    }
}
