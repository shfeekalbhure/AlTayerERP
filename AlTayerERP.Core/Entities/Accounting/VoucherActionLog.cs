using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities.Accounting
{
    /// <summary>
    /// سجل جميع الحركات التي تتم على السند المالي.
    /// </summary>
    [Table("voucher_action_logs")]
    public class VoucherActionLog
    {
        [Key]
        [Column("Voucher_Action_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Voucher_Action_ID { get; set; }

        [Required]
        [Column("Voucher_ID")]
        public long Voucher_ID { get; set; }

        [Required]
        [MaxLength(30)]
        [Column("Action_Type")]
        public string Action_Type { get; set; } = string.Empty;

        [Column("Old_Status_ID")]
        public int? Old_Status_ID { get; set; }

        [Column("New_Status_ID")]
        public int? New_Status_ID { get; set; }

        [MaxLength(50)]
        [Column("User_ID")]
        public string? User_ID { get; set; }

        [Required]
        [Column("Action_At")]
        public DateTime Action_At { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(20)]
        [Column("Action_Channel")]
        public string Action_Channel { get; set; } = "DESKTOP";

        [MaxLength(150)]
        [Column("Device_Name")]
        public string? Device_Name { get; set; }

        [MaxLength(50)]
        [Column("IP_Address")]
        public string? IP_Address { get; set; }

        [MaxLength(500)]
        [Column("Reason")]
        public string? Reason { get; set; }

        [MaxLength(500)]
        [Column("Notes")]
        public string? Notes { get; set; }

        [ForeignKey(nameof(Voucher_ID))]
        public virtual FinancialVoucherHeader Voucher { get; set; } = null!;
    }
}