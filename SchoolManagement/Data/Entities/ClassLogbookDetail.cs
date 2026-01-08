using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagement.Data.Entities
{
    public class ClassLogbookDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClassLogbookId { get; set; } // Khóa ngoại tới Header

        // --- Thông tin thời gian ---
        [Required]
        [Display(Name = "Ngày học")]
        public DateTime Date { get; set; } // Ngày cụ thể

        [Required]
        [Display(Name = "Thứ")]
        public int DayOfWeek { get; set; } // 2=Thứ 2, 3=Thứ 3... (Dùng enum hoặc int)

        [Required]
        [Range(1, 5)]
        [Display(Name = "Tiết")]
        public int PeriodIndex { get; set; } // Tiết 1, 2, 3, 4, 5

        // --- Nội dung tiết học ---
        [Display(Name = "Môn học")]
        [Column(TypeName = "nvarchar(256)")]
        public string SubjectName { get; set; } // Hoặc dùng SubjectId nếu có bảng Môn

        [Display(Name = "Tiết CT")]
        [Column(TypeName = "varchar(10)")]
        public string? CurriculumCode { get; set; } // Tiết theo phân phối chương trình (VD: 45)

        [Display(Name = "HS nghỉ tiết")]
        [Column(TypeName = "nvarchar(500)")]
        public string? AbsentStudents { get; set; } // Lưu tên HS nghỉ (VD: "Nam, Hùng (P)")

        [Column(TypeName = "nvarchar(1024)")]
        [Display(Name = "Đầu bài / Nội dung")]
        public string LessonContent { get; set; }

        [Display(Name = "Nhận xét của GV")]
        [Column(TypeName = "nvarchar(1024)")]
        public string? TeacherComment { get; set; }

        // --- Điểm số (Validation Range theo yêu cầu) ---

        [Range(0, 4, ErrorMessage = "Điểm Học tập tối đa 4 điểm")]
        [Display(Name = "Điểm Học tập")]
        public int ScoreLearning { get; set; } // Max 4

        [Range(0, 3, ErrorMessage = "Điểm Kỷ luật tối đa 3 điểm")]
        [Display(Name = "Điểm Kỷ luật")]
        public int ScoreDiscipline { get; set; } // Max 3

        [Range(0, 2, ErrorMessage = "Điểm Vệ sinh tối đa 2 điểm")]
        [Display(Name = "Điểm Vệ sinh")]
        public int ScoreSanitation { get; set; } // Max 2

        [Range(0, 1, ErrorMessage = "Điểm Chuyên cần tối đa 1 điểm")]
        [Display(Name = "Điểm Chuyên cần")]
        public int ScoreDiligent { get; set; } // Max 1 (CC)

        // --- Logic tính toán ---

        // Tổng điểm (Computed property - không nhất thiết phải lưu vào DB nếu không muốn)
        [Display(Name = "Tổng điểm")]
        public int TotalScore
        {
            get
            {
                return ScoreLearning + ScoreDiscipline + ScoreSanitation + ScoreDiligent;
            }
        }

        // Xếp loại (A, B, C, D hoặc Tốt, Khá...)
        [Display(Name = "Xếp loại")]
        public string Classification
        {
            get
            {
                if (TotalScore >= 9) return "Tốt"; // A
                if (TotalScore >= 7) return "Khá"; // B
                if (TotalScore >= 5) return "Trung bình"; // C
                return "Yếu"; // D
            }
        }

        // --- Xác nhận ---
        [Display(Name = "Giáo viên xác nhận")]
        public string ConfirmedBy { get; set; } // Lưu ID hoặc Tên giáo viên ký tên
        public bool IsConfirmed { get; set; } = false;

        // Relationship
        [ForeignKey("ClassLogbookId")]
        public virtual ClassLogbook ClassLogbook { get; set; }
    }
}