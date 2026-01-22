using MembershipOperations.Domain.Entities;
using MembershipOperations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MembershipOperations.Api.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        await db.Database.MigrateAsync();

        if (await db.Members.AnyAsync())
            return;

        db.Members.AddRange(
            new Member { FirstName = "Jacob", LastName = "Bananal", Email = "jacob@example.com", IsActive = true },
            new Member { FirstName = "Ava", LastName = "Nguyen", Email = "ava@example.com", IsActive = true },
            new Member { FirstName = "Mateo", LastName = "Garcia", Email = "mateo@example.com", IsActive = false }
        );

        await db.SaveChangesAsync();
    }
}
