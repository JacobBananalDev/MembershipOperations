namespace MembershipOperations.Domain.Entities
{
    public class AuditLog
    {
        public long Id { get; set; }
        
        public string Actor { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;     // e.g. MemberCreated
        public string EntityType { get; set; } = string.Empty; // e.g. Member
        public string EntityId { get; set; } = string.Empty;   // e.g. "123"

        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

        public string? DetailsJson { get; set; }
    }
}
