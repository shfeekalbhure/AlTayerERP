using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities.Accounting
{
    /// <summary>
    /// يمثل حالات السندات المالية
    /// مثل: معلق - مرحل - معتمد - ملغي.
    /// </summary>
    [Table("voucher_statuses")]
    public class VoucherStatus
    {
        /// <summary>
        /// المفتاح الرئيسي لحالة السند.
        /// </summary>
        [Key]
        [Column("Voucher_Status_ID")]
        public int Voucher_Status_ID { get; set; }

        /// <summary>
        /// الكود المختصر للحالة.
        /// مثال: POSTED أو DRAFT.
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column("Voucher_Status_Code")]
        public string Voucher_Status_Code { get; set; } = string.Empty;

        /// <summary>
        /// اسم الحالة باللغة العربية.
        /// </summary>
        [Required]
        [MaxLength(150)]
        [Column("Voucher_Status_Name_AR")]
        public string Voucher_Status_Name_AR { get; set; } = string.Empty;

        /// <summary>
        /// اسم الحالة باللغة الإنجليزية.
        /// </summary>
        [MaxLength(150)]
        [Column("Voucher_Status_Name_EN")]
        public string? Voucher_Status_Name_EN { get; set; }

        /// <summary>
        /// هل الحالة نشطة ويمكن استخدامها؟
        /// </summary>
        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;

        /// <summary>
        /// ترتيب ظهور الحالة في القوائم.
        /// </summary>
        [Column("Sort_Order")]
        public int Sort_Order { get; set; }
    }
}