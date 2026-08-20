using System.Net.Http.Headers;
using System.Net.Http.Json;
using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class PaymentRequestReferenceService(HttpClient httpClient, SessionStorageService sessionStorage)
{
    public async Task<PaymentRequestReferencesDto> GetAsync(
        bool includeLookupData = true,
        CancellationToken cancellationToken = default)
    {
        var session = await sessionStorage.GetAsync()
            ?? throw new InvalidOperationException("لا توجد جلسة دخول محفوظة.");

        var uri = includeLookupData
            ? "api/mobile/payment-request-references"
            : "api/mobile/payment-request-references?includeLookupData=false";
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(MobileApiErrorHandler.FromPayload(
                raw,
                "تعذر تحميل الحسابات والعملات ومراكز التكلفة."));
        }

        return await response.Content.ReadFromJsonAsync<PaymentRequestReferencesDto>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("استجابة البيانات المرجعية غير صالحة.");
    }

    public async Task<PaymentRequestReferenceLookupDto> GetLookupAsync(
        string resource,
        string? search = null,
        int limit = 25,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(resource))
            throw new ArgumentException("نوع البيانات المرجعية مطلوب.", nameof(resource));

        var session = await sessionStorage.GetAsync()
            ?? throw new InvalidOperationException("لا توجد جلسة دخول محفوظة.");
        var uri = $"api/mobile/payment-request-references/lookup?resource={Uri.EscapeDataString(resource)}&limit={limit}";
        if (!string.IsNullOrWhiteSpace(search))
            uri += $"&search={Uri.EscapeDataString(search.Trim())}";

        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(MobileApiErrorHandler.FromPayload(
                raw,
                "تعذر تحميل قائمة البيانات المرجعية."));
        }

        return await response.Content.ReadFromJsonAsync<PaymentRequestReferenceLookupDto>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("استجابة البحث في البيانات المرجعية غير صالحة.");
    }
}
