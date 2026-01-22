namespace MembershipOperations.Shared.Dto.Audit;

public class AuditLogDto
{
    public long Id { get; set; }
    public string Actor { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; }
    public string? DetailsJson { get; set; }
}
