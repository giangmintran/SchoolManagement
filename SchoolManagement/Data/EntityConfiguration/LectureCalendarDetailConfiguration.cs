using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Data.Entities;

namespace SchoolManagement.Data.Configurations
{
    public class LectureCalendarDetailConfiguration : IEntityTypeConfiguration<LectureCalendarDetail>
    {
        public void Configure(EntityTypeBuilder<LectureCalendarDetail> builder)
        {
            builder.ToTable("LectureCalendarDetails");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Class)
                .IsRequired(false)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnType("varchar(10)");

            builder.Property(x => x.Subject)
                .IsRequired(false)
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");
     
            builder.Property(x => x.LessonTitle)
                .HasMaxLength(1024)
                .HasColumnType("nvarchar(1024)");

            builder.Property(x => x.Note)
                .IsRequired(false);
        }
    }
}