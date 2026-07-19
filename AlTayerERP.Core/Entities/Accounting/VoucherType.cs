using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities.Accounting
{
    [Table("voucher_types")]
    public class VoucherType
    {
        [Key]
        [Column("Voucher_Type_ID")]
        public int Voucher_Type_ID { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("Voucher_Type_Code")]
        public string Voucher_Type_Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [Column("Voucher_Type_Name_AR")]
        public string Voucher_Type_Name_AR { get; set; } = string.Empty;

        [MaxLength(150)]
        [Column("Voucher_Type_Name_EN")]
        public string? Voucher_Type_Name_EN { get; set; }

        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;

        [Column("Sort_Order")]
        public int Sort_Order { get; set; }
    }
}