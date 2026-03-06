using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data;
using SchoolManagement.Data.Entities;
using SchoolManagement.Models.ViewModels;
using System.Linq;

namespace SchoolManagement.Controllers
{
    public class NotificationManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var notifications = _context.NotificationUsers
                .Include(e => e.NotificationType)
                .OrderByDescending(e => e.NotificationType.CreatedAt)
                .ToList();
            return View(notifications);
        }

        // Action mới: Xử lý tìm kiếm trả về JSON
        [HttpGet]
        public IActionResult SearchUsers(string keyword)
        {
            var query = _context.Users.Where(e => e.UserName != "admin@gmail.com").AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(e => e.UserName.Contains(keyword));
            }

            // Lấy ra danh sách (Nên giới hạn số lượng bằng Take() nếu db lớn)
            var userList = query.Select(e => new
            {
                id = e.Id,
                name = e.UserName
            }).Take(50).ToList();

            return Json(userList);
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Không cần truyền ViewBag.UserList nữa vì View sẽ tự gọi AJAX khi load
            return View(new CreateNotificationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateNotificationViewModel model)
        {
            if (ModelState.IsValid && model.SelectedUserIds != null && model.SelectedUserIds.Any())
            {
                try
                {
                    var notificationsToInsert = new List<NotificationUser>();

                    foreach (var userId in model.SelectedUserIds)
                    {
                        var notification = new NotificationUser
                        {
                            UserId = userId,
                            Type = model.Type,
                            Content = model.Content,
                            RedirectUrl = model.RedirectUrl,
                            SenderName = "Admin",
                            SenderAvatar = null,
                            CreatedAt = DateTime.UtcNow,
                            IsRead = false
                        };
                        notificationsToInsert.Add(notification);
                    }

                    _context.NotificationUsers.AddRange(notificationsToInsert);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Đã gửi thông báo thành công tới {model.SelectedUserIds.Count} người dùng!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi lưu: " + ex.Message);
                }
            }

            if (model.SelectedUserIds == null || model.SelectedUserIds.Count == 0)
            {
                ModelState.AddModelError("SelectedUserIds", "Vui lòng chọn ít nhất 1 người nhận.");
            }

            // Trả về View cùng model (View sẽ tự render lại danh sách ID đã chọn thông qua JS)
            return View(model);
        }
    }
}