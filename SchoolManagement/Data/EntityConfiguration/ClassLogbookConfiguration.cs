using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Data.Entities;

namespace SchoolManagement.Data.EntityConfiguration
{
    public class ClassLogbookConfiguration : IEntityTypeConfiguration<ClassLogbook>
    {
        public void Configure(EntityTypeBuilder<ClassLogbook> builder)
        {
            builder.HasMany(p => p.LogbookDetails)
                .WithOne(d => d.ClassLogbook)
                .HasForeignKey(d => d.ClassLogbookId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: Một lớp trong 1 tuần của 1 năm chỉ có 1 phiếu
            builder.HasIndex(p => new { p.Class, p.WeekNumber, p.SchoolYear })
                .IsUnique();
        }
    }
}
