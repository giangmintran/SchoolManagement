namespace SchoolManagement.Data.Entities
{
    public enum NotificationType
    {
        System,   // Tương ứng bi-shield-lock-fill
        Feedback, // Tương ứng bi-chat-fill
        Warning   // Tương ứng bi-exclamation-triangle-fill
    }
    public class NotificationUser
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        // Tên người gửi hoặc hệ thống (ví dụ: "Hệ thống SchoolMgt" hoặc "Nguyễn Văn A" 
        public string? SenderName { get; set; }

        // Ảnh đại diện người gửi
        public string? SenderAvatar { get; set; }

        // Nội dung thông báo
        public string? Content { get; set; }

        // Loại thông báo để hiển thị Icon Badge (Primary, Success, Warning...) 
        public NotificationType Type { get; set; }

        // Thời gian gửi 
        public DateTime CreatedAt { get; set; }

        // Trạng thái đã đọc hay chưa (để hiển thị .notif-unread-dot)
        public bool IsRead { get; set; }

        // Đường dẫn khi người dùng nhấn vào thông báo
        public string? RedirectUrl { get; set; }
    }
}
