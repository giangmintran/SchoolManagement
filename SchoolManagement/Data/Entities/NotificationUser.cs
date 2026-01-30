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

        // Tên người gửi hoặc hệ thống (ví dụ: "Hệ thống SchoolMgt" [cite: 4] hoặc "Nguyễn Văn A" [cite: 7])
        public string SenderName { get; set; }

        // Ảnh đại diện người gửi [cite: 3, 6]
        public string SenderAvatar { get; set; }

        // Nội dung thông báo [cite: 4, 7, 10]
        public string Content { get; set; }

        // Loại thông báo để hiển thị Icon Badge (Primary, Success, Warning...) 
        public NotificationType Type { get; set; }

        // Thời gian gửi 
        public DateTime CreatedAt { get; set; }

        // Trạng thái đã đọc hay chưa (để hiển thị .notif-unread-dot) [cite: 5, 8]
        public bool IsRead { get; set; }

        // Đường dẫn khi người dùng nhấn vào thông báo
        public string RedirectUrl { get; set; }
    }
}
