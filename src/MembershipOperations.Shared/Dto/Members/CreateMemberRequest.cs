using System.ComponentModel.DataAnnotations;

namespace MembershipOperations.Shared.Dto.Members;

public class CreateMemberRequest
{
    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [EmailAddress, StringLength(256)]
    public string? Email { get; set; }
}
