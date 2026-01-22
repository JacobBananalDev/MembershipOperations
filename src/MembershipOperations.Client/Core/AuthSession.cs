namespace MembershipOperations.Client.Core;

public class AuthSession
{
    public string? Token { get; private set; }
    public string? Username { get; private set; }
    public string? Role { get; private set; }
    public DateTime? ExpiresUtc { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);

    public void Set(string token, string username, string role, DateTime expiresUtc)
    {
        Token = token;
        Username = username;
        Role = role;
        ExpiresUtc = expiresUtc;
    }

    public void Clear()
    {
        Token = null;
        Username = null;
        Role = null;
        ExpiresUtc = null;
    }
}
