using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data; // Thay đổi theo namespace chứa ApplicationDbContext của bạn
using SchoolManagement.Data.Entities;
using SchoolManagement.Hubs;
using SchoolManagement.Models.ViewModels;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SchoolManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminNotificationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public AdminNotificationController(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext; 
        }

        // GET: /AdminNotification/Index
        public async Task<IActionResult> Index(string search, int? type)
        {
            var query = _context.NotificationTypes
                // Eager loading UserNotifications để view gọi hàm .Count
                .Include(n => n.UserNotifications)
                .AsQueryable();

            // 1. Xử lý tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(n => n.Title.Contains(search) || n.Content.Contains(search));
            }

            // 2. Xử lý lọc theo Type (1: System, 2: Promotion,...)
            if (type.HasValue)
            {
                query = query.Where(n => n.Type == type.Value);
            }

            // 3. Thực thi query lấy dữ liệu mới nhất lên đầu
            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return View(notifications);
        }

        // GET: /AdminNotification/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            // Lấy thông báo kèm theo danh sách User đã nhận
            var notification = await _context.NotificationTypes
                .Include(n => n.UserNotifications)
                    .ThenInclude(nu => nu.User) // Join sang bảng ApplicationUser để lấy thông tin người dùng
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // POST: /AdminNotification/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var notification = await _context.NotificationTypes.FindAsync(id);
            if (notification != null)
            {
                _context.NotificationTypes.Remove(notification);
                // Bảng NotificationUser sẽ tự động bị xóa nhờ Cascade Delete (nếu đã config foreign key chuẩn)
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /AdminNotification/Create
        // GET: /AdminNotification/Create
        public async Task<IActionResult> Create()
        {
            var model = new CreateNotificationViewModel();

            // Lấy danh sách user để hiển thị lên dropdown. 
            // Tùy theo project của bạn, có thể dùng thuộc tính FullName hoặc UserName
            model.AvailableUsers = await _context.Users
                .Where(e => e.UserName != "admin@gmail.com")
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.UserName // Hoặc u.FullName
                }).ToListAsync();

            return View(model);
        }

        // POST: /AdminNotification/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateNotificationViewModel model)
        {
            // Custom Validation: Nếu không gửi cho tất cả thì phải chọn ít nhất 1 người
            if (!model.SendToAll && (model.SelectedUserIds == null || !model.SelectedUserIds.Any()))
            {
                ModelState.AddModelError("SelectedUserIds", "Vui lòng chọn ít nhất một người nhận.");
            }

            if (!ModelState.IsValid)
            {
                model.AvailableUsers = await _context.Users
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = u.UserName
                    }).ToListAsync();

                return View(model);
            }
            var userId = User.FindFirstValue(ClaimTypes.Name);
            // 1. Tạo NotificationType
            var notification = new NotificationType
            {
                Id = Guid.NewGuid(),
                Title = model.Title,
                Content = model.Content,
                Type = model.Type,
                Status = NotificationStatus.Sent,
                CreatedAt = DateTime.Now,
                CreatedBy = userId
            };

            _context.NotificationTypes.Add(notification);

            // 2. Xác định danh sách ID người nhận
            List<string> targetUserIds;

            if (model.SendToAll)
            {
                // Lấy tất cả user ID
                targetUserIds = await _context.Users.Select(u => u.Id).ToListAsync();
            }
            else
            {
                // Chỉ lấy những user ID được chọn từ form
                targetUserIds = model.SelectedUserIds;
            }

            // 3. Tạo các bản ghi NotificationUser
            if (targetUserIds.Any())
            {
                var userNotifications = targetUserIds.Select(userId => new NotificationUser
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    NotificationId = notification.Id,
                    IsRead = false
                });

                _context.NotificationUsers.AddRange(userNotifications);
            }

            await _context.SaveChangesAsync();
            // --- BẮT ĐẦU LOGIC PUSH NOTIFICATION ---
            var pushData = new
            {
                title = notification.Title,
                content = notification.Content,
                createdAt = notification.CreatedAt.ToString("dd/MM/yyyy HH:mm")
            };

            if (model.SendToAll)
            {
                // Gửi cho tất cả mọi người đang online
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", pushData);
            }
            else
            {
                // Gửi đích danh cho các User được chọn
                // Lưu ý: SignalR mặc định dùng UserId của Identity để định danh connection
                await _hubContext.Clients.Users(targetUserIds).SendAsync("ReceiveNotification", pushData);
            }
            // --- KẾT THÚC LOGIC PUSH NOTIFICATION ---
            TempData["SuccessMessage"] = "Tạo và gửi thông báo thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}