namespace AlTayerERP.Mobile.Office.DTOs;

public sealed class LoginRequestDto
{
    public string Company_ID { get; set; } = string.Empty;
    public int Branch_ID { get; set; }
    public int Year_ID { get; set; }
    public int User_ID { get; set; }
    public string Login_Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Device_ID { get; set; }
}

public sealed class LoginResponseDto
{
    public int User_ID { get; set; }
    public string Full_Name { get; set; } = string.Empty;
    public string Login_Name { get; set; } = string.Empty;
    public int Role_ID { get; set; }
    public int Branch_ID { get; set; }
    public string Company_ID { get; set; } = string.Empty;
    public int Year_ID { get; set; }
    public bool Is_System_Admin { get; set; }
    public bool Must_Change_Password { get; set; }
    public string Session_ID { get; set; } = string.Empty;
    public string Access_Token { get; set; } = string.Empty;
    public DateTimeOffset Access_Token_Expires_At { get; set; }
    public string Refresh_Token { get; set; } = string.Empty;
    public DateTimeOffset Refresh_Token_Expires_At { get; set; }
    public string Token_Type { get; set; } = "Bearer";
}

public sealed class RefreshRequestDto
{
    public string Refresh_Token { get; set; } = string.Empty;
    public string? Device_ID { get; set; }
}

public sealed class StoredSessionDto
{
    public int UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string CompanyId { get; init; } = string.Empty;
    public int BranchId { get; init; }
    public int YearId { get; init; }
    public string SessionId { get; init; } = string.Empty;
    public string AccessToken { get; init; } = string.Empty;
    public DateTimeOffset AccessTokenExpiresAt { get; init; }
    public string RefreshToken { get; init; } = string.Empty;
    public DateTimeOffset RefreshTokenExpiresAt { get; init; }
}
