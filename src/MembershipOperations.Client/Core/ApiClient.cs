using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MembershipOperations.Client.Core;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly AuthSession _session;

    public ApiClient(HttpClient http, AuthSession session)
    {
        _http = http;
        _session = session;
    }

    private void ApplyAuthHeader()
    {
        _http.DefaultRequestHeaders.Authorization = null;

        if (_session.IsAuthenticated)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _session.Token);
        }
    }

    public async Task<T?> GetAsync<T>(string url, CancellationToken ct = default)
    {
        ApplyAuthHeader();
        return await _http.GetFromJsonAsync<T>(url, ct);
    }

    public async Task<HttpResponseMessage> PostAsync<TBody>(string url, TBody body, CancellationToken ct = default)
    {
        ApplyAuthHeader();
        return await _http.PostAsJsonAsync(url, body, ct);
    }

    public async Task<HttpResponseMessage> PutAsync<TBody>(string url, TBody body, CancellationToken ct = default)
    {
        ApplyAuthHeader();
        return await _http.PutAsJsonAsync(url, body, ct);
    }

    public async Task<HttpResponseMessage> DeleteAsync(string url, CancellationToken ct = default)
    {
        ApplyAuthHeader();
        return await _http.DeleteAsync(url, ct);
    }
}
