using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class TrialBalanceService(HttpClient httpClient, SessionStorageService sessionStorage)
{
    public async Task<TrialBalanceResponseDto> GetAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var session = await sessionStorage.GetAsync() ?? throw new InvalidOperationException("لا توجد جلسة دخول محفوظة.");
        var url = $"api/mobile/trial-balance?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}";
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
                    throw new InvalidOperationException(message.GetString() ?? "تعذر تحميل ميزان المراجعة.");
            }
            catch (JsonException) { }
            throw new InvalidOperationException("تعذر تحميل ميزان المراجعة.");
        }

        return await response.Content.ReadFromJsonAsync<TrialBalanceResponseDto>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("استجابة ميزان المراجعة غير صالحة.");
    }
}
