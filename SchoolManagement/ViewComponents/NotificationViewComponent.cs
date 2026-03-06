using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data;
using SchoolManagement.Models.ReadModels;
using System.Security.Claims;

namespace SchoolManagement.ViewComponents
{
    public class NotificationViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public NotificationViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Lấy UserId hiện tại
            var userId = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Lấy danh sách thông báo của user này
            var notifications = await _context.NotificationUsers
                .Include(e => e.NotificationType)
                .Where(n => n.UserId == userId)
                .OrderByDescending(e => e.NotificationType.CreatedAt)
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
            return View(notifications);
        }
    }
}