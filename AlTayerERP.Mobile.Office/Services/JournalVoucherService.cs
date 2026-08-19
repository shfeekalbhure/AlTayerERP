using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class JournalVoucherService(HttpClient httpClient, SessionStorageService sessionStorage)
{
    public async Task<List<JournalVoucherListItemDto>> GetAsync(string? voucherNo = null, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        var url = "api/mobile/journal-vouchers";
        if (!string.IsNullOrWhiteSpace(voucherNo))
            url += $"?voucherNo={Uri.EscapeDataString(voucherNo.Trim())}";

        using var request = CreateRequest(HttpMethod.Get, url, session.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر تحميل القيود اليومية.", cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<JournalVoucherListItemDto>>(cancellationToken: cancellationToken) ?? [];
    }

    public async Task<JournalVoucherDetailsDto> GetByIdAsync(long voucherId, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        using var request = CreateRequest(HttpMethod.Get, $"api/mobile/journal-vouchers/{voucherId}", session.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر تحميل تفاصيل القيد اليومي.", cancellationToken);
        return await response.Content.ReadFromJsonAsync<JournalVoucherDetailsDto>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("استجابة تفاصيل القيد اليومي غير صالحة.");
    }

    private async Task<StoredSessionDto> GetSessionAsync() =>
        await sessionStorage.GetAsync() ?? throw new InvalidOperationException("لا توجد جلسة دخول محفوظة.");

    private static HttpRequestMessage CreateRequest(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string fallback, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode) return;
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(raw))
        {
            try
            {
                using var json = JsonDocument.Parse(raw);
                if (json.RootElement.TryGetProperty("message", out var message))
                    throw new InvalidOperationException(message.GetString() ?? fallback);
            }
            catch (JsonException)
            {
            }
        }
        throw new InvalidOperationException(fallback);
    }
}
