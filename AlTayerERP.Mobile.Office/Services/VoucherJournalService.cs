using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class VoucherJournalService(HttpClient httpClient, SessionStorageService sessionStorage)
{
    public async Task<VoucherJournalDto> GetAsync(long voucherId, CancellationToken cancellationToken = default)
    {
        var session = await sessionStorage.GetAsync()
            ?? throw new InvalidOperationException("لا توجد جلسة دخول محفوظة.");

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/mobile/voucher-journal/{voucherId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(raw))
            {
                try
                {
                    using var json = JsonDocument.Parse(raw);
                    if (json.RootElement.TryGetProperty("message", out var message))
                        throw new InvalidOperationException(message.GetString() ?? "تعذر تحميل القيد المحاسبي.");
                }
                catch (JsonException)
                {
                }
            }
            throw new InvalidOperationException("تعذر تحميل القيد المحاسبي.");
        }

        return await response.Content.ReadFromJsonAsync<VoucherJournalDto>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("استجابة القيد المحاسبي غير صالحة.");
    }
}
