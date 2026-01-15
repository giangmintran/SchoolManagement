using DocumentFormat.OpenXml.Office2010.Excel;
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
        public async Task<IActionResult> Index(string? className, int? weekNumber, DateTime? searchDate, int pageIndex = 1)
        {
            int pageSize = 10; // Số bản ghi trên 1 trang

            // --- Logic khởi tạo Dropdown Lớp (Giữ nguyên) ---
            var listClasses = Enumerable.Range(6, 4)
                .SelectMany(grade =>
                    new[] { "A", "B", "C", "D" }
                    .Select(section => new SelectListItem
                    {
                        Value = $"{grade}{section}",
                        Text = $"{grade}{section}",
                        Selected = ($"{grade}{section}" == className)
                    })
                ).ToList();
            ViewBag.Classes = listClasses;

            // --- Logic Query và Filter (Giữ nguyên) ---
            var query = _context.ClassLogbooks.AsQueryable();

            if (!string.IsNullOrEmpty(className))
            {
                query = query.Where(x => x.Class == className);
            }

            if (searchDate.HasValue)
            {
                query = query.Where(x => x.FromDate.Date <= searchDate.Value.Date
                                      && x.ToDate.Date >= searchDate.Value.Date);
                ViewBag.SelectedDate = searchDate;
            }
            else if (weekNumber.HasValue)
            {
                query = query.Where(x => x.WeekNumber == weekNumber);
                ViewBag.SelectedWeek = weekNumber;
            }

            // Sắp xếp: Tuần giảm dần, sau đó đến Lớp
            query = query.OrderByDescending(x => x.WeekNumber).ThenBy(x => x.Class);

            // --- THAY ĐỔI QUAN TRỌNG: Phân trang ---
            var pagedData = await query.ToPagedResultAsync(pageIndex, pageSize);

            return View(pagedData);
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
        public IActionResult Create(string className, int? weekNumber)
        {
            var classNameQuery = Request.Query["ClassName"].ToString();
            if (Request.Query.ContainsKey("ClassName") && string.IsNullOrWhiteSpace(classNameQuery) && string.IsNullOrWhiteSpace(className))
            {
                TempData.ToastWarning("Vui lòng chọn lớp");
            }

            var weekQuery = Request.Query["WeekNumber"].ToString();
            if (Request.Query.ContainsKey("WeekNumber") && string.IsNullOrWhiteSpace(weekQuery) && weekNumber is null)
            {
                TempData.ToastWarning("Vui lòng nhập số tuần");
            }

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
                WeekNumber = weekNumber,
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

        private async Task<LogbookUpsertViewModel> GetEditViewModel(int id, ClassLogbook logbook)
        {
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
            return viewModel;
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
            return View(await GetEditViewModel(id, logbook));
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
                                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                                if (hasContent)
                                {
                                    bool hasChanges =
                                        itemDb.SubjectName != itemModel.SubjectName ||
                                        itemDb.CurriculumCode != itemModel.CurriculumCode ||
                                        itemDb.LessonContent != itemModel.LessonContent ||
                                        itemDb.AbsentStudents != itemModel.AbsentStudents ||
                                        itemDb.ScoreLearning != itemModel.ScoreLearning ||
                                        itemDb.ScoreDiscipline != itemModel.ScoreDiscipline ||
                                        itemDb.ScoreSanitation != itemModel.ScoreSanitation ||
                                        itemDb.ScoreDiligent != itemModel.ScoreDiligent ||
                                        itemDb.TeacherComment != itemModel.TeacherComment;

                                    // 2. Nếu CÓ thay đổi thì mới check quyền
                                    if (hasChanges)
                                    {
                                        // Logic: Đã được xác nhận (ConfirmedBy có data) VÀ người xác nhận KHÁC userId hiện tại
                                        if (!string.IsNullOrEmpty(itemDb.ConfirmedBy) && itemDb.ConfirmedBy != userId)
                                        {
                                            TempData.ToastWarning($"Bạn không có quyền chỉnh sửa nội dung môn {itemDb.SubjectName}. (Thứ {itemDb.DayOfWeek}, tiết {itemDb.PeriodIndex})");
                                            ModelState.Clear();
                                            return View(await GetEditViewModel(logbookDb.Id, logbookDb));
                                        }
                                    }
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

        [HttpGet]
        public async Task<IActionResult> GetCurrentPeriodModal(int logbookId)
        {
            // 1. Xác định thời gian (Giữ nguyên logic cũ của bạn)
            var now = DateTime.Now;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Map Thứ: C# Sunday=0 -> App T2=2, CN=8 (Tuỳ logic DB của bạn)
            // Lưu ý: Logic dưới đây giả định T2=2, T3=3... T7=7, CN=1 hoặc 8 tuỳ bạn quy định
            int currentDay = (int)now.DayOfWeek + 1;
            if (now.DayOfWeek == DayOfWeek.Sunday) currentDay = 8; // Ví dụ CN là 8

            // Map Tiết học
            int currentHour = now.Hour;
            int currentPeriod = 0;

            // Logic khung giờ (Ví dụ)
            if (currentHour >= 7 && currentHour < 8) currentPeriod = 1;
            else if (currentHour >= 8 && currentHour < 9) currentPeriod = 2;
            else if (currentHour >= 9 && currentHour < 10) currentPeriod = 3;
            else if (currentHour >= 10 && currentHour < 11) currentPeriod = 4;
            else if (currentHour >= 11 && currentHour < 12) currentPeriod = 5;
            // ... (Thêm logic chiều nếu cần)

            // Nếu không trong giờ học -> Trả về lỗi 204 (No Content) hoặc 400 để JS xử lý thông báo
            if (currentPeriod == 0)
            {
                return BadRequest($"Bây giờ là {now:HH:mm}, không nằm trong khung giờ học chính khóa.");
            }

            // 2. Tìm dữ liệu trong DB
            var logbook = await _context.ClassLogbooks
                .Include(l => l.LogbookDetails)
                .FirstOrDefaultAsync(l => l.Id == logbookId);

            if (logbook == null) return NotFound("Không tìm thấy sổ đầu bài.");

            // 1. Tìm bản ghi chi tiết trong LogbookDetails
            var detailEntity = logbook.LogbookDetails
                .FirstOrDefault(d => d.DayOfWeek == currentDay && d.PeriodIndex == currentPeriod);

            // 2. KIỂM TRA QUYỀN (Logic mới thêm)
            // Chỉ kiểm tra khi tiết học ĐÃ CÓ dữ liệu (detailEntity != null).
            // Nếu confirmBy (người đã lưu/ký) khác với userId (người đang sửa) thì chặn lại.
            if (detailEntity != null && detailEntity.ConfirmedBy != userId)
            {
                return BadRequest("Bạn không phải giáo viên dạy tiết này (hoặc tiết này đã được giáo viên khác nhập).");
            }

            // 3. Map sang ViewModel
            // Logic này xử lý được cả 2 trường hợp:
            // - Nếu detailEntity == null: Id = 0, các trường khác null -> Form thêm mới.
            // - Nếu detailEntity != null: Load dữ liệu cũ lên -> Form chỉnh sửa.
            var model = new LogbookDetailViewModel
            {
                DayOfWeek = currentDay,
                PeriodIndex = currentPeriod,

                // Nếu detailEntity null thì Id = 0 (để action POST biết là thêm mới)
                Id = detailEntity?.Id ?? 0,

                SubjectName = detailEntity?.SubjectName,
                CurriculumCode = detailEntity?.CurriculumCode,
                LessonContent = detailEntity?.LessonContent,
                AbsentStudents = detailEntity?.AbsentStudents,

                ScoreLearning = detailEntity?.ScoreLearning ?? 0,
                ScoreDiscipline = detailEntity?.ScoreDiscipline ?? 0,
                ScoreSanitation = detailEntity?.ScoreSanitation ?? 0,
                ScoreDiligent = detailEntity?.ScoreDiligent ?? 0,

                TeacherComment = detailEntity?.TeacherComment
            };

            // Truyền ID sổ cái để dùng khi lưu
            ViewBag.ClassLogbookId = logbookId;
            return PartialView("_CurrentPeriodModal", model);
        }

        // --- THÊM HÀM NÀY: Để lưu dữ liệu từ Modal ---
        [HttpPost]
        public async Task<IActionResult> UpdatePeriodDetail(LogbookDetailViewModel model, int ClassLogbookId)
        {
            if (ClassLogbookId == 0) return BadRequest("Thiếu ID sổ đầu bài.");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                // 1. Lấy sổ cái từ DB
                var logbook = await _context.ClassLogbooks
                    .Include(l => l.LogbookDetails)
                    .FirstOrDefaultAsync(l => l.Id == ClassLogbookId);

                if (logbook == null) return NotFound("Sổ không tồn tại.");

                // 2. Kiểm tra xem tiết này đã có record chưa
                var existingDetail = logbook.LogbookDetails
                    .FirstOrDefault(d => d.DayOfWeek == model.DayOfWeek && d.PeriodIndex == model.PeriodIndex && (d.ConfirmedBy == null || d.ConfirmedBy == userId));

                if (existingDetail != null)
                {
                    // UPDATE
                    existingDetail.SubjectName = model.SubjectName;
                    existingDetail.CurriculumCode = model.CurriculumCode;
                    existingDetail.LessonContent = model.LessonContent;
                    existingDetail.AbsentStudents = model.AbsentStudents;
                    existingDetail.ScoreLearning = model.ScoreLearning;
                    existingDetail.ScoreDiscipline = model.ScoreDiscipline;
                    existingDetail.ScoreSanitation = model.ScoreSanitation;
                    existingDetail.ScoreDiligent = model.ScoreDiligent;
                    existingDetail.TeacherComment = model.TeacherComment;
                    existingDetail.ConfirmedBy = userId;
                    // Cập nhật người sửa cuối nếu cần
                }
                else
                {
                    // INSERT MỚI
                    var newDetail = new ClassLogbookDetail
                    {
                        ClassLogbookId = ClassLogbookId,
                        DayOfWeek = model.DayOfWeek,
                        PeriodIndex = model.PeriodIndex,
                        SubjectName = model.SubjectName,
                        CurriculumCode = model.CurriculumCode,
                        LessonContent = model.LessonContent,
                        AbsentStudents = model.AbsentStudents,
                        ScoreLearning = model.ScoreLearning,
                        ScoreDiscipline = model.ScoreDiscipline,
                        ScoreSanitation = model.ScoreSanitation,
                        ScoreDiligent = model.ScoreDiligent,
                        TeacherComment = model.TeacherComment,
                        IsConfirmed = true,
                        ConfirmedBy = userId
                    };
                    _context.ClassLogbookDetails.Add(newDetail);
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi khi lưu: " + ex.Message);
            }
        }
    }
}