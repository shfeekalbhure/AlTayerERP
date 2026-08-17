using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class DocumentSearchService(HttpClient httpClient, SessionStorageService sessionStorage)
{
    public async Task<List<DocumentSearchResultDto>> SearchAsync(string query, string? type = null, CancellationToken cancellationToken = default)
    {
        var session = await sessionStorage.GetAsync() ?? throw new InvalidOperationException("لا توجد جلسة دخول محفوظة.");
        var url = $"api/mobile/document-search?query={Uri.EscapeDataString(query.Trim())}";
        if (!string.IsNullOrWhiteSpace(type) && type != "ALL") url += $"&type={Uri.EscapeDataString(type)}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);
            try
            {
                using var json = JsonDocument.Parse(raw);
                if (json.RootElement.TryGetProperty("message", out var message))
                    throw new InvalidOperationException(message.GetString() ?? "تعذر البحث في المستندات.");
            }
            catch (JsonException) { }
            throw new InvalidOperationException("تعذر البحث في المستندات.");
        }
        return await response.Content.ReadFromJsonAsync<List<DocumentSearchResultDto>>(cancellationToken: cancellationToken) ?? [];
    }
}
