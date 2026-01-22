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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Member>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
                e.Property(x => x.LastName).HasMaxLength(100).IsRequired();
                e.Property(x => x.Email).HasMaxLength(256);
                e.HasIndex(x => x.Email);
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
        }
    }
}
