using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagement.Data.Entities; // Namespace chứa entity của bạn
using System;
using System.Collections.Generic;

namespace SchoolManagement.Models
{
    public class LogbookUpsertViewModel
    {
        public int? Id { get; set; } // Null nếu là tạo mới
        public string? ClassName { get; set; }
        public int? WeekNumber { get; set; }
        public string? SchoolYear { get; set; }
        public DateTime FromDate { get; set; } = DateTime.Now;
        public DateTime ToDate { get; set; } = DateTime.Now.AddDays(6);
        public string? HomeroomTeacherComment { get; set; }
        public IEnumerable<SelectListItem>? AvailableClasses { get; set; }
        // Danh sách chi tiết tiết học (để binding ra view)
        public List<LogbookDetailViewModel> Details { get; set; } = new List<LogbookDetailViewModel>();
    }
    public class LogbookDetailViewModel
    {
        public int? Id { get; set; }
        public int DayOfWeek { get; set; } // 2, 3, 4, 5, 6, 7
        public int PeriodIndex { get; set; } // 1 -> 5
        public DateTime Date { get; set; }

        public string? SubjectName { get; set; }
        public string? CurriculumCode { get; set; }
        public string? LessonContent { get; set; }
        public string? AbsentStudents { get; set; }
        public string? TeacherComment { get; set; }

        // Điểm số
        public int ScoreLearning { get; set; }
        public int ScoreDiscipline { get; set; }
        public int ScoreSanitation { get; set; }
        public int ScoreDiligent { get; set; }
    }
}