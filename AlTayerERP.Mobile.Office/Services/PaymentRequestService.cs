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
        var session = await GetSessionAsync();
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(status))
            query.Add($"status={Uri.EscapeDataString(status)}");
        if (!string.IsNullOrWhiteSpace(requestNo))
            query.Add($"requestNo={Uri.EscapeDataString(requestNo.Trim())}");

        var url = "api/payment-requests" + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);
        using var request = CreateRequest(HttpMethod.Get, url, session.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر تحميل طلبات الصرف.", cancellationToken);

        return await response.Content.ReadFromJsonAsync<List<PaymentRequestListItemDto>>(
                   cancellationToken: cancellationToken)
               ?? [];
    }

    public async Task<PaymentRequestListItemDto> CreateAsync(
        CreatePaymentRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        using var request = CreateRequest(HttpMethod.Post, "api/payment-requests", session.AccessToken);
        request.Content = JsonContent.Create(dto);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر حفظ طلب الصرف.", cancellationToken);

        return await response.Content.ReadFromJsonAsync<PaymentRequestListItemDto>(
                   cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("استجابة حفظ طلب الصرف غير صالحة.");
    }

    public async Task SubmitAsync(long id, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync();
        using var request = CreateRequest(HttpMethod.Post, $"api/payment-requests/{id}/submit", session.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "تعذر إرسال طلب الصرف للمراجعة.", cancellationToken);
    }

    private async Task<StoredSessionDto> GetSessionAsync() =>
        await sessionStorage.GetAsync()
        ?? throw new InvalidOperationException("لا توجد جلسة دخول محفوظة.");

    private static HttpRequestMessage CreateRequest(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string fallback,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var message = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
            ? fallback
            : message.Trim().Trim('"'));
    }
}
