using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AlTayerERP.API.DTOs.Accounting
{
    /// <summary>
    /// بيانات إنشاء سند مالي جديد:
    /// سند قبض، سند صرف، أو أي سند مالي يستخدم المحرك الموحد.
    /// </summary>
    public class CreateFinancialVoucherDto
    {
        #region بيانات السند الأساسية

        [Required(ErrorMessage = "نوع السند مطلوب.")]
        public int Voucher_Type_ID { get; set; }

        [Required(ErrorMessage = "حالة السند مطلوبة.")]
        public int Voucher_Status_ID { get; set; }

        [Required(ErrorMessage = "الفرع مطلوب.")]
        [MaxLength(50)]
        public string Branch_ID { get; set; } = string.Empty;

        [Required(ErrorMessage = "السنة المالية مطلوبة.")]
        public int Fiscal_Year_ID { get; set; }

        [Required(ErrorMessage = "تاريخ المستند مطلوب.")]
        public DateTime Voucher_Date { get; set; }

        [Required(ErrorMessage = "تاريخ الحركة المحاسبية مطلوب.")]
        public DateTime Transaction_Date { get; set; }

        #endregion

        #region بيانات الصندوق أو البنك

        [Required(ErrorMessage = "حساب الصندوق أو البنك مطلوب.")]
        [MaxLength(50)]
        public string Cash_Account_ID { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Party_ID { get; set; }

        public int? Payment_Method_ID { get; set; }

        [Required(ErrorMessage = "العملة مطلوبة.")]
        public int Currency_ID { get; set; }

        [Range(0.000001, double.MaxValue, ErrorMessage = "سعر الصرف يجب أن يكون أكبر من صفر.")]
        public decimal Exchange_Rate { get; set; } = 1.000000m;

        [Range(0, double.MaxValue)]
        public decimal Foreign_Total { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Local_Total { get; set; }

        #endregion

        #region المرجع والبيانات النصية

        [MaxLength(100)]
        public string? Reference_No { get; set; }

        public DateTime? Reference_Date { get; set; }

        [MaxLength(500)]
        public string? Against_Text { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        #endregion

        #region المستند المصدر

        public int? Module_ID { get; set; }

        public int? Document_Type_ID { get; set; }

        public long? Document_ID { get; set; }

        [MaxLength(100)]
        public string? Source_Document_No { get; set; }

        #endregion

        #region الاعتماد

        public bool Requires_Approval { get; set; }

        #endregion

        #region بيانات المستخدم

        [Required(ErrorMessage = "معرف المستخدم المنشئ مطلوب.")]
        [MaxLength(50)]
        public string Created_By { get; set; } = string.Empty;

        /// <summary>
        /// معرف المستخدم الذي نفذ آخر تعديل.
        /// </summary>
        [MaxLength(50)]
        public string? Updated_By { get; set; }

        #endregion




        #region التفاصيل والتوزيعات

        [Required(ErrorMessage = "يجب إدخال تفاصيل السند.")]
        [MinLength(2, ErrorMessage = "يجب أن يحتوي السند على سطرين محاسبيين على الأقل.")]
        public List<CreateFinancialVoucherDetailDto> Details { get; set; }
            = new List<CreateFinancialVoucherDetailDto>();

        public List<CreateDocumentAllocationDto> Allocations { get; set; }
            = new List<CreateDocumentAllocationDto>();

        #endregion
    }

    /// <summary>
    /// سطر محاسبي داخل السند المالي.
    /// </summary>
    public class CreateFinancialVoucherDetailDto
    {
        [Required]
        public int Line_No { get; set; }

        [Required(ErrorMessage = "الحساب المالي مطلوب.")]
        [MaxLength(50)]
        public string Account_ID { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Cost_Center_ID { get; set; }

        [MaxLength(50)]
        public string? Project_ID { get; set; }

        #region بيانات المرجع

        /// <summary>
        /// نوع المرجع.
        /// يحدد نوع المستند المرتبط بهذا السطر المحاسبي.
        /// أمثلة:
        /// بوليصة، تذكرة، فاتورة، مطالبة، شيك، رحلة...
        /// </summary>
        [MaxLength(50)]
        public string? Reference_Type { get; set; }

        /// <summary>
        /// رقم المرجع.
        /// رقم المستند المرتبط بهذا السطر.
        /// مثال:
        /// رقم البوليصة أو رقم الفاتورة أو رقم التذكرة.
        /// </summary>
        [MaxLength(100)]
        public string? Reference_No { get; set; }

        /// <summary>
        /// اسم المرجع.
        /// الاسم أو الوصف الذي يظهر للمستخدم للمستند المرتبط.
        /// </summary>
        [MaxLength(250)]
        public string? Reference_Name { get; set; }

        /// <summary>
        /// تاريخ المرجع.
        /// تاريخ المستند المرتبط بهذا السطر.
        /// </summary>
        public DateTime? Reference_Date { get; set; }

        #endregion



        [Required]
        public int Currency_ID { get; set; }

        [Range(0.000001, double.MaxValue)]
        public decimal Exchange_Rate { get; set; } = 1.000000m;

        [Range(0, double.MaxValue)]
        public decimal Foreign_Amount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Local_Amount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Debit_Amount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Credit_Amount { get; set; }

        [Range(1, 7)]
        public byte Line_Type { get; set; } = 2;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// توزيع مبلغ السند على بوليصة أو تذكرة أو مستند آخر.
    /// </summary>
    public class CreateDocumentAllocationDto
    {
        [Required]
        public int Module_ID { get; set; }

        [Required]
        public int Document_Type_ID { get; set; }

        [Required]
        public long Document_ID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Document_No { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Party_ID { get; set; }

        [Required]
        public int Currency_ID { get; set; }

        [Range(0.000001, double.MaxValue)]
        public decimal Exchange_Rate { get; set; } = 1.000000m;

        [Range(0, double.MaxValue)]
        public decimal Document_Total { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Collected_Before { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "المبلغ المحصل الآن يجب أن يكون أكبر من صفر.")]
        public decimal Collected_Now { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Remaining_Balance { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}