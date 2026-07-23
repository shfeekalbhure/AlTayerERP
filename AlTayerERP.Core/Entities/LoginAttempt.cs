using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities;

/// <summary>سجل غير حساس لمحاولات الدخول الناجحة والفاشلة.</summary>
[Table("login_attempts")]
public class LoginAttempt
{
    [Key]
    [Column("Login_Attempt_ID")]
    public long Login_Attempt_ID { get; set; }

    [Column("User_ID")]
    public int? User_ID { get; set; }

    [MaxLength(50)]
    [Column("Company_ID")]
    public string? Company_ID { get; set; }

    [Column("Branch_ID")]
    public int? Branch_ID { get; set; }

    [MaxLength(100)]
    [Column("Login_Name")]
    public string? Login_Name { get; set; }

    [Column("Is_Success")]
    public bool Is_Success { get; set; }

    [MaxLength(200)]
    [Column("Failure_Reason")]
    public string? Failure_Reason { get; set; }

    [MaxLength(64)]
    [Column("IP_Address")]
    public string? IP_Address { get; set; }

    [MaxLength(100)]
    [Column("Device_ID")]
    public string? Device_ID { get; set; }

    [Column("Attempted_At")]
    public DateTime Attempted_At { get; set; } = DateTime.UtcNow;
}