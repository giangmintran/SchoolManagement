using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Common;
using SchoolManagement.Data;
using SchoolManagement.Data.Entities;
using System.Security.Claims;
// Nhớ using namespace chứa class PagedResult của bạn
// using SchoolManagement.Utilities; 

namespace SchoolManagement.Controllers
{
    [Authorize]
    public class ProfessionalActivitiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfessionalActivitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ProfessionalActivities
        // Đổi tên tham số thành pageIndex cho khớp với property của class PagedResult
        // GET: ProfessionalActivities
        public async Task<IActionResult> Index(string searchTerm, DateTime? fromDate, DateTime? toDate, int pageIndex = 1)
        {
            // 1. Chuẩn bị truy vấn
            var query = _context.ProfessionalActivities
                .Include(p => p.AppUser)
                .AsQueryable(); // Sử dụng AsQueryable để cộng dồn các điều kiện lọc

            // 2. Thực hiện lọc dữ liệu
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Content.Contains(searchTerm) ||
                                         p.Attendance.Contains(searchTerm) ||
                                         p.AppUser.UserName.Contains(searchTerm));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(p => p.Date >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(p => p.Date <= toDate.Value);
            }

            // Sắp xếp sau khi lọc
            query = query.OrderByDescending(p => p.Date)
                         .ThenByDescending(p => p.SequenceNumber)
                         .AsNoTracking();

            // 3. Cấu hình phân trang
            int pageSize = 10;
            int totalRecords = await query.CountAsync();

            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 4. Lưu lại giá trị lọc để hiển thị lại trên Form ở View
            ViewBag.SearchTerm = searchTerm;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            var result = new PagedResult<ProfessionalActivity>
            {
                Items = items,
                PageIndex = pageIndex,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
            };

            return View(result);
        }

        // --- CREATE ---
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProfessionalActivity professionalActivity)
        {
            // Tự động lấy ID người dùng đang đăng nhập gán vào
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            professionalActivity.AppUserId = userId;
            professionalActivity.CreatedBy = userId;

            // Bỏ qua validate AppUser (vì ta gán code, không nhập từ form)
            ModelState.Remove("AppUser");
            ModelState.Remove("AppUserId");

            if (ModelState.IsValid)
            {
                _context.Add(professionalActivity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(professionalActivity);
        }

        // --- EDIT ---
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var professionalActivity = await _context.ProfessionalActivities.FindAsync(id);
            if (professionalActivity == null) return NotFound();

            // Kiểm tra quyền (chỉ người tạo mới được sửa)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (professionalActivity.AppUserId != userId) return Forbid();

            return View(professionalActivity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProfessionalActivity professionalActivity)
        {
            if (id != professionalActivity.Id) return NotFound();

            // Giữ nguyên User ID cũ để không bị mất
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            professionalActivity.AppUserId = userId;

            ModelState.Remove("AppUser");
            ModelState.Remove("AppUserId");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(professionalActivity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.ProfessionalActivities.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(professionalActivity);
        }

        // --- DELETE ---
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var professionalActivity = await _context.ProfessionalActivities
                .Include(p => p.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (professionalActivity == null) return NotFound();

            // Kiểm tra quyền
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (professionalActivity.AppUserId != userId) return Forbid();

            return View(professionalActivity);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var professionalActivity = await _context.ProfessionalActivities.FindAsync(id);
            if (professionalActivity != null)
            {
                _context.ProfessionalActivities.Remove(professionalActivity);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}