using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagement.Data.Entities
{
    public class LectureCalendar : IAuditable
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        public string TeacherName { get; set; } = null!;

        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Tuần học (1 – 52)
        /// </summary>
        [Required]
        [Range(1, 52)]
        public int Week { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime EndDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public ICollection<LectureCalendarDetail> Details { get; set; } = [];
    }

    public class LectureCalendarDetail
    {
        public int Id { get; set; }
        public int LectureCalendarId { get; set; }
        public DateTime Date { get; set; }
        public LectureCalendar LectureCalendar{ get; set; } = null!;
        /// <summary>
        /// Tiết học
        /// </summary>
        public int Period { get; set; }
        /// <summary>
        /// Lớp
        /// </summary>
        [StringLength(10)]
        [Column(TypeName = "varchar(10)")]
        public string? Class { get; set; }
        /// <summary>
        /// Môn học
        /// </summary>
        [StringLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        public string? Subject { get; set; }
        /// <summary>
        /// Tiết PPCT
        /// </summary>
        
        public int? Lesson { get; set; }
        /// <summary>
        /// Tên bài dạy
        /// </summary>
        [StringLength(1024)]
        [Column(TypeName = "nvarchar(1024)")]
        public string? LessonTitle { get; set; }
        public string? Note { get; set; }
    }
}
