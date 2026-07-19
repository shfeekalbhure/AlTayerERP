using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities.Accounting
{
    /// <summary>
    /// رأس القيد المحاسبي العام.
    ///
    /// يمثل القيد المحاسبي الذي يؤثر على دفتر الأستاذ العام،
    /// سواء تم إنشاؤه تلقائيًا من سند مالي أو يدويًا من شاشة القيود.
    ///
    /// أمثلة مصادر القيد:
    /// - سند قبض.
    /// - سند صرف.
    /// - قيد يومية.
    /// - إشعار مدين.
    /// - إشعار دائن.
    /// - قيد افتتاحي.
    /// - قيد إقفال.
    /// - قيد عكسي.
    /// </summary>
    [Table("journal_entry_headers")]
    public class JournalEntryHeader
    {
        #region 1. هوية القيد المحاسبي

        /// <summary>
        /// المعرف الداخلي للقيد المحاسبي.
        /// يتم توليده تلقائيًا بواسطة قاعدة البيانات.
        /// </summary>
        [Key]
        [Column("Journal_Entry_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Journal_Entry_ID { get; set; }

        /// <summary>
        /// الرقم الرسمي للقيد المحاسبي.
        /// مثال: JE-2026-000001
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column("Entry_No")]
        public string Entry_No { get; set; } = string.Empty;

        /// <summary>
        /// نوع القيد المحاسبي.
        ///
        /// 1 = قيد تلقائي.
        /// 2 = قيد يدوي.
        /// 3 = قيد عكسي.
        /// 4 = قيد افتتاحي.
        /// 5 = قيد إقفال.
        /// 6 = قيد تسوية.
        /// </summary>
        [Required]
        [Column("Entry_Type")]
        public byte Entry_Type { get; set; } = 1;

        /// <summary>
        /// حالة القيد المحاسبي.
        ///
        /// 1 = مسودة.
        /// 2 = مرحل.
        /// 3 = ملغي.
        /// 4 = معكوس.
        /// </summary>
        [Required]
        [Column("Entry_Status_ID")]
        public int Entry_Status_ID { get; set; } = 1;

        #endregion

        #region 2. بيانات الفرع والفترة المالية

        /// <summary>
        /// معرف الفرع الذي يتبع له القيد.
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column("Branch_ID")]
        public string Branch_ID { get; set; } = string.Empty;

        /// <summary>
        /// معرف السنة المالية المرتبط بها القيد.
        /// </summary>
        [Column("Fiscal_Year_ID")]
        public int? Fiscal_Year_ID { get; set; }

        /// <summary>
        /// تاريخ القيد المحاسبي.
        /// </summary>
        [Required]
        [Column("Entry_Date")]
        public DateTime Entry_Date { get; set; }

        /// <summary>
        /// التاريخ الفعلي للحركة المالية.
        /// قد يختلف عن تاريخ إنشاء القيد.
        /// </summary>
        [Required]
        [Column("Transaction_Date")]
        public DateTime Transaction_Date { get; set; }

        #endregion

        #region 3. مصدر القيد

        /// <summary>
        /// النظام أو الوحدة التي أنشأت القيد.
        ///
        /// أمثلة:
        /// VOUCHER
        /// WAYBILL
        /// TICKET
        /// WAREHOUSE
        /// MANUAL
        /// SYSTEM
        /// </summary>
        [Required]
        [MaxLength(30)]
        [Column("Source_System")]
        public string Source_System { get; set; } = "VOUCHER";

        /// <summary>
        /// هل تم إنشاء القيد تلقائيًا بواسطة النظام؟
        /// </summary>
        [Required]
        [Column("Is_System_Generated")]
        public bool Is_System_Generated { get; set; } = true;

        /// <summary>
        /// معرف السند المالي الذي نتج عنه القيد.
        /// يكون فارغًا في القيود اليدوية.
        /// </summary>
        [Column("Source_Voucher_ID")]
        public long? Source_Voucher_ID { get; set; }

        /// <summary>
        /// نوع المستند المصدر.
        ///
        /// أمثلة:
        /// RECEIPT_VOUCHER
        /// PAYMENT_VOUCHER
        /// JOURNAL_VOUCHER
        /// DEBIT_NOTE
        /// CREDIT_NOTE
        /// </summary>
        [MaxLength(50)]
        [Column("Source_Document_Type")]
        public string? Source_Document_Type { get; set; }

        /// <summary>
        /// رقم المستند المصدر.
        /// </summary>
        [MaxLength(100)]
        [Column("Source_Document_No")]
        public string? Source_Document_No { get; set; }

        /// <summary>
        /// البيان العام للقيد المحاسبي.
        /// </summary>
        [MaxLength(500)]
        [Column("Description")]
        public string? Description { get; set; }

        /// <summary>
        /// ملاحظات إضافية على القيد.
        /// </summary>
        [MaxLength(1000)]
        [Column("Notes")]
        public string? Notes { get; set; }

        #endregion

        #region 4. إجماليات القيد

        /// <summary>
        /// إجمالي الطرف المدين بالعملة المحلية.
        /// </summary>
        [Required]
        [Column("Total_Debit", TypeName = "decimal(18,2)")]
        public decimal Total_Debit { get; set; } = 0.00m;

        /// <summary>
        /// إجمالي الطرف الدائن بالعملة المحلية.
        /// </summary>
        [Required]
        [Column("Total_Credit", TypeName = "decimal(18,2)")]
        public decimal Total_Credit { get; set; } = 0.00m;

        #endregion

        #region 5. بيانات الترحيل

        /// <summary>
        /// يحدد هل تم ترحيل القيد إلى دفتر الأستاذ العام.
        /// </summary>
        [Required]
        [Column("Is_Posted")]
        public bool Is_Posted { get; set; } = false;

        /// <summary>
        /// معرف المستخدم الذي قام بترحيل القيد.
        /// </summary>
        [MaxLength(50)]
        [Column("Posted_By")]
        public string? Posted_By { get; set; }

        /// <summary>
        /// تاريخ ووقت ترحيل القيد.
        /// </summary>
        [Column("Posted_At")]
        public DateTime? Posted_At { get; set; }

        #endregion

        #region 6. بيانات العكس المحاسبي

        /// <summary>
        /// هل هذا القيد نفسه قيد عكسي؟
        /// </summary>
        [Required]
        [Column("Is_Reversal")]
        public bool Is_Reversal { get; set; } = false;

        /// <summary>
        /// إذا كان هذا القيد عكسيًا،
        /// يحتوي على معرف القيد الأصلي الذي تم عكسه.
        /// </summary>
        [Column("Original_Journal_Entry_ID")]
        public long? Original_Journal_Entry_ID { get; set; }

        /// <summary>
        /// هل تم عكس هذا القيد بواسطة قيد عكسي؟
        /// </summary>
        [Required]
        [Column("Is_Reversed")]
        public bool Is_Reversed { get; set; } = false;

        /// <summary>
        /// معرف القيد العكسي الذي تم إنشاؤه لعكس هذا القيد.
        /// </summary>
        [Column("Reversal_Journal_Entry_ID")]
        public long? Reversal_Journal_Entry_ID { get; set; }

        /// <summary>
        /// سبب إنشاء القيد العكسي.
        /// </summary>
        [MaxLength(500)]
        [Column("Reversal_Reason")]
        public string? Reversal_Reason { get; set; }

        /// <summary>
        /// معرف المستخدم الذي نفذ عملية العكس.
        /// </summary>
        [MaxLength(50)]
        [Column("Reversed_By")]
        public string? Reversed_By { get; set; }

        /// <summary>
        /// تاريخ ووقت تنفيذ عملية العكس.
        /// </summary>
        [Column("Reversed_At")]
        public DateTime? Reversed_At { get; set; }

        #endregion

        #region 7. بيانات الإلغاء

        /// <summary>
        /// هل تم إلغاء القيد؟
        /// </summary>
        [Required]
        [Column("Is_Cancelled")]
        public bool Is_Cancelled { get; set; } = false;

        /// <summary>
        /// سبب إلغاء القيد.
        /// </summary>
        [MaxLength(500)]
        [Column("Cancellation_Reason")]
        public string? Cancellation_Reason { get; set; }

        /// <summary>
        /// المستخدم الذي قام بإلغاء القيد.
        /// </summary>
        [MaxLength(50)]
        [Column("Cancelled_By")]
        public string? Cancelled_By { get; set; }

        /// <summary>
        /// تاريخ ووقت إلغاء القيد.
        /// </summary>
        [Column("Cancelled_At")]
        public DateTime? Cancelled_At { get; set; }

        #endregion

        #region 8. بيانات النظام والتدقيق

        /// <summary>
        /// هل السجل فعال داخل النظام؟
        /// </summary>
        [Required]
        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;

        /// <summary>
        /// المستخدم الذي أنشأ القيد.
        /// </summary>
        [MaxLength(50)]
        [Column("Created_By")]
        public string? Created_By { get; set; }

        /// <summary>
        /// تاريخ ووقت إنشاء القيد.
        /// </summary>
        [Required]
        [Column("Created_At")]
        public DateTime Created_At { get; set; } = DateTime.Now;

        /// <summary>
        /// آخر مستخدم قام بتعديل القيد.
        /// </summary>
        [MaxLength(50)]
        [Column("Updated_By")]
        public string? Updated_By { get; set; }

        /// <summary>
        /// تاريخ ووقت آخر تعديل.
        /// </summary>
        [Column("Updated_At")]
        public DateTime? Updated_At { get; set; }

        #endregion

        #region 9. العلاقات

        /// <summary>
        /// جميع تفاصيل القيد المحاسبي.
        /// </summary>
        public virtual ICollection<JournalEntryDetail> Details { get; set; }
            = new List<JournalEntryDetail>();

        #endregion
    }
}