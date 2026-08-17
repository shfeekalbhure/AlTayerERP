using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities
{
    /// <summary>
    /// رمز تجديد مخزن على هيئة بصمة SHA-256 فقط. القيمة الأصلية لا تخزن في قاعدة البيانات.
    /// </summary>
    [Table("refresh_tokens")]
    public class RefreshToken
    {
        [Key]
        [Column("Refresh_Token_ID")]
        public long Refresh_Token_ID { get; set; }

        [Column("Token_Hash")]
        public string Token_Hash { get; set; } = string.Empty;

        [Column("Session_ID")]
        public string Session_ID { get; set; } = string.Empty;

        [Column("User_ID")]
        public int User_ID { get; set; }

        [Column("Company_ID")]
        public string Company_ID { get; set; } = string.Empty;

        [Column("Branch_ID")]
        public int Branch_ID { get; set; }

        [Column("Fiscal_Year_ID")]
        public int Fiscal_Year_ID { get; set; }

        [Column("Device_ID")]
        public string Device_ID { get; set; } = string.Empty;

        [Column("Created_At")]
        public DateTime Created_At { get; set; } = DateTime.UtcNow;

        [Column("Expires_At")]
        public DateTime Expires_At { get; set; }

        [Column("Revoked_At")]
        public DateTime? Revoked_At { get; set; }

        [Column("Replaced_By_Hash")]
        public string? Replaced_By_Hash { get; set; }

        [Column("Revoked_Reason")]
        public string? Revoked_Reason { get; set; }
    }
}