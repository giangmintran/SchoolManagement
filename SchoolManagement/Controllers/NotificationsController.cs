using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data;
using SchoolManagement.Data.Entities;
using SchoolManagement.Models.ReadModels;
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
                .Include(e => e.NotificationType)
                .OrderByDescending(e => e.NotificationType.CreatedAt)
                .Where(n => n.UserId == userId);

            // Xử lý lọc theo yêu cầu
            if (filter == "unread")
            {
                query = query.Where(n => !n.IsRead);
            }

            var notifications = await query
                .Select(e => new NotificationUserRM
                {
                    UserId = userId,
                    NotificationType = e.NotificationType,
                    CreatedAt = e.NotificationType.CreatedAt,
                    Content = e.NotificationType.Content,
                    IsRead = e.IsRead,
                    ReadAt = e.ReadAt,
                    RedirectUrl = e.NotificationType.RedirectUrl,
                    Sender = e.NotificationType.CreatedBy,
                    Type = e.NotificationType.Type
                })
                .ToListAsync();

            // Trả về Partial View đã tạo ở Bước 1
            return PartialView("_NotificationList", notifications);
        }

        [HttpPost("[controller]/mark-as-read/{id}")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var notification = await _dbContext.NotificationUsers.FirstOrDefaultAsync(e => e.Id == id && !e.IsRead);
            if (notification == null) return NotFound();

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
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
                .Include(e => e.NotificationType)
                .OrderByDescending(e => e.NotificationType.CreatedAt)
                .Where(e => e.UserId == userId)
                 .Select(e => new NotificationUserRM
                 {
                     UserId = userId,
                     NotificationType = e.NotificationType,
                     CreatedAt = e.NotificationType.CreatedAt,
                     Content = e.NotificationType.Content,
                     IsRead = e.IsRead,
                     ReadAt = e.ReadAt,
                     RedirectUrl = e.NotificationType.RedirectUrl,
                     Sender = e.User.UserName,
                     Type = e.NotificationType.Type
                 });

            //if (!string.IsNullOrEmpty(search)) query = query.Where(x => x.Content.Contains(search));
            //if (type.HasValue) query = query.Where(x => x.Type == type);
            return View(query);
        }
    }
}
