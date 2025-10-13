using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Psychology.Domain.Entities;
using System.Globalization;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace Psychology.Infrastructure.Persistence
{
    public class AppDbContext
        : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ErrorLog> ErrorLogs => Set<ErrorLog>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Global soft-delete filter for all BaseEntity
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var prop = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                    var compare = Expression.Equal(prop, Expression.Constant(false));
                    var lambda = Expression.Lambda(compare, parameter);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }

            base.OnModelCreating(modelBuilder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            var utcNow = DateTime.UtcNow;

            // Iran timezone + Persian calendar helpers (inline for brevity)
            var iranTz = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")
                : TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran");
            var local = TimeZoneInfo.ConvertTimeFromUtc(utcNow, iranTz);
            var pc = new PersianCalendar();
            string persianDate = $"{pc.GetYear(local):0000}/{pc.GetMonth(local):00}/{pc.GetDayOfMonth(local):00}";
            string hhmm = local.ToString("HHmm");

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreateDateAndTime = utcNow;
                        entry.Entity.UpdateDateAndTime = utcNow;

                        entry.Entity.CreateDate = persianDate;
                        entry.Entity.CreateTime = hhmm;
                        entry.Entity.UpdateDate = persianDate;
                        entry.Entity.UpdateTime = hhmm;

                        entry.Entity.IsDeleted = false;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdateDateAndTime = utcNow;
                        entry.Entity.UpdateDate = persianDate;
                        entry.Entity.UpdateTime = hhmm;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.UpdateDateAndTime = utcNow;
                        entry.Entity.UpdateDate = persianDate;
                        entry.Entity.UpdateTime = hhmm;
                        break;
                }
            }

            return base.SaveChangesAsync(ct);
        }
    }
}
