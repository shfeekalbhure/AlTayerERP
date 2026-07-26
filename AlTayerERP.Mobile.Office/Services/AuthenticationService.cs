using System.Net.Http.Headers;
using System.Net.Http.Json;
using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class AuthenticationService(HttpClient httpClient, SessionStorageService sessionStorage)
{
    public async Task<List<LoginCompanyOptionDto>> GetLoginCompaniesAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync("api/Auth/LoginCompanies", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(NormalizeError(message, "تعذر تحميل الشركات."));
        }

        return await response.Content.ReadFromJsonAsync<List<LoginCompanyOptionDto>>(
                   cancellationToken: cancellationToken)
               ?? [];
    }

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

    public async Task<StoredSessionDto?> RestoreSessionAsync(CancellationToken cancellationToken = default)
    {
        var stored = await sessionStorage.GetAsync();
        if (stored == null || stored.AccessTokenExpiresAt <= DateTimeOffset.UtcNow)
        {
            sessionStorage.Clear();
            return null;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/Auth/CurrentSession");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", stored.AccessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            sessionStorage.Clear();
            return null;
        }

        return stored;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        var stored = await sessionStorage.GetAsync();
        try
        {
            if (stored != null)
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "api/Auth/Logout");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", stored.AccessToken);
                using var _ = await httpClient.SendAsync(request, cancellationToken);
            }
        }
        finally
        {
            sessionStorage.Clear();
        }
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
