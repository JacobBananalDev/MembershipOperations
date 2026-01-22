using System.Net;
using System.Net.Http.Json;
using MembershipOperations.Client.Core;
using MembershipOperations.Shared.Dto.Auth;

namespace MembershipOperations.Client.Services;

public class AuthApi
{
    private readonly ApiClient _api;

    public AuthApi(ApiClient api)
    {
        _api = api;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var response = await _api.PostAsync("/api/auth/login", request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new InvalidOperationException("Invalid username or password.");

        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: ct);
        if (auth == null || string.IsNullOrWhiteSpace(auth.Token))
            throw new InvalidOperationException("Login succeeded but no token was returned.");

        return auth;
    }

    public async Task RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var response = await _api.PostAsync("/api/auth/register", request, ct);

        if (response.StatusCode == HttpStatusCode.Conflict)
            throw new InvalidOperationException("Username already exists.");

        response.EnsureSuccessStatusCode();
    }
}
