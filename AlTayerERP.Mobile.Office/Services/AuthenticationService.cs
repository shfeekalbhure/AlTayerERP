using System.Net.Http.Headers;
using System.Net.Http.Json;
using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class AuthenticationService(HttpClient httpClient, SessionStorageService sessionStorage, ApiConnectionDiagnosticsService diagnostics)
{
    public async Task<List<LoginCompanyOptionDto>> GetLoginCompaniesAsync(CancellationToken cancellationToken = default) =>
        await GetJsonAsync<List<LoginCompanyOptionDto>>(HttpMethod.Get, "api/Auth/LoginCompanies", null, null, cancellationToken) ?? [];

    public Task<LoginOptionsResponseDto> GetLoginOptionsAsync(LoginOptionsRequestDto request, CancellationToken cancellationToken = default) =>
        GetJsonAsync<LoginOptionsResponseDto>(HttpMethod.Post, "api/Auth/LoginOptions", request, null, cancellationToken);

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var result = await GetJsonAsync<LoginResponseDto>(HttpMethod.Post, "api/Auth/Login", request, null, cancellationToken);
        await sessionStorage.SaveAsync(result);
        return result;
    }

    public async Task<StoredSessionDto?> RestoreSessionAsync(CancellationToken cancellationToken = default)
    {
        var stored = await sessionStorage.GetAsync();
        if (stored == null || stored.AccessTokenExpiresAt <= DateTimeOffset.UtcNow) { sessionStorage.Clear(); return null; }
        try { await GetJsonAsync<object>(HttpMethod.Get, "api/Auth/CurrentSession", null, stored, cancellationToken); return stored; }
        catch (ApiDiagnosticException) { sessionStorage.Clear(); return null; }
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        var stored = await sessionStorage.GetAsync();
        try { if (stored != null) await SendAsync(HttpMethod.Post, "api/Auth/Logout", null, stored, cancellationToken); }
        finally { sessionStorage.Clear(); }
    }

    private async Task<T> GetJsonAsync<T>(HttpMethod method, string endpoint, object? body, StoredSessionDto? session, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(method, endpoint, body, session, cancellationToken);
        try
        {
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken)
                   ?? throw new ApiDiagnosticException(diagnostics.Create(endpoint, true, (int)response.StatusCode, ApiErrorType.DeserializeFailure, session));
        }
        catch (ApiDiagnosticException) { throw; }
        catch (Exception ex) when (ex is System.Text.Json.JsonException or NotSupportedException)
        {
            throw new ApiDiagnosticException(diagnostics.Create(endpoint, true, (int)response.StatusCode, ApiErrorType.DeserializeFailure, session));
        }
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string endpoint, object? body, StoredSessionDto? session, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, endpoint);
        if (body != null) request.Content = JsonContent.Create(body);
        if (session != null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        try
        {
            var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode) return response;
            var diagnostic = diagnostics.FromStatus(endpoint, (int)response.StatusCode, session);
            response.Dispose();
            throw new ApiDiagnosticException(diagnostic);
        }
        catch (ApiDiagnosticException) { throw; }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex) { throw new ApiDiagnosticException(diagnostics.FromException(endpoint, ex, session)); }
    }
}

public sealed class ApiDiagnosticException(ApiDiagnosticResult diagnostic) : Exception(diagnostic.Message)
{
    public ApiDiagnosticResult Diagnostic { get; } = diagnostic;
}
