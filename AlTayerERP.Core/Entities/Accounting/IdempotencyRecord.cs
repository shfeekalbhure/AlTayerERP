using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities.Accounting
{
    /// <summary>
    /// سجل مفاتيح عدم التكرار للعمليات المالية الحساسة.
    /// يحفظ داخل معاملة المستند نفسها، لذلك تعيد إعادة إرسال المفتاح نفسه
    /// النتيجة الأصلية بدلاً من إنشاء مستند ثانٍ.
    /// </summary>
    [Table("idempotency_records")]
    public class IdempotencyRecord
    {
        public const byte InProgress = 1;
        public const byte Completed = 2;

        [Key]
        [Column("Idempotency_Record_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Idempotency_Record_ID { get; set; }

        [Required]
        [MaxLength(80)]
        [Column("Operation")]
        public string Operation { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("Idempotency_Key")]
        public string Idempotency_Key { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("Company_ID")]
        public string Company_ID { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("Branch_ID")]
        public string Branch_ID { get; set; } = string.Empty;

        [Column("Fiscal_Year_ID")]
        public int Fiscal_Year_ID { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("User_ID")]
        public string User_ID { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        [Column("Request_Fingerprint", TypeName = "char(64)")]
        public string Request_Fingerprint { get; set; } = string.Empty;

        [Column("Status")]
        public byte Status { get; set; } = InProgress;

        [Column("Resource_ID")]
        public long? Resource_ID { get; set; }

        [MaxLength(100)]
        [Column("Resource_No")]
        public string? Resource_No { get; set; }

        [Required]
        [Column("Created_At")]
        public DateTime Created_At { get; set; } = DateTime.UtcNow;

        [Column("Completed_At")]
        public DateTime? Completed_At { get; set; }
    }
}
