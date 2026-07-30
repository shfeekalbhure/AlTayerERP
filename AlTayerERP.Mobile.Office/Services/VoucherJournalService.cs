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
        MobileApiErrorHandler.EnsureSuccess(response);

        return await response.Content.ReadFromJsonAsync<VoucherJournalDto>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("استجابة القيد المحاسبي غير صالحة.");
    }
}
