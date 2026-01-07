using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data;
using SchoolManagement.Data.Entities;

namespace SchoolManagement.Controllers
{
    // Chỉ Admin mới truy cập được Controller này
    [Authorize(Roles = "Admin")]
    public class AdminLectureCalendarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminLectureCalendarController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AdminLectureCalendar
        public async Task<IActionResult> Index(int? week, string searchTeacher)
        {
            // Admin xem toàn bộ dữ liệu, không lọc theo User ID
            var query = _context.LectureCalendars.AsQueryable();

            // 1. Lọc theo tuần
            if (week.HasValue)
            {
                query = query.Where(x => x.Week == week.Value);
            }

            // 2. Tìm kiếm theo tên giáo viên
            if (!string.IsNullOrEmpty(searchTeacher))
            {
                query = query.Where(x => x.TeacherName.Contains(searchTeacher));
            }

            // 3. Sắp xếp: Ưu tiên tuần mới nhất, sau đó đến tên giáo viên
            query = query.OrderByDescending(x => x.Week).ThenBy(x => x.TeacherName);

            // Chuẩn bị dữ liệu cho Dropdown chọn tuần
            var weeks = await _context.LectureCalendars
                .Select(x => x.Week)
                .Distinct()
                .OrderByDescending(x => x)
                .ToListAsync();

            ViewBag.WeekList = weeks;
            ViewBag.CurrentWeek = week;
            ViewBag.SearchTeacher = searchTeacher;

            return View(await query.ToListAsync());
        }

        // GET: AdminLectureCalendar/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var lectureCalendar = await _context.LectureCalendars
                .Include(l => l.Details) // Eager loading chi tiết
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lectureCalendar == null) return NotFound();

            return View(lectureCalendar);
        }
    }
}