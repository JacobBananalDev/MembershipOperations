using MembershipOperations.Domain.Entities;
using MembershipOperations.Domain.Services;
using MembershipOperations.Infrastructure.Persistence;

namespace MembershipOperations.Infrastructure.Services;

public class AuditLogger : IAuditLogger
{
    private readonly ApplicationDbContext _db;

    public AuditLogger(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(string actor, string action, string entityType, string entityId, string? detailsJson = null)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            Actor = actor,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            TimestampUtc = DateTime.UtcNow,
            DetailsJson = detailsJson
        });

        await _db.SaveChangesAsync();
    }
}
