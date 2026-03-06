using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data;
using SchoolManagement.Data.Entities;
using SchoolManagement.Models.ReadModels;
using SchoolManagement.Models.ViewModels;
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
                    Title = e.NotificationType.Title,
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


        public async Task<IActionResult> Index(string search, int type = 0)
        {
            // Lấy UserId hiện tại
            var userId = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var query = await _dbContext.NotificationUsers
                .Include(e => e.NotificationType)
                .OrderByDescending(e => e.NotificationType.CreatedAt)
                .Where(e => e.UserId == userId
                    && (type == 0 || e.NotificationType.Type == type)
                    && (search == null || e.NotificationType.Title.ToLower().Contains(search.ToLower()))
                )
                .Select(e => new NotificationUserRM
                {
                    Id = e.Id,
                    UserId = userId,
                    NotificationType = e.NotificationType,
                    CreatedAt = e.NotificationType.CreatedAt,
                    Title = e.NotificationType.Title,
                    IsRead = e.IsRead,
                    ReadAt = e.ReadAt,
                    RedirectUrl = e.NotificationType.RedirectUrl,
                    Sender = e.NotificationType.CreatedBy,
                    Type = e.NotificationType.Type
                })
                .ToListAsync();

            return View(query);
        }
        public async Task<IActionResult> Detail(Guid id)
        {
            // Lấy NotificationUser kèm theo NotificationType thông qua Navigation Property
            var notificationUser = await _dbContext.NotificationUsers
                .Include(nu => nu.NotificationType)
                .FirstOrDefaultAsync(nu => nu.Id == id);

            if (notificationUser == null)
            {
                return NotFound();
            }

            // Đánh dấu đã đọc nếu chưa đọc (Tùy chọn: Thường xem detail thì sẽ tính là đã đọc)
            if (!notificationUser.IsRead)
            {
                notificationUser.IsRead = true;
                notificationUser.ReadAt = DateTime.Now;
                await _dbContext.SaveChangesAsync();
            }

            // Map sang ViewModel
            var viewModel = new NotificationDetailViewModel
            {
                NotificationUserId = notificationUser.Id,
                IsRead = notificationUser.IsRead,
                ReadAt = notificationUser.ReadAt,
                UserId = notificationUser.UserId,

                Title = notificationUser.NotificationType.Title,
                Content = notificationUser.NotificationType.Content,
                CreatedAt = notificationUser.NotificationType.CreatedAt,
                RedirectUrl = notificationUser.NotificationType.RedirectUrl,
                CreatedBy = notificationUser.NotificationType.CreatedBy,
                // Ép kiểu int sang Enum NotificationCategory
                Category = (NotificationCategory) notificationUser.NotificationType.Type
            };

            return View(viewModel);
        }
    }
}
