using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.VariantTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Common;
using SchoolManagement.Data;
using SchoolManagement.Data.Entities;
using SchoolManagement.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SchoolManagement.Controllers
{
    public class ClassLogbookController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClassLogbookController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? className, int? weekNumber, DateTime? searchDate)
        {
            // Tạo list SelectListItem gồm các lớp 6A..9D
            var listClasses = Enumerable.Range(6, 4) // Khối 6,7,8,9
                .SelectMany(grade =>
                    new[] { "A", "B", "C", "D" } // Các lớp A,B,C,D
                    .Select(section => new SelectListItem
                    {
                        Value = $"{grade}{section}", // Giá trị: "6A"
                        Text = $"{grade}{section}",  // Hiển thị: "6A"
                        Selected = ($"{grade}{section}" == className) // Giữ trạng thái selected nếu đang chọn
                    })
                ).ToList();

            ViewBag.Classes = listClasses;
            // 1. Khởi tạo query
            var query = _context.ClassLogbooks.AsQueryable();
            if (!string.IsNullOrEmpty(className))
            {
                query = query.Where(x => x.Class == className);
            }
            else
            {
                return View(new List<ClassLogbook>());
            }
           
            // 4. Logic lọc theo Ngày (Ưu tiên lọc ngày nếu user nhập cả 2)
            // Yêu cầu: Ngày chọn nằm trong khoảng FromDate và ToDate
            if (searchDate.HasValue)
            {
                query = query.Where(x => x.FromDate.Date <= searchDate.Value.Date
                                      && x.ToDate.Date >= searchDate.Value.Date);

                // Lưu lại giá trị để hiển thị lại trên View
                ViewBag.SelectedDate = searchDate;
            }
            // 5. Logic lọc theo Tuần (Nếu không chọn ngày mới xét đến tuần)
            else if (weekNumber.HasValue)
            {
                query = query.Where(x => x.WeekNumber == weekNumber);
                ViewBag.SelectedWeek = weekNumber;
            }

            // Sắp xếp dữ liệu (Ví dụ: Tuần giảm dần)
            query = query.OrderByDescending(x => x.WeekNumber);

            return View(await query.ToListAsync());
        }

        public static string GetAcademicYear(DateTime? date = null)
        {
            var currentDate = date ?? DateTime.Now;

            int year = currentDate.Year;
            int month = currentDate.Month;

            if (month >= 9) // Từ tháng 9 trở đi là năm học mới
            {
                return $"{year}-{year + 1}";
            }
            else // Trước tháng 9
            {
                return $"{year - 1}-{year}";
            }
        }

        // GET: Tạo sổ đầu bài mới
        [HttpGet]
        public IActionResult Create(string className)
        {
            var now = DateTime.Now;
            var academicYear = GetAcademicYear(now);

            // DayOfWeek: Sunday = 0, Monday = 1, ..., Saturday = 6
            int diffToMonday = now.DayOfWeek == DayOfWeek.Sunday
                ? -6
                : DayOfWeek.Monday - now.DayOfWeek;

            var fromDate = now.AddDays(diffToMonday);
            var toDate = fromDate.AddDays(5); // Thứ 7

            var model = new LogbookUpsertViewModel
            {
                ClassName = className,
                WeekNumber = 1,
                SchoolYear = academicYear,
                FromDate = fromDate,
                ToDate = toDate,
                Details = new List<LogbookDetailViewModel>(),
                AvailableClasses = [.. Enumerable
                    .Range(6, 4) // 6,7,8,9
                    .SelectMany(grade =>
                        new[] { "A", "B", "C", "D" }
                            .Select(section => new SelectListItem
                            {
                                Value = $"{grade}{section}",
                                Text = $"{grade}{section}"
                            })
                    )]
            };

            // LOGIC QUAN TRỌNG: Khởi tạo sẵn khung cho 6 ngày (Thứ 2 -> Thứ 7) x 5 tiết
            for (DateTime date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                for (int period = 1; period <= 5; period++)
                {
                    model.Details.Add(new LogbookDetailViewModel
                    {
                        DayOfWeek = (int) date.DayOfWeek + 1,
                        PeriodIndex = period,
                        Date = date
                        // Mặc định ngày học dựa trên FromDate (bạn có thể tính toán logic ngày ở đây)
                    });
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LogbookUpsertViewModel model)
        {
            var today = DateTime.Today;

            // DayOfWeek: Sunday = 0, Monday = 1, ..., Saturday = 6
            int diffToMonday = today.DayOfWeek == DayOfWeek.Sunday
                ? -6
                : DayOfWeek.Monday - today.DayOfWeek;

            var fromDate = today.AddDays(diffToMonday);
            var toDate = fromDate.AddDays(5); // Thứ 7
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ModelState.IsValid)
            {
                // BƯỚC 1: Map từ ViewModel Header sang Entity Header
                var logbookEntity = new ClassLogbook
                {
                    // Lưu ý: Nếu Class là string tên lớp, bạn cần query lấy ID hoặc lưu string tùy thiết kế DB
                    // Giả sử DB của bạn lưu ClassId, bạn cần xử lý ở đây. 
                    // Ví dụ tạm thời: int.Parse(model.Class) nếu string đó chứa ID.
                    Class = model.ClassName,
                    WeekNumber = model.WeekNumber ?? 0,
                    SchoolYear = model.SchoolYear,
                    FromDate = fromDate,
                    ToDate = toDate,
                    HomeroomTeacherComment = model.HomeroomTeacherComment,
                    LogbookDetails = new List<ClassLogbookDetail>()
                };

                // BƯỚC 2: Map danh sách Details từ ViewModel sang Entity
                foreach (var itemVM in model.Details)
                {
                    // Chỉ lưu những tiết có nhập tên môn học hoặc nội dung (tránh lưu rác)
                    if (!string.IsNullOrEmpty(itemVM.SubjectName) || !string.IsNullOrEmpty(itemVM.LessonContent))
                    {
                        var detailEntity = new ClassLogbookDetail
                        {
                            DayOfWeek = itemVM.DayOfWeek,
                            PeriodIndex = itemVM.PeriodIndex,
                            Date = itemVM.Date,
                            SubjectName = itemVM.SubjectName,
                            CurriculumCode = itemVM.CurriculumCode,
                            LessonContent = itemVM.LessonContent,
                            AbsentStudents = itemVM.AbsentStudents,
                            TeacherComment = itemVM.TeacherComment,

                            // Map điểm số
                            ScoreLearning = itemVM.ScoreLearning,
                            ScoreDiscipline = itemVM.ScoreDiscipline,
                            ScoreSanitation = itemVM.ScoreSanitation,
                            ScoreDiligent = itemVM.ScoreDiligent,
                            ConfirmedBy = userId ?? string.Empty,
                            IsConfirmed = true
                        };

                        logbookEntity.LogbookDetails.Add(detailEntity);
                    }
                }

                _context.Add(logbookEntity);
                await _context.SaveChangesAsync();
                return RedirectToAction("Edit", new { id = logbookEntity.Id });
            }

            // Nếu lỗi, trả về view cũ
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // 1. Lấy dữ liệu Header và Details từ DB
            var logbook = await _context.ClassLogbooks
                .Include(l => l.LogbookDetails)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (logbook == null) return NotFound();

            // 2. Khởi tạo ViewModel
            var viewModel = new LogbookUpsertViewModel
            {
                Id = logbook.Id,
                ClassName = logbook.Class,
                WeekNumber = logbook.WeekNumber,
                SchoolYear = logbook.SchoolYear,
                FromDate = logbook.FromDate,
                ToDate = logbook.ToDate,
                HomeroomTeacherComment = logbook.HomeroomTeacherComment,
                AvailableClasses = await GetClassSelectList(),
                Details = new List<LogbookDetailViewModel>() // Chuẩn bị list rỗng
            };

            // 3. LOGIC LẤY FULL NGÀY (Thứ 2 -> Thứ 7, Tiết 1 -> 5)
            // Giả sử học từ Thứ 2 (2) đến Thứ 7 (7)
            for (int day = 2; day <= 7; day++)
            {
                // Giả sử mỗi buổi có 5 tiết (Sửa thành 9 hoặc 10 nếu học cả ngày)
                for (int period = 1; period <= 5; period++)
                {
                    // Tìm xem trong DB đã có tiết này chưa
                    var existingDetail = logbook.LogbookDetails
                        .FirstOrDefault(d => d.DayOfWeek == day && d.PeriodIndex == period);

                    if (existingDetail != null)
                    {
                        // A. Nếu CÓ: Map dữ liệu cũ vào
                        viewModel.Details.Add(new LogbookDetailViewModel
                        {
                            Id = existingDetail.Id, // Có ID -> Update
                            DayOfWeek = existingDetail.DayOfWeek,
                            PeriodIndex = existingDetail.PeriodIndex,
                            SubjectName = existingDetail.SubjectName,
                            CurriculumCode = existingDetail.CurriculumCode,
                            LessonContent = existingDetail.LessonContent,
                            AbsentStudents = existingDetail.AbsentStudents,
                            ScoreLearning = existingDetail.ScoreLearning,
                            ScoreDiscipline = existingDetail.ScoreDiscipline,
                            ScoreSanitation = existingDetail.ScoreSanitation,
                            ScoreDiligent = existingDetail.ScoreDiligent,
                            TeacherComment = existingDetail.TeacherComment
                        });
                    }
                    else
                    {
                        // B. Nếu KHÔNG (chưa nhập): Tạo dòng trống
                        viewModel.Details.Add(new LogbookDetailViewModel
                        {
                            DayOfWeek = day,
                            PeriodIndex = period,
                            SubjectName = "", // Để trống để hiển thị ô nhập liệu
                                              // Các trường khác null/default
                        });
                    }
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LogbookUpsertViewModel model)
        {
            var logbookDb = await _context.ClassLogbooks
                   .Include(l => l.LogbookDetails)
                   .FirstOrDefaultAsync(l => l.Id == model.Id);
            if (!ModelState.IsValid)
            {
                model.AvailableClasses = await GetClassSelectList();
                return View(model);
            }
            try
            {
               

                if (logbookDb != null)
                {
                    // Update Header
                    logbookDb.HomeroomTeacherComment = model.HomeroomTeacherComment;

                    // Xử lý Details
                    // Danh sách model.Details bây giờ chỉ chứa:
                    // 1. Các dòng mới CÓ dữ liệu (Id=0)
                    // 2. Các dòng cũ (Id>0) - bao gồm cả dòng có dữ liệu và dòng bị user xóa trắng
                    foreach (var itemModel in model.Details)
                    {
                        // Kiểm tra xem dòng này có dữ liệu thực tế không
                        bool hasContent = !string.IsNullOrEmpty(itemModel.SubjectName);

                        if (itemModel.Id > 0)
                        {
                            // --- TRƯỜNG HỢP 1: UPDATE hoặc DELETE dòng cũ ---
                            var itemDb = logbookDb.LogbookDetails.FirstOrDefault(d => d.Id == itemModel.Id);
                            if (itemDb != null)
                            {
                                if (hasContent)
                                {
                                    // A. Nếu còn dữ liệu -> Update
                                    itemDb.SubjectName = itemModel.SubjectName;
                                    itemDb.CurriculumCode = itemModel.CurriculumCode;
                                    itemDb.LessonContent = itemModel.LessonContent;
                                    itemDb.AbsentStudents = itemModel.AbsentStudents;
                                    itemDb.ScoreLearning = itemModel.ScoreLearning;
                                    itemDb.ScoreDiscipline = itemModel.ScoreDiscipline;
                                    itemDb.ScoreSanitation = itemModel.ScoreSanitation;
                                    itemDb.ScoreDiligent = itemModel.ScoreDiligent;
                                    itemDb.TeacherComment = itemModel.TeacherComment;
                                }
                                else
                                {
                                    // B. Nếu dữ liệu rỗng (User xóa text trên view) -> Delete khỏi DB
                                    _context.ClassLogbookDetails.Remove(itemDb);
                                }
                            }
                        }
                        else
                        {
                            // --- TRƯỜNG HỢP 2: INSERT dòng mới ---
                            // (JS đã chặn các dòng Id=0 rỗng, nên vào đây chắc chắn là có dữ liệu)
                            if (hasContent)
                            {
                                var newDetail = new ClassLogbookDetail
                                {
                                    ClassLogbookId = logbookDb.Id,
                                    DayOfWeek = itemModel.DayOfWeek,
                                    PeriodIndex = itemModel.PeriodIndex,
                                    SubjectName = itemModel.SubjectName,
                                    CurriculumCode = itemModel.CurriculumCode,
                                    LessonContent = itemModel.LessonContent,
                                    AbsentStudents = itemModel.AbsentStudents,
                                    ScoreLearning = itemModel.ScoreLearning,
                                    ScoreDiscipline = itemModel.ScoreDiscipline,
                                    ScoreSanitation = itemModel.ScoreSanitation,
                                    ScoreDiligent = itemModel.ScoreDiligent,
                                    TeacherComment = itemModel.TeacherComment,
                                    IsConfirmed = true,
                                    ConfirmedBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? ""
                                };
                                _context.ClassLogbookDetails.Add(newDetail);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData.ToastSuccess("Cập nhật thành công.");
                    return RedirectToAction("Edit", new { id = logbookDb.Id });
                }
                return View(logbookDb);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi: " + ex.Message);
                model.AvailableClasses = await GetClassSelectList();
                return View(model);
            }
        }
        // --- Helper Methods ---

        // Hàm giả lập lấy danh sách lớp
        private async Task<List<SelectListItem>> GetClassSelectList()
        {
            return [.. Enumerable
                    .Range(6, 4) // 6,7,8,9
                    .SelectMany(grade =>
                        new[] { "A", "B", "C", "D" }
                            .Select(section => new SelectListItem
                            {
                                Value = $"{grade}{section}",
                                Text = $"{grade}{section}"
                            })
                    )];
        }

        private int GetCurrentWeek()
        {
            // Logic tính tuần hiện tại của bạn
            return 1;
        }
    }
}