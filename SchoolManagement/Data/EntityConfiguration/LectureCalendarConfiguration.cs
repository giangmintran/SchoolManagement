using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManagement.Data.Entities;

namespace SchoolManagement.Data.EntityConfiguration
{
    public class LectureCalendarConfiguration : IEntityTypeConfiguration<LectureCalendar>
    {
        public void Configure(EntityTypeBuilder<LectureCalendar> builder)
        {
            builder.ToTable("LectureCalendars");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TeacherName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            builder.Property(x => x.Week)
                .IsRequired();

            builder.ToTable(t => t.HasCheckConstraint("CK_LectureCalendar_Week", "[Week] >= 1 AND [Week] <= 52"));

            builder.Property(x => x.StartDate)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(x => x.EndDate)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany() // hoặc .WithMany(u => u.LectureCalendars)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Details)
                .WithOne(x => x.LectureCalendar)
                .HasForeignKey(x => x.LectureCalendarId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
