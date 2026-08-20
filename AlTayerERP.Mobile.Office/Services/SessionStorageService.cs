using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class SessionStorageService(HttpClient httpClient, DeviceIdentityService deviceIdentity)
{
    private const string SessionKey = "altayer_mobile_session";
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    public async Task<StoredSessionDto> SaveAsync(LoginResponseDto response)
    {
        var session = new StoredSessionDto
        {
            UserId = response.User_ID,
            FullName = response.Full_Name,
            CompanyId = response.Company_ID,
            BranchId = response.Branch_ID,
            YearId = response.Year_ID,
            SessionId = response.Session_ID,
            AccessToken = response.Access_Token,
            AccessTokenExpiresAt = response.Access_Token_Expires_At,
            RefreshToken = response.Refresh_Token,
            RefreshTokenExpiresAt = response.Refresh_Token_Expires_At
        };

        await SecureStorage.Default.SetAsync(SessionKey, JsonSerializer.Serialize(session));
        return session;
    }

    /// <summary>
    /// يعيد جلسة صالحة للاستخدام. عند انتهاء رمز الوصول، يجدد الجلسة مرة واحدة فقط
    /// مهما تزامنت طلبات الخدمات، ثم يحفظ زوج الرموز الجديد قبل إعادة الجلسة.
    /// </summary>
    public async Task<StoredSessionDto?> GetAsync(CancellationToken cancellationToken = default)
    {
        var stored = await GetStoredAsync();
        if (stored is null || stored.AccessTokenExpiresAt > DateTimeOffset.UtcNow)
            return stored;

        if (stored.RefreshTokenExpiresAt <= DateTimeOffset.UtcNow || string.IsNullOrWhiteSpace(stored.RefreshToken))
        {
            Clear();
            return null;
        }

        await RefreshLock.WaitAsync(cancellationToken);
        try
        {
            // قد يكون طلب آخر قد أتم التدوير بينما كان هذا الطلب بانتظار القفل.
            stored = await GetStoredAsync();
            if (stored is null || stored.AccessTokenExpiresAt > DateTimeOffset.UtcNow)
                return stored;

            if (stored.RefreshTokenExpiresAt <= DateTimeOffset.UtcNow || string.IsNullOrWhiteSpace(stored.RefreshToken))
            {
                Clear();
                return null;
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, "api/Auth/Refresh")
            {
                Content = JsonContent.Create(new RefreshRequestDto
                {
                    Refresh_Token = stored.RefreshToken,
                    Device_ID = await deviceIdentity.GetAsync()
                })
            };
            using var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                Clear();
                return null;
            }

            response.EnsureSuccessStatusCode();
            var renewed = await response.Content.ReadFromJsonAsync<LoginResponseDto>(cancellationToken: cancellationToken)
                          ?? throw new InvalidOperationException("استجابة تجديد جلسة الدخول غير صالحة.");
            return await SaveAsync(renewed);
        }
        finally
        {
            RefreshLock.Release();
        }
    }

    /// <summary>
    /// يعيد الجلسة المخزنة بلا اتصال شبكي؛ يستخدم فقط لمسارات الاستعادة والخروج.
    /// </summary>
    public async Task<StoredSessionDto?> GetStoredAsync()
    {
        var json = await SecureStorage.Default.GetAsync(SessionKey);
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<StoredSessionDto>(json);
        }
        catch (JsonException)
        {
            Clear();
            return null;
        }
    }

    public void Clear() => SecureStorage.Default.Remove(SessionKey);
}
