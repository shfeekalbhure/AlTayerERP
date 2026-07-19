using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities.Accounting
{
    /// <summary>
    /// توزيع السند المالي على المستندات مثل البوالص والتذاكر والفواتير.
    /// </summary>
    [Table("document_allocations")]
    public class DocumentAllocation
    {
        [Key]
        [Column("Allocation_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Allocation_ID { get; set; }

        [Required]
        [Column("Module_ID")]
        public int Module_ID { get; set; }

        [Required]
        [Column("Document_Type_ID")]
        public int Document_Type_ID { get; set; }

        [Required]
        [Column("Document_ID")]
        public long Document_ID { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("Document_No")]
        public string Document_No { get; set; } = string.Empty;

        [Required]
        [Column("Voucher_ID")]
        public long Voucher_ID { get; set; }

        [MaxLength(50)]
        [Column("Party_ID")]
        public string? Party_ID { get; set; }

        [Required]
        [Column("Currency_ID")]
        public int Currency_ID { get; set; }

        [Required]
        [Column("Exchange_Rate", TypeName = "decimal(18,6)")]
        public decimal Exchange_Rate { get; set; } = 1m;

        [Required]
        [Column("Document_Total", TypeName = "decimal(18,2)")]
        public decimal Document_Total { get; set; }

        [Required]
        [Column("Collected_Before", TypeName = "decimal(18,2)")]
        public decimal Collected_Before { get; set; }

        [Required]
        [Column("Collected_Now", TypeName = "decimal(18,2)")]
        public decimal Collected_Now { get; set; }

        [Required]
        [Column("Remaining_Balance", TypeName = "decimal(18,2)")]
        public decimal Remaining_Balance { get; set; }

        [Required]
        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;

        [MaxLength(500)]
        [Column("Notes")]
        public string? Notes { get; set; }

        [MaxLength(50)]
        [Column("Created_By")]
        public string? Created_By { get; set; }

        [Required]
        [Column("Created_At")]
        public DateTime Created_At { get; set; } = DateTime.Now;

        [MaxLength(50)]
        [Column("Updated_By")]
        public string? Updated_By { get; set; }

        [Column("Updated_At")]
        public DateTime? Updated_At { get; set; }

        [ForeignKey(nameof(Voucher_ID))]
        public virtual FinancialVoucherHeader Voucher { get; set; } = null!;
    }
}