using System.Net.Http.Headers;
using System.Net.Http.Json;
using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class PaymentRequestReferenceService(HttpClient httpClient, SessionStorageService sessionStorage)
{
    public async Task<PaymentRequestReferencesDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var session = await sessionStorage.GetAsync()
            ?? throw new InvalidOperationException("لا توجد جلسة دخول محفوظة.");

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/mobile/payment-request-references");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                ? "تعذر تحميل الحسابات والعملات ومراكز التكلفة."
                : message.Trim().Trim('"'));
        }

        return await response.Content.ReadFromJsonAsync<PaymentRequestReferencesDto>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("استجابة البيانات المرجعية غير صالحة.");
    }
}
