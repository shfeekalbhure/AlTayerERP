using System.Net.Http.Headers;
using System.Net.Http.Json;
using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class PaymentRequestService(HttpClient httpClient, SessionStorageService sessionStorage)
{
    public async Task<List<PaymentRequestListItemDto>> GetAsync(
        string? status = null,
        string? requestNo = null,
        CancellationToken cancellationToken = default)
    {
        var session = await sessionStorage.GetAsync()
            ?? throw new InvalidOperationException("لا توجد جلسة دخول محفوظة.");

        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(status))
            query.Add($"status={Uri.EscapeDataString(status)}");
        if (!string.IsNullOrWhiteSpace(requestNo))
            query.Add($"requestNo={Uri.EscapeDataString(requestNo.Trim())}");

        var url = "api/payment-requests" + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                ? "تعذر تحميل طلبات الصرف."
                : message.Trim('"'));
        }

        return await response.Content.ReadFromJsonAsync<List<PaymentRequestListItemDto>>(
                   cancellationToken: cancellationToken)
               ?? [];
    }
}
