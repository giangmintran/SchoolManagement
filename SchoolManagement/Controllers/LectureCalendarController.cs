using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data;
using SchoolManagement.Data.Entities;
using SchoolManagement.Models;
using SchoolManagement.Models.ReadModels;
using System.Security.Claims;

namespace SchoolManagement.Controllers
{
    [Authorize(Roles = "User")]
    public class LectureCalendarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LectureCalendarController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: LectureCalendars
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Index(int? week, string searchTeacher)
        {
            // Lấy UserId (NameIdentifier) của user đang đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Lọc lịch giảng của user hiện tại
            var mySchedules = await _context.LectureCalendars
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Week)
                .ToListAsync();

            return View(mySchedules);
        }

        // GET: LectureCalendars/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // 1. Lấy dữ liệu từ Database (Entity)
            var lectureCalendar = await _context.LectureCalendars
                .Include(l => l.Details)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lectureCalendar == null)
            {
                return NotFound();
            }

            // 2. Mapping Entity -> ViewModel
            var viewModel = new LectureCalendarReadVM
            {
                Id = lectureCalendar.Id,
                TeacherName = lectureCalendar.TeacherName,
                Week = lectureCalendar.Week,
                StartDate = lectureCalendar.StartDate,
                EndDate = lectureCalendar.EndDate,
                CreatedDate = lectureCalendar.CreatedDate,
                // Map danh sách chi tiết và sắp xếp luôn ở đây cho gọn
                Details = lectureCalendar.Details.Select(d => new LectureCalendarDetailReadVM
                {
                    Id = d.Id,
                    Date = d.Date,
                    Period = d.Period,
                    Class = d.Class,
                    Subject = d.Subject,
                    Lesson = d.Lesson,
                    LessonTitle = d.LessonTitle,
                    Note = d.Note
                })
                .OrderBy(d => d.Date)
                .ThenBy(d => d.Period)
                .ToList()
            };

            // 3. Trả về ViewModel cho View
            return View(viewModel);
        }

        // GET: LectureCalendars/Create
        public IActionResult Create()
        {
            var model = new LectureCalendarCreateViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(6), // Mặc định 1 tuần
            };
            return View(model);
        }

        // POST: LectureCalendars/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LectureCalendarCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Mapping ViewModel sang Entity Parent
                var lectureCalendar = new LectureCalendar
                {
                    TeacherName = model.TeacherName,
                    Week = model.Week,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    CreatedDate = DateTime.Now,
                    // Lấy UserId từ User đang đăng nhập
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                };

                // 2. Mapping List Details
                if (model.Details != null && model.Details.Count != 0)
                {
                    foreach (var item in model.Details)
                    {
                        // Chỉ thêm những dòng có dữ liệu quan trọng (ví dụ: có nhập Lớp)
                        lectureCalendar.Details.Add(new LectureCalendarDetail
                        {
                            Date = item.Date,
                            Period = item.Period,
                            Class = item.Class,
                            Subject = item.Subject,
                            Lesson = item.Lesson,
                            LessonTitle = item.LessonTitle,
                            Note = item.Note
                        });
                    }
                }

                _context.Add(lectureCalendar);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Chuyển hướng sau khi lưu xong
            }
            return View(model);
        }

        // GET: LectureCalendars/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Load kèm Details nếu bạn muốn cho phép sửa luôn danh sách chi tiết ở đây
            var lectureCalendar = await _context.LectureCalendars
                                                .Include(x => x.Details)
                                                .FirstOrDefaultAsync(x => x.Id == id);

            if (lectureCalendar == null)
            {
                return NotFound();
            }
            return View(lectureCalendar);
        }

        // POST: LectureCalendars/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TeacherName,Week,StartDate,EndDate")] LectureCalendar lectureCalendar)
        {
            if (id != lectureCalendar.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Attach để EF biết đây là update
                    _context.Update(lectureCalendar);

                    // SaveChanges sẽ kích hoạt logic update ModifiedDate trong DbContext
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LectureCalendarExists(lectureCalendar.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(lectureCalendar);
        }

        // GET: LectureCalendars/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lectureCalendar = await _context.LectureCalendars
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lectureCalendar == null)
            {
                return NotFound();
            }

            return View(lectureCalendar);
        }

        // POST: LectureCalendars/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var lectureCalendar = await _context.LectureCalendars.FindAsync(id);
            if (lectureCalendar != null)
            {
                _context.LectureCalendars.Remove(lectureCalendar);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LectureCalendarExists(int id)
        {
            return _context.LectureCalendars.Any(e => e.Id == id);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuick(LectureCalendarReadVM model)
        {
            if (model.Details == null || !model.Details.Any())
            {
                return RedirectToAction("Details", new { id = model.Id });
            }

            // 1. Lấy dữ liệu gốc từ DB
            var calendar = await _context.LectureCalendars
                .Include(c => c.Details)
                .FirstOrDefaultAsync(c => c.Id == model.Id);

            if (calendar == null) return NotFound();

            // 2. Cập nhật dữ liệu
            foreach (var item in model.Details)
            {
                // Tìm chi tiết tương ứng trong DB (dựa vào Id đã hidden ở View)
                var detailDb = calendar.Details.FirstOrDefault(d => d.Id == item.Id);
                if (detailDb != null)
                {
                    detailDb.Lesson = item.Lesson;
                    detailDb.LessonTitle = item.LessonTitle;
                    detailDb.Note = item.Note;
                    // Cập nhật các trường khác nếu có
                }
            }

            // 3. Lưu và redirect
            calendar.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            // Thông báo thành công
            TempData["Success"] = "Đã cập nhật lịch báo giảng thành công!";
            return RedirectToAction("Details", new { id = model.Id });
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(int id)
        {
            // 1. Lấy dữ liệu
            var calendar = await _context.LectureCalendars
                .Include(c => c.Details)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (calendar == null)
            {
                return NotFound("Không tìm thấy lịch báo giảng.");
            }

            // 2. Khởi tạo Workbook ClosedXML
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("LichBaoGiang");

                // --- ĐỊNH DẠNG CỘT ---
                // ClosedXML dùng Width đơn vị hơi khác Excel chút, nhưng tương đối
                worksheet.Column(1).Width = 15; // Thứ/Ngày
                worksheet.Column(2).Width = 8;  // Tiết
                worksheet.Column(3).Width = 10; // Lớp
                worksheet.Column(4).Width = 20; // Môn
                worksheet.Column(5).Width = 10; // PPCT
                worksheet.Column(6).Width = 45; // Tên bài dạy
                worksheet.Column(7).Width = 20; // Ghi chú

                // --- PHẦN HEADER ---

                // Tiêu đề chính
                var titleRange = worksheet.Range("A1:G1");
                titleRange.Merge();
                titleRange.Value = "LỊCH BÁO GIẢNG";
                titleRange.Style.Font.FontSize = 16;
                titleRange.Style.Font.Bold = true;
                titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Thông tin giáo viên
                worksheet.Range("A2:C2").Merge().Value = $"Họ tên: {calendar.TeacherName}";
                worksheet.Cell("A2").Style.Font.Bold = true;

                // Thông tin Tuần
                var weekRange = worksheet.Range("D2:E2");
                weekRange.Merge();
                weekRange.Value = $"Tuần: {calendar.Week}";
                weekRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                weekRange.Style.Font.Bold = true;

                // Thông tin Ngày
                var dateRange = worksheet.Range("F2:G2");
                dateRange.Merge();
                dateRange.Value = $"Từ: {calendar.StartDate:dd/MM/yyyy} đến {calendar.EndDate:dd/MM/yyyy}";
                dateRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                dateRange.Style.Font.Italic = true;

                // --- BẢNG DỮ LIỆU ---
                int headerRow = 4;
                string[] headers = { "THỨ NGÀY", "TIẾT", "LỚP", "MÔN", "TIẾT PPCT", "TÊN BÀI DẠY", "GHI CHÚ" };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(headerRow, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                int currentRow = headerRow + 1;

                // Loop qua từng ngày
                for (var date = calendar.StartDate; date <= calendar.EndDate; date = date.AddDays(1))
                {
                    if (date.DayOfWeek == DayOfWeek.Sunday) continue; // Bỏ qua chủ nhật nếu muốn

                    // Lấy chi tiết tiết dạy trong ngày
                    var dailyDetails = calendar.Details
                        .Where(d => d.Date.Date == date.Date)
                        .OrderBy(d => d.Period)
                        .ToList();

                    string dayName = GetVietnameseDayName(date);
                    string dateString = date.ToString("dd/MM");
                    int startRowOfDay = currentRow;

                    // Form cứng: 5 tiết sáng (hoặc chiều)
                    int maxPeriod = 5;

                    for (int period = 1; period <= maxPeriod; period++)
                    {
                        var detail = dailyDetails.FirstOrDefault(d => d.Period == period);

                        worksheet.Cell(currentRow, 2).Value = period; // Cột Tiết

                        if (detail != null)
                        {
                            worksheet.Cell(currentRow, 3).Value = detail.Class;
                            worksheet.Cell(currentRow, 4).Value = detail.Subject;
                            // Ép kiểu về string hoặc số tùy nhu cầu, ở đây ClosedXML tự hiểu
                            if (detail.Lesson.HasValue)
                                worksheet.Cell(currentRow, 5).Value = detail.Lesson.Value;

                            worksheet.Cell(currentRow, 6).Value = detail.LessonTitle;
                            worksheet.Cell(currentRow, 7).Value = detail.Note;

                            // Wrap text
                            worksheet.Cell(currentRow, 6).Style.Alignment.WrapText = true;
                        }

                        // Kẻ khung
                        for (int col = 1; col <= 7; col++)
                        {
                            worksheet.Cell(currentRow, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }

                        currentRow++;
                    }

                    // Merge cột Thứ/Ngày
                    var rangeDay = worksheet.Range(startRowOfDay, 1, currentRow - 1, 1);
                    rangeDay.Merge();
                    rangeDay.Value = $"{dayName}{Environment.NewLine}{dateString}";
                    rangeDay.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    rangeDay.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    rangeDay.Style.Alignment.WrapText = true;
                }

                // --- FOOTER / CHỮ KÝ ---
                currentRow += 1;
                var signDateRange = worksheet.Range(currentRow, 5, currentRow, 7);
                signDateRange.Merge();
                signDateRange.Value = $"Ngày {DateTime.Now.Day} tháng {DateTime.Now.Month} năm {DateTime.Now.Year}";
                signDateRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                signDateRange.Style.Font.Italic = true;

                currentRow += 1;

                // Tổ trưởng
                var ttRange = worksheet.Range(currentRow, 1, currentRow, 2);
                ttRange.Merge();
                ttRange.Value = "NHẬN XÉT   ";
                ttRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ttRange.Style.Font.Bold = true;

                // Người báo giảng
                var gvRange = worksheet.Range(currentRow, 5, currentRow, 7);
                gvRange.Merge();
                gvRange.Value = "TM-BGH";
                gvRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                gvRange.Style.Font.Bold = true;

                currentRow += 4; // Khoảng trống ký tên

                // 3. Xuất file ra Stream
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0; // Reset con trỏ stream về đầu

                    string fileName = $"LichBaoGiang_Tuan{calendar.Week}_{calendar.TeacherName}.xlsx";

                    // Cần copy sang một MemoryStream mới để trả về File result (do stream của using workbook sẽ bị đóng)
                    var outputStream = new MemoryStream(stream.ToArray());

                    return File(outputStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        private string GetVietnameseDayName(DateTime date)
        {
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Monday: return "Hai";
                case DayOfWeek.Tuesday: return "Ba";
                case DayOfWeek.Wednesday: return "Tư";
                case DayOfWeek.Thursday: return "Năm";
                case DayOfWeek.Friday: return "Sáu";
                case DayOfWeek.Saturday: return "Bảy";
                case DayOfWeek.Sunday: return "CN";
                default: return "";
            }
        }
    }
}