using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Data.Entities;

namespace SchoolManagement.Data.EntityConfiguration
{
    public class ClassLogbookDetailConfiguration : IEntityTypeConfiguration<ClassLogbookDetail>
    {
        public void Configure(EntityTypeBuilder<ClassLogbookDetail> builder)
        {
            // Unique constraint (Optional): Trong 1 sổ, không được trùng Thứ + Tiết
            builder.HasIndex(d => new { d.ClassLogbookId, d.DayOfWeek, d.PeriodIndex })
                .IsUnique();
        }
    }
}
