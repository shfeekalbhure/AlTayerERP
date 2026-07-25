namespace AlTayerERP.API.DTOs;

/// <summary>
/// طلب تغيير كلمة مرور المستخدم الحالي. لا يحمل معرف مستخدم لأن الهوية تستخرج من الجلسة الموثوقة.
/// </summary>
public sealed class ChangePasswordDto
{
    public string Current_Password { get; set; } = string.Empty;
    public string New_Password { get; set; } = string.Empty;
    public string Confirm_New_Password { get; set; } = string.Empty;
}
