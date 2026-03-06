using SchoolManagement.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagement.Models.ReadModels
{
    public class NotificationUserRM
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public Guid NotificationId { get; set; }
        public int Type { get; set; }
        public string? Sender { get; set; }
        public string? Title { get; set; }
        public bool IsRead { get; set; } = false;

        public DateTime? ReadAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? RedirectUrl { get; set; }

        // Navigation properties
        [ForeignKey(nameof(NotificationId))]
        public virtual NotificationType NotificationType { get; set; } = null!;
    }
}
