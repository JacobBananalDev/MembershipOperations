namespace MembershipOperations.Domain.Services;

public interface IAuditLogger
{
    Task LogAsync(string actor, string action, string entityType, string entityId, string? detailsJson = null);
}
