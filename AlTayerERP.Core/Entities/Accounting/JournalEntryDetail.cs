using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities.Accounting
{
    /// <summary>
    /// تفاصيل القيد المحاسبي العام.
    /// يمثل كل حساب مدين أو دائن داخل القيد.
    /// </summary>
    [Table("journal_entry_details")]
    public class JournalEntryDetail
    {
        #region هوية السطر

        [Key]
        [Column("Journal_Entry_Detail_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Journal_Entry_Detail_ID { get; set; }

        [Required]
        [Column("Journal_Entry_ID")]
        public long Journal_Entry_ID { get; set; }

        [Required]
        [Column("Line_No")]
        public int Line_No { get; set; }

        #endregion

        #region الحساب والتوجيه

        [Required]
        [MaxLength(50)]
        [Column("Account_ID")]
        public string Account_ID { get; set; } = string.Empty;

        [MaxLength(500)]
        [Column("Description")]
        public string? Description { get; set; }

        [MaxLength(50)]
        [Column("Cost_Center_ID")]
        public string? Cost_Center_ID { get; set; }

        [MaxLength(50)]
        [Column("Project_ID")]
        public string? Project_ID { get; set; }

        #endregion

        #region العملة

        [Required]
        [Column("Currency_ID")]
        public int Currency_ID { get; set; }

        [Required]
        [Column("Exchange_Rate", TypeName = "decimal(18,6)")]
        public decimal Exchange_Rate { get; set; } = 1.000000m;

        [Required]
        [Column("Foreign_Amount", TypeName = "decimal(18,2)")]
        public decimal Foreign_Amount { get; set; }

        [Required]
        [Column("Local_Amount", TypeName = "decimal(18,2)")]
        public decimal Local_Amount { get; set; }

        #endregion

        #region المدين والدائن

        [Required]
        [Column("Debit_Amount", TypeName = "decimal(18,2)")]
        public decimal Debit_Amount { get; set; }

        [Required]
        [Column("Credit_Amount", TypeName = "decimal(18,2)")]
        public decimal Credit_Amount { get; set; }

        #endregion

        #region بيانات المرجع

        [MaxLength(50)]
        [Column("Reference_Type")]
        public string? Reference_Type { get; set; }

        [MaxLength(100)]
        [Column("Reference_No")]
        public string? Reference_No { get; set; }

        [MaxLength(250)]
        [Column("Reference_Name")]
        public string? Reference_Name { get; set; }

        [Column("Reference_Date")]
        public DateTime? Reference_Date { get; set; }

        #endregion

        #region بيانات المصدر

        /// <summary>
        /// معرف سطر السند المالي الذي أنشأ هذا السطر.
        /// </summary>
        [Column("Source_Voucher_Detail_ID")]
        public long? Source_Voucher_Detail_ID { get; set; }

        /// <summary>
        /// نوع السطر:
        /// 1 صندوق أو بنك، 2 حساب مقابل، 3 ضريبة،
        /// 4 خصم، 5 فرق عملة، 6 تلقائي، 7 تسوية.
        /// </summary>
        [Required]
        [Column("Line_Type")]
        public byte Line_Type { get; set; } = 2;

        #endregion

        #region بيانات النظام

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

        #endregion

        #region العلاقات

        [ForeignKey(nameof(Journal_Entry_ID))]
        public virtual JournalEntryHeader JournalEntry { get; set; }
            = null!;

        #endregion
    }
}