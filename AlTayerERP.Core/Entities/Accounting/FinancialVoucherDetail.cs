using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities.Accounting
{
    /// <summary>
    /// تفاصيل السند المالي.
    /// تمثل السطور المحاسبية داخل السند المالي.
    /// </summary>
    [Table("financial_voucher_details")]
    public class FinancialVoucherDetail
    {
        #region هوية السطر

        [Key]
        [Column("Voucher_Detail_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Voucher_Detail_ID { get; set; }

        [Required]
        [Column("Voucher_ID")]
        public long Voucher_ID { get; set; }

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

        #region بيانات المرجع

        /// <summary>
        /// نوع المرجع (Reference Type).
        /// يحدد نوع المستند المرتبط بهذا السطر.
        /// أمثلة:
        /// بوليصة، تذكرة، فاتورة، مطالبة، شيك، سند، رحلة...
        /// </summary>
        [MaxLength(50)]
        [Column("Reference_Type")]
        public string? Reference_Type { get; set; }

        /// <summary>
        /// رقم المرجع (Reference Number).
        /// رقم المستند المرتبط بهذا السطر.
        /// مثال:
        /// رقم البوليصة أو رقم التذكرة أو رقم الفاتورة.
        /// </summary>
        [MaxLength(100)]
        [Column("Reference_No")]
        public string? Reference_No { get; set; }

        /// <summary>
        /// اسم المرجع (Reference Name).
        /// الاسم التوضيحي للمستند المرتبط بالسطر.
        /// مثال:
        /// اسم العميل أو اسم الرحلة أو وصف البوليصة.
        /// </summary>
        [MaxLength(250)]
        [Column("Reference_Name")]
        public string? Reference_Name { get; set; }

        /// <summary>
        /// تاريخ المرجع (Reference Date).
        /// تاريخ إصدار المستند المرتبط بهذا السطر.
        /// </summary>
        [Column("Reference_Date")]
        public DateTime? Reference_Date { get; set; }

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
        public decimal Foreign_Amount { get; set; } = 0.00m;

        [Required]
        [Column("Local_Amount", TypeName = "decimal(18,2)")]
        public decimal Local_Amount { get; set; } = 0.00m;

        #endregion

        #region المدين والدائن

        [Required]
        [Column("Debit_Amount", TypeName = "decimal(18,2)")]
        public decimal Debit_Amount { get; set; } = 0.00m;

        [Required]
        [Column("Credit_Amount", TypeName = "decimal(18,2)")]
        public decimal Credit_Amount { get; set; } = 0.00m;

        /// <summary>
        /// 1 صندوق أو بنك، 2 حساب مقابل، 3 ضريبة، 4 خصم،
        /// 5 فرق عملة، 6 تلقائي، 7 تسوية.
        /// </summary>
        [Required]
        [Column("Line_Type")]
        public byte Line_Type { get; set; } = 2;

        #endregion

        #region الملاحظات وبيانات النظام

        [MaxLength(500)]
        [Column("Notes")]
        public string? Notes { get; set; }

        // يسمح بالقيمة الفارغة طبقًا لجدول MySQL الحالي.
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

        [ForeignKey(nameof(Voucher_ID))]
        public virtual FinancialVoucherHeader Voucher { get; set; } = null!;

        #endregion
    }
}