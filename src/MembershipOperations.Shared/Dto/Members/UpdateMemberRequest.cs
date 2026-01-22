namespace MembershipOperations.Shared.Dto.Members;

public class UpdateMemberRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsActive { get; set; }
}
