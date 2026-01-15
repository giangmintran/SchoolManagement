using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Models.ViewModels
{
    public class ProfessionalActivityViewModel
    {
        public int Id { get; set; }

        // 1. Lần thứ
        [Required(ErrorMessage = "Vui lòng nhập lần thứ")]
        [Display(Name = "Lần thứ")]
        public int SequenceNumber { get; set; }

        // 2. Ngày
        [Required(ErrorMessage = "Vui lòng chọn ngày")]
        [Display(Name = "Ngày sinh hoạt")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        // 3. Kiểm diện (Lưu text để linh động, ví dụ: "Đủ", "Vắng: A, B")
        [Display(Name = "Kiểm diện")]
        [StringLength(500)]
        public string? Attendance { get; set; }

        // 4. Thực hiện chương trình
        [Display(Name = "Thực hiện chương trình")]
        public string? Implementation { get; set; }

        // 5. Nội dung
        [Display(Name = "Nội dung chi tiết")]
        public string? Content { get; set; }

        // 6. Đề xuất với tổ trưởng
        [Display(Name = "Đề xuất với tổ trưởng")]
        public string? Proposal { get; set; }

        // 7. Kết quả (Kết luận của buổi sinh hoạt)
        [Display(Name = "Kết quả")]
        public string? Result { get; set; }
    }
}
