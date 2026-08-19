using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities
{
    /// <summary>
    /// سجل غير قابل للتعديل لمحاولات تسجيل الدخول. لا يحتوي مطلقاً على كلمة المرور.
    /// </summary>
    [Table("login_attempts")]
    public class LoginAttempt
    {
        [Key]
        [Column("Login_Attempt_ID")]
        public long Login_Attempt_ID { get; set; }

        [Column("User_ID")]
        public int? User_ID { get; set; }

        [Column("Login_Name")]
        public string Login_Name { get; set; } = string.Empty;

        [Column("Company_ID")]
        public string? Company_ID { get; set; }

        [Column("Branch_ID")]
        public int? Branch_ID { get; set; }

        [Column("Fiscal_Year_ID")]
        public int? Fiscal_Year_ID { get; set; }

        [Column("Attempted_At")]
        public DateTime Attempted_At { get; set; } = DateTime.UtcNow;

        [Column("Is_Success")]
        public bool Is_Success { get; set; }

        // رمز داخلي عام مثل INVALID_CREDENTIALS أو ACCOUNT_LOCKED؛ لا يسجل أسراراً.
        [Column("Failure_Reason")]
        public string? Failure_Reason { get; set; }

        [Column("IP_Address")]
        public string? IP_Address { get; set; }

        [Column("User_Agent")]
        public string? User_Agent { get; set; }

        [Column("Device_ID")]
        public string? Device_ID { get; set; }

        [Column("Session_ID")]
        public string? Session_ID { get; set; }

        [Column("Lockout_Until")]
        public DateTime? Lockout_Until { get; set; }
    }
}