using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Data.Entities;

namespace SchoolManagement.Data.EntityConfiguration
{
    public class NotificationTypeConfiguration : IEntityTypeConfiguration<NotificationType>
    {
        public void Configure(EntityTypeBuilder<NotificationType> builder)
        {
            // Khóa chính
            builder.HasKey(e => e.Id);

            // Cấu hình các thuộc tính (Ghi đè hoặc củng cố Data Annotations)
            builder.Property(e => e.Title)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(e => e.Content)
                   .IsRequired();

            builder.Property(e => e.Status)
                  .HasDefaultValue(NotificationStatus.NotSend)
                  .IsRequired();

            // Cấu hình giá trị mặc định cho CreatedAt. 
            // Vì trong class bạn dùng DateTime.UtcNow, ở SQL nên dùng GETUTCDATE() thay vì GETDATE()
            builder.Property(e => e.CreatedAt)
                   .HasDefaultValueSql("GETDATE()");

            // Cấu hình quan hệ 1 - N (1 NotificationType có nhiều NotificationUser)
            builder.HasMany(e => e.UserNotifications)
                   .WithOne(nu => nu.NotificationType)
                   .HasForeignKey(nu => nu.NotificationId)
                   .OnDelete(DeleteBehavior.Cascade); // Nếu xóa NotificationType thì xóa luôn các NotificationUser liên quan
        }
    }
}
