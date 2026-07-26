using System.Text.Json;
using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class SessionStorageService
{
    private const string SessionKey = "altayer_mobile_session";

    public async Task SaveAsync(LoginResponseDto response)
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
    }

    public async Task<StoredSessionDto?> GetAsync()
    {
        var json = await SecureStorage.Default.GetAsync(SessionKey);
        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<StoredSessionDto>(json);
    }

    public void Clear() => SecureStorage.Default.Remove(SessionKey);
}
