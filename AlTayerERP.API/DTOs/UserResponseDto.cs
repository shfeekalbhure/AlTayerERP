namespace AlTayerERP.API.DTOs;

/// <summary>
/// بيانات المستخدم الآمنة المعادة من الـ API.
/// لا تحتوي مطلقاً على Password_Hash أو كلمة المرور.
/// </summary>
public sealed class UserResponseDto
{
    public int User_ID { get; set; }
    public string Company_ID { get; set; } = string.Empty;
    public int Branch_ID { get; set; }
    public int Role_ID { get; set; }
    public string User_Code { get; set; } = string.Empty;
    public string Full_Name { get; set; } = string.Empty;
    public string Login_Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Notes { get; set; }
    public bool Must_Change_Password { get; set; }
    public bool Is_Active { get; set; }
    public DateTime Created_At { get; set; }
    public DateTime? Updated_At { get; set; }
}