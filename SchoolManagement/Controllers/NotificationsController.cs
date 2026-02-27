using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data;
using SchoolManagement.Data.Entities;
using System.Security.Claims;

namespace SchoolManagement.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        public NotificationsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpGet]
        public async Task<IActionResult> GetNotifications(string filter = "all")
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var query = _dbContext.NotificationUsers
                .Where(n => n.UserId == userId);

            // Xử lý lọc theo yêu cầu
            if (filter == "unread")
            {
                query = query.Where(n => !n.IsRead);
            }

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            // Trả về Partial View đã tạo ở Bước 1
            return PartialView("_NotificationList", notifications);
        }

        [HttpPost("[controller]/mark-as-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _dbContext.NotificationUsers.FindAsync(id);
            if (notification == null) return NotFound();

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                _dbContext.NotificationUsers.Update(notification);
                await _dbContext.SaveChangesAsync();
            }

            return Ok();
        }


        public async Task<IActionResult> Index(string search, NotificationType? type)
        {
            // Lấy UserId hiện tại
            var userId = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var query = _dbContext.NotificationUsers
                .OrderByDescending(n => n.CreatedAt)
                .Where(e => e.UserId == userId);

            if (!string.IsNullOrEmpty(search)) query = query.Where(x => x.Content.Contains(search));
            if (type.HasValue) query = query.Where(x => x.Type == type);
            return View(query);
        }
    }
}
