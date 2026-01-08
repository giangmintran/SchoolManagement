using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagement.Data.Entities
{
    public class ClassLogbook
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Lớp
        /// </summary>
        [StringLength(10)]
        [Required]
        [Column(TypeName = "varchar(10)")]
        public string Class { get; set; } = null!;

        [Required]
        [Display(Name = "Tuần thứ")]
        public int WeekNumber { get; set; } // Ví dụ: Tuần 1, Tuần 2

        [Required]
        [Display(Name = "Năm học")]
        [Column(TypeName = "varchar(10)")]
        public string SchoolYear { get; set; } // Ví dụ: "2025-2026"

        [Display(Name = "Từ ngày")]
        public DateTime FromDate { get; set; }

        [Display(Name = "Đến ngày")]
        public DateTime ToDate { get; set; }

        [Display(Name = "Nhận xét chung của GVCN")]
        [Column(TypeName = "nvarchar(500)")]
        public string? HomeroomTeacherComment { get; set; }

        // Relationship: Một tuần có nhiều tiết học chi tiết
        public virtual ICollection<ClassLogbookDetail> LogbookDetails { get; set; }
    }
}