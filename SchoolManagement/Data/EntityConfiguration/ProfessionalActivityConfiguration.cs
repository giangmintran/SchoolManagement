using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Data.Entities;

namespace SchoolManagement.Data.EntityConfiguration
{
    public class ProfessionalActivityConfiguration : IEntityTypeConfiguration<ProfessionalActivity>
    {
        public void Configure(EntityTypeBuilder<ProfessionalActivity> builder)
        {
            builder.HasKey(e => e.Id);

            // Cấu hình quan hệ với User
            builder.HasOne(e => e.AppUser)
                    .WithMany() // Hoặc .WithMany(u => u.ProfessionalActivities) nếu trong User có list này
                    .HasForeignKey(e => e.AppUserId)
                    .OnDelete(DeleteBehavior.Cascade); // Xóa user thì xóa luôn nội dung sinh hoạt

            // Cấu hình trường Audit tự động set ngày (tùy chọn nếu không xử lý trong Override SaveChanges)
            builder.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
        }
    }
}
