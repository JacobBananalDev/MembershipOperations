using MembershipOperations.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MembershipOperations.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Member> Members => Set<Member>();
        public DbSet<MembershipPlan> MembershipPlans => Set<MembershipPlan>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<AppUser> Users => Set<AppUser>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Member>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
                e.Property(x => x.LastName).HasMaxLength(100).IsRequired();
                e.Property(x => x.Email).HasMaxLength(256).IsRequired(false);
                e.HasIndex(x => x.Email).IsUnique();
            });

            modelBuilder.Entity<MembershipPlan>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).HasMaxLength(100).IsRequired();
                e.Property(x => x.MonthlyPrice).HasColumnType("decimal(10,2)");
            });

            modelBuilder.Entity<AuditLog>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Actor).HasMaxLength(256).IsRequired();
                e.Property(x => x.Action).HasMaxLength(100).IsRequired();
                e.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
                e.Property(x => x.EntityId).HasMaxLength(50).IsRequired();
            });

            modelBuilder.Entity<AppUser>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Username).HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.Username).IsUnique();

                e.Property(x => x.PasswordHash).IsRequired();
                e.Property(x => x.Role).HasMaxLength(20).IsRequired();
            });
        }
    }
}
