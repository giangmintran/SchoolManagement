using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Data
{
    public abstract class AuditableEntity
    {
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(450)]
        public string? CreatedBy { get; set; } // Lưu UserId hoặc UserName người tạo

        public DateTime? ModifiedDate { get; set; }

        [StringLength(450)]
        public string? ModifiedBy { get; set; }
    }
}
