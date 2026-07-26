using System.Net.Http.Json;
using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class AuthenticationService(HttpClient httpClient, SessionStorageService sessionStorage)
{
    public async Task<LoginOptionsResponseDto> GetLoginOptionsAsync(
        LoginOptionsRequestDto request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("api/Auth/LoginOptions", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(NormalizeError(message, "تعذر تحميل خيارات الدخول."));
        }

        return await response.Content.ReadFromJsonAsync<LoginOptionsResponseDto>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("استجابة خيارات الدخول غير صالحة.");
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("api/Auth/Login", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(NormalizeError(message, "تعذر تسجيل الدخول."));
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>(cancellationToken: cancellationToken)
                     ?? throw new InvalidOperationException("استجابة تسجيل الدخول غير صالحة.");

        await sessionStorage.SaveAsync(result);
        return result;
    }

    private static string NormalizeError(string? raw, string fallback)
    {
        if (string.IsNullOrWhiteSpace(raw)) return fallback;

        var message = raw.Trim();
        if (message.Length >= 2 && message.StartsWith('"') && message.EndsWith('"'))
            message = message[1..^1];

        return message.Replace("\\u0022", "\"");
    }
}
