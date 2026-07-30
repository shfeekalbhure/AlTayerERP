using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class GeneralLedgerService(HttpClient httpClient, SessionStorageService sessionStorage)
{
    public async Task<List<GeneralLedgerAccountDto>> GetAccountsAsync(CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        using var request = CreateRequest(HttpMethod.Get, "api/mobile/general-ledger/accounts", session.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر تحميل الحسابات.", cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<GeneralLedgerAccountDto>>(cancellationToken: cancellationToken) ?? [];
    }

    public async Task<GeneralLedgerResponseDto> GetAsync(string accountId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        var url = $"api/mobile/general-ledger?accountId={Uri.EscapeDataString(accountId)}&fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}";
        using var request = CreateRequest(HttpMethod.Get, url, session.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر تحميل كشف الأستاذ العام.", cancellationToken);
        return await response.Content.ReadFromJsonAsync<GeneralLedgerResponseDto>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("استجابة الأستاذ العام غير صالحة.");
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
        MobileApiErrorHandler.EnsureSuccess(response);
        await Task.CompletedTask;
    }
}
