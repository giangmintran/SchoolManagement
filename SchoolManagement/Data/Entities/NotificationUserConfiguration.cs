using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SchoolManagement.Data.Entities
{
    public class NotificationUserConfiguration : IEntityTypeConfiguration<NotificationUser>
    {
        public void Configure(EntityTypeBuilder<NotificationUser> builder)
        {
            // Khóa chính
            builder.HasKey(e => e.Id);

            // Cấu hình giá trị mặc định
            builder.Property(e => e.IsRead)
                   .HasDefaultValue(false);

            // Cấu hình quan hệ với User
            builder.HasOne(e => e.User)
                   .WithMany() // Điền (u => u.NotificationUsers) nếu ApplicationUser có ICollection<NotificationUser>
                   .HasForeignKey(e => e.UserId)
                   .OnDelete(DeleteBehavior.Cascade); // Nếu User bị xóa, xóa luôn các thông báo của User này
        }
    }
}
