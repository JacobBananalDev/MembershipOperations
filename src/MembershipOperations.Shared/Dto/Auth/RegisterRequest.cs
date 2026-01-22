using System.ComponentModel.DataAnnotations;

namespace MembershipOperations.Shared.Dto.Auth;

public class RegisterRequest
{
    [Required, StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Staff";
}
