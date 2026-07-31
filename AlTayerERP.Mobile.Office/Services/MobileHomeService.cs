using System.Net.Http.Headers;
using System.Net.Http.Json;
using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class MobileHomeService(HttpClient httpClient, SessionStorageService sessionStorage)
{
    public async Task<MobileHomePermissionsResponseDto> GetPermissionsAsync(
        CancellationToken cancellationToken = default)
    {
        var session = await sessionStorage.GetAsync()
                      ?? throw new InvalidOperationException("لا توجد جلسة محفوظة. سجل الدخول من جديد.");

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/mobile/home/permissions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        MobileApiErrorHandler.EnsureSuccess(response);

        return await response.Content.ReadFromJsonAsync<MobileHomePermissionsResponseDto>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("استجابة صلاحيات الشاشة الرئيسية غير صالحة.");
    }
}
