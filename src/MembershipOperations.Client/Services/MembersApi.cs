using MembershipOperations.Client.Core;
using MembershipOperations.Shared.Dto.Common;
using MembershipOperations.Shared.Dto.Members;

namespace MembershipOperations.Client.Services;

public class MembersApi
{
    private readonly ApiClient _api;

    public MembersApi(ApiClient api)
    {
        _api = api;
    }

    public async Task<PagedResult<MemberDto>?> GetMembersAsync(
        int pageNumber = 1,
        int pageSize = 50,
        string? search = null,
        bool? activeOnly = null,
        CancellationToken ct = default)
    {
        var url = $"/api/members?pageNumber={pageNumber}&pageSize={pageSize}";

        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";

        if (activeOnly.HasValue)
            url += $"&activeOnly={activeOnly.Value.ToString().ToLowerInvariant()}";

        return await _api.GetAsync<PagedResult<MemberDto>>(url, ct);
    }
}
