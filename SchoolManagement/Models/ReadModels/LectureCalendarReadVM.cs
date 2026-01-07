using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Models.ReadModels
{
    public class LectureCalendarReadVM
    {
        public int Id { get; set; }

        [Display(Name = "Giáo viên")]
        public string TeacherName { get; set; } = null!;

        [Display(Name = "Tuần")]
        public int Week { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedDate { get; set; }

        // Danh sách chi tiết
        public List<LectureCalendarDetailReadVM> Details { get; set; } = new List<LectureCalendarDetailReadVM>();
    }

    // ViewModel cho từng dòng chi tiết
    public class LectureCalendarDetailReadVM
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Period { get; set; }
        public string? Class { get; set; }
        public string? Subject { get; set; }
        public int? Lesson { get; set; }
        public string? LessonTitle { get; set; }
        public string? Note { get; set; }
    }
}
