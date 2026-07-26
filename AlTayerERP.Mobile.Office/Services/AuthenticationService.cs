using System.Net.Http.Json;
using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class AuthenticationService(HttpClient httpClient, SessionStorageService sessionStorage)
{
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("api/Auth/Login", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                ? "تعذر تسجيل الدخول."
                : message.Trim('"'));
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>(cancellationToken: cancellationToken)
                     ?? throw new InvalidOperationException("استجابة تسجيل الدخول غير صالحة.");

        await sessionStorage.SaveAsync(result);
        return result;
    }
}
