using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Data.Entities;

namespace SchoolManagement.Controllers
{
    [Route("api/[controller]")]
    public class NotificationsController : Controller
    {
        // Mock data hoặc gọi từ Repository
        [HttpGet]
        public IActionResult GetNotifications()
        {
            // Giả sử lấy 5 thông báo mới nhất như badge hiển thị 
            var notifications = new List<NotificationUser>
            {
                new NotificationUser {
                    SenderName = "Hệ thống SchoolMgt",
                    Content = "đã cập nhật chính sách bảo mật mới.",
                    Type = NotificationType.System,
                    CreatedAt = DateTime.Now.AddMinutes(-5),
                    IsRead = false
                },
            };

            return PartialView("_NotificationPartial", notifications);
        }

        [HttpPost("mark-as-read/{id}")]
        public IActionResult MarkAsRead(int id)
        {
            // Logic cập nhật IsRead = true trong Database
            return Ok();
        }
    }
}
