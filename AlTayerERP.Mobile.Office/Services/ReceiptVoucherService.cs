using System.Net.Http.Headers;
using System.Net.Http.Json;
using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class ReceiptVoucherService(HttpClient httpClient, SessionStorageService sessionStorage)
{
    public async Task<List<ReceiptVoucherListItemDto>> GetAsync(string? voucherNo = null, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        var url = "api/mobile/receipt-vouchers";
        if (!string.IsNullOrWhiteSpace(voucherNo))
            url += $"?voucherNo={Uri.EscapeDataString(voucherNo.Trim())}";

        using var request = CreateRequest(HttpMethod.Get, url, session.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر تحميل سندات القبض.", cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<ReceiptVoucherListItemDto>>(cancellationToken: cancellationToken) ?? [];
    }

    public async Task<ReceiptVoucherDetailsDto> GetByIdAsync(long voucherId, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        using var request = CreateRequest(HttpMethod.Get, $"api/mobile/receipt-vouchers/{voucherId}", session.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر تحميل تفاصيل سند القبض.", cancellationToken);
        return await response.Content.ReadFromJsonAsync<ReceiptVoucherDetailsDto>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("استجابة تفاصيل سند القبض غير صالحة.");
    }

    public async Task DeleteAsync(long voucherId, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        using var request = CreateRequest(HttpMethod.Delete, $"api/FinancialVoucher/{voucherId}", session.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر حذف سند القبض.", cancellationToken);
    }

    public async Task RecordPrintAsync(long voucherId, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        using var request = CreateRequest(HttpMethod.Post, $"api/FinancialVoucher/{voucherId}/record-print", session.AccessToken);
        request.Content = JsonContent.Create(new { Action_Channel = "MOBILE", Device_Name = DeviceInfo.Name });
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر تسجيل طباعة سند القبض.", cancellationToken);
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
        throw MobileApiErrorHandler.CreateException(response, raw, fallback);
    }
}
