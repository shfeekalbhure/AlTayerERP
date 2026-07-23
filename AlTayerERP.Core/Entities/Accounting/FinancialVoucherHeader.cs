using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities.Accounting
{
    /// <summary>
    /// رأس السند المالي.
    /// يستخدم لجميع السندات المالية:
    /// سند قبض - سند صرف - قيد يومية - إشعار مدين - إشعار دائن - قيود التسوية...
    /// </summary>
    [Table("financial_voucher_headers")]
    public class FinancialVoucherHeader
    {
        #region أولاً: هوية السند
        [Key]
        [Column("Voucher_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Voucher_ID { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("Voucher_No")]
        public string Voucher_No { get; set; } = string.Empty;

        [Required]
        [Column("Voucher_Type_ID")]
        public int Voucher_Type_ID { get; set; }

        [Required]
        [Column("Voucher_Status_ID")]
        public int Voucher_Status_ID { get; set; }
        #endregion

        #region ثانياً: بيانات السند
        [Required]
        [MaxLength(50)]
        [Column("Branch_ID")]
        public string Branch_ID { get; set; } = string.Empty;

        [Column("Fiscal_Year_ID")]
        public int? Fiscal_Year_ID { get; set; }

        [Required]
        [Column("Voucher_Date")]
        public DateTime Voucher_Date { get; set; }

        [Required]
        [Column("Transaction_Date")]
        public DateTime Transaction_Date { get; set; }
        #endregion

        #region ثالثاً: بيانات القبض أو الصرف
        [Required]
        [MaxLength(50)]
        [Column("Cash_Account_ID")]
        public string Cash_Account_ID { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("Party_ID")]
        public string? Party_ID { get; set; }

        /// <summary>
        /// اسم الشخص الذي تم استلام المبلغ منه كما ظهر وقت إنشاء السند.
        /// يحتفظ بالاسم حتى عند عدم اختيار طرف مسجل أو تغيير اسمه لاحقًا.
        /// </summary>
        [MaxLength(200)]
        [Column("Received_From_Name")]
        public string? Received_From_Name { get; set; }

        [Column("Payment_Method_ID")]
        public int? Payment_Method_ID { get; set; }

        [Required]
        [Column("Currency_ID")]
        public int Currency_ID { get; set; }

        [Required]
        [Column("Exchange_Rate", TypeName = "decimal(18,6)")]
        public decimal Exchange_Rate { get; set; } = 1.000000m;

        /// <summary>
        /// المبلغ الأصلي الذي أدخله المستخدم بعملة السند.
        /// </summary>
        [Required]
        [Column("Amount", TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } = 0.00m;

        [Required]
        [Column("Foreign_Total", TypeName = "decimal(18,2)")]
        public decimal Foreign_Total { get; set; } = 0.00m;

        [Required]
        [Column("Local_Total", TypeName = "decimal(18,2)")]
        public decimal Local_Total { get; set; } = 0.00m;

        [MaxLength(100)]
        [Column("Reference_No")]
        public string? Reference_No { get; set; }

        [Column("Reference_Date")]
        public DateTime? Reference_Date { get; set; }

        [MaxLength(500)]
        [Column("Against_Text")]
        public string? Against_Text { get; set; }

        [MaxLength(500)]
        [Column("Description")]
        public string? Description { get; set; }

        [MaxLength(1000)]
        [Column("Notes")]
        public string? Notes { get; set; }
        #endregion

        #region رابعاً: ربط المستند التشغيلي
        [Column("Module_ID")]
        public int? Module_ID { get; set; }

        [Column("Document_Type_ID")]
        public int? Document_Type_ID { get; set; }

        [Column("Document_ID")]
        public long? Document_ID { get; set; }




        [MaxLength(100)]
        [Column("Source_Document_No")]
        public string? Source_Document_No { get; set; }
        #endregion

        #region خامساً: بيانات الاعتماد
        [Required]
        [Column("Requires_Approval")]
        public bool Requires_Approval { get; set; }

        [Required]
        [Column("Approval_Status")]
        public byte Approval_Status { get; set; }

        [MaxLength(50)]
        [Column("Approval_Requested_By_User_ID")]
        public string? Approval_Requested_By_User_ID { get; set; }

        [Column("Approval_Requested_At")]
        public DateTime? Approval_Requested_At { get; set; }

        [MaxLength(50)]
        [Column("Approved_By_User_ID")]
        public string? Approved_By_User_ID { get; set; }

        [Column("Approved_At")]
        public DateTime? Approved_At { get; set; }

        [MaxLength(50)]
        [Column("Rejected_By_User_ID")]
        public string? Rejected_By_User_ID { get; set; }

        [Column("Rejected_At")]
        public DateTime? Rejected_At { get; set; }

        [MaxLength(500)]
        [Column("Rejection_Reason")]
        public string? Rejection_Reason { get; set; }

        /// <summary>
        /// حالة المراجعة الرقابية: 0 غير مراجع، 1 قيد المراجعة،
        /// 2 تمت المراجعة، 3 معاد للتصحيح.
        /// </summary>
        [Required]
        [Column("Review_Status")]
        public byte Review_Status { get; set; } = 0;

        [MaxLength(50)]
        [Column("Reviewed_By_User_ID")]
        public string? Reviewed_By_User_ID { get; set; }

        [Column("Reviewed_At")]
        public DateTime? Reviewed_At { get; set; }

        [MaxLength(500)]
        [Column("Review_Notes")]
        public string? Review_Notes { get; set; }
        #endregion

        #region سادساً: بيانات الرقابة والتعديل والطباعة والتراجع
        /// <summary>
        /// عدد التعديلات - Edit Count
        /// يسجل عدد مرات تعديل السند بعد إنشائه.
        /// </summary>
        [Required]
        [Column("Edit_Count")]
        public int Edit_Count { get; set; } = 0;

        /// <summary>
        /// عدد مرات الطباعة - Print Count
        /// يسجل عدد مرات طباعة السند.
        /// </summary>
        [Required]
        [Column("Print_Count")]
        public int Print_Count { get; set; } = 0;

        /// <summary>
        /// آخر مستخدم قام بطباعة السند - Last Printed By
        /// يحتوي على معرف المستخدم الذي نفذ آخر عملية طباعة.
        /// </summary>
        [MaxLength(50)]
        [Column("Last_Printed_By")]
        public string? Last_Printed_By { get; set; }

        /// <summary>
        /// تاريخ آخر طباعة - Last Print Date
        /// يحتوي على تاريخ ووقت آخر عملية طباعة للسند.
        /// </summary>
        [Column("Last_Print_Date")]
        public DateTime? Last_Print_Date { get; set; }

        /// <summary>
        /// عدد مرات التراجع - Undo Count
        /// يسجل عدد مرات تنفيذ عملية التراجع على السند.
        /// </summary>
        [Required]
        [Column("Undo_Count")]
        public int Undo_Count { get; set; } = 0;

        /// <summary>
        /// آخر مستخدم نفذ التراجع - Last Undo By
        /// يحتوي على معرف المستخدم الذي نفذ آخر عملية تراجع.
        /// </summary>
        [MaxLength(50)]
        [Column("Last_Undo_By")]
        public string? Last_Undo_By { get; set; }

        /// <summary>
        /// تاريخ آخر تراجع - Last Undo At
        /// يحتوي على تاريخ ووقت آخر عملية تراجع.
        /// </summary>
        [Column("Last_Undo_At")]
        public DateTime? Last_Undo_At { get; set; }
        #endregion

        #region سابعاً: بيانات الترحيل وإلغاء الترحيل
        /// <summary>
        /// هل السند مرحل - Is Posted
        /// يحدد هل تم إنشاء القيد المحاسبي للسند.
        /// </summary>
        [Required]
        [Column("Is_Posted")]
        public bool Is_Posted { get; set; } = false;

        /// <summary>
        /// معرف القيد المحاسبي - Journal Entry ID
        /// يربط السند بالقيد الناتج عن عملية الترحيل.
        /// </summary>
        [Column("Journal_Entry_ID")]
        public long? Journal_Entry_ID { get; set; }

        /// <summary>
        /// المستخدم الذي قام بترحيل السند - Posted By User ID
        /// يحتوي على معرف المستخدم الذي نفذ عملية الترحيل المحاسبي.
        /// </summary>
        [MaxLength(50)]
        [Column("Posted_By_User_ID")]
        public string? Posted_By_User_ID { get; set; }

        /// <summary>
        /// تاريخ ترحيل السند - Posted At
        /// يحتوي على تاريخ ووقت ترحيل السند إلى القيود المحاسبية.
        /// </summary>
        [Column("Posted_At")]
        public DateTime? Posted_At { get; set; }

        /// <summary>
        /// المستخدم الذي ألغى ترحيل السند - Unposted By
        /// يحتوي على معرف المستخدم الذي نفذ إلغاء الترحيل.
        /// </summary>
        [MaxLength(50)]
        [Column("Unposted_By")]
        public string? Unposted_By { get; set; }

        /// <summary>
        /// تاريخ إلغاء الترحيل - Unposted At
        /// يحتوي على تاريخ ووقت آخر عملية إلغاء ترحيل.
        /// </summary>
        [Column("Unposted_At")]
        public DateTime? Unposted_At { get; set; }

        /// <summary>
        /// سبب إلغاء الترحيل - Unpost Reason
        /// يوضح سبب إعادة السند من الحالة المرحلة إلى غير المرحل.
        /// </summary>
        [MaxLength(500)]
        [Column("Unpost_Reason")]
        public string? Unpost_Reason { get; set; }

        /// <summary>هل تم عكس السند محاسبياً بقيد عكسي؟</summary>
        [Column("Is_Reversed")]
        public bool Is_Reversed { get; set; }

        /// <summary>معرف القيد العكسي الناتج؛ يستخدم للربط والطباعة والمراجعة.</summary>
        [Column("Reversal_Journal_Entry_ID")]
        public long? Reversal_Journal_Entry_ID { get; set; }

        /// <summary>معرف المستخدم الذي نفذ عملية العكس.</summary>
        [MaxLength(50)]
        [Column("Reversed_By")]
        public string? Reversed_By { get; set; }

        /// <summary>وقت تنفيذ العكس بتوقيت الخادم.</summary>
        [Column("Reversed_At")]
        public DateTime? Reversed_At { get; set; }

        /// <summary>سبب العكس الإلزامي للحفظ في سجل الرقابة والطباعة.</summary>
        [MaxLength(500)]
        [Column("Reversal_Reason")]
        public string? Reversal_Reason { get; set; }
        #endregion

        #region ثامناً: بيانات النظام
        [Required]
        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;

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

        #region العلاقات (Navigation Properties)
        #region تاسعاً: العلاقات

        /// <summary>
        /// تفاصيل السند المالي - Voucher Details
        /// تمثل جميع القيود المحاسبية التابعة لرأس السند.
        /// </summary>
        public virtual ICollection<FinancialVoucherDetail> Details { get; set; }
            = new List<FinancialVoucherDetail>();

        /// <summary>
        /// توزيعات المستندات - Document Allocations
        /// تمثل توزيع مبلغ السند على البوالص أو التذاكر
        /// أو الفواتير والمستندات التشغيلية الأخرى.
        /// </summary>
        public virtual ICollection<DocumentAllocation> DocumentAllocations { get; set; }
            = new List<DocumentAllocation>();

        /// <summary>
        /// سجل حركات السند - Voucher Action Logs
        /// يسجل عمليات الإنشاء والتعديل والطباعة
        /// والترحيل وإلغاء الترحيل والتراجع والحذف.
        /// </summary>
        public virtual ICollection<VoucherActionLog> VoucherActionLogs { get; set; }
            = new List<VoucherActionLog>();

        #endregion
        #endregion
    }
}
