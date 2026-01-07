using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Models
{
    public class LectureCalendarCreateViewModel
    {
        [Display(Name = "Tên giáo viên")]
        [Required(ErrorMessage = "Vui lòng nhập tên giáo viên")]
        public string TeacherName { get; set; } = null!;

        [Display(Name = "Tuần thứ")]
        [Required]
        [Range(1, 52)]
        public int Week { get; set; }

        [Display(Name = "Từ ngày")]
        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Display(Name = "Đến ngày")]
        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        // Danh sách chi tiết sẽ được hứng từ form
        public List<LectureCalendarDetailViewModel> Details { get; set; } = new List<LectureCalendarDetailViewModel>();
    }

    public class LectureCalendarDetailViewModel
    {
        // Ngày dạy cụ thể
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Display(Name = "Tiết")]
        [Required]
        public int Period { get; set; }

        [Display(Name = "Lớp")]
        public string? Class { get; set; }

        [Display(Name = "Môn học")]
        public string? Subject { get; set; }

        [Display(Name = "Tiết PPCT")]
        public int? Lesson { get; set; }

        [Display(Name = "Tên bài dạy")]
        public string? LessonTitle { get; set; }

        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }
    }
}
