using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data.Configurations;
using SchoolManagement.Data.Entities;
using SchoolManagement.Data.EntityConfiguration;

namespace SchoolManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<LectureCalendar> LectureCalendars { get; set; }
        public DbSet<ClassLogbook> ClassLogbooks { get; set; }
        public DbSet<ClassLogbookDetail> ClassLogbookDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfiguration(new LectureCalendarConfiguration());
            builder.ApplyConfiguration(new LectureCalendarDetailConfiguration());
            builder.ApplyConfiguration(new ClassLogbookConfiguration());
            builder.ApplyConfiguration(new ClassLogbookDetailConfiguration());
        }

        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is IAuditable &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (IAuditable)entry.Entity;
                var now = DateTime.Now;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedDate = now;
                    entity.ModifiedDate = now;
                }
                else
                {
                    entry.Property(nameof(IAuditable.CreatedDate)).IsModified = false;
                    entity.ModifiedDate = now;
                }
            }
        }
    }
}
