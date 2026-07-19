using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AlTayerERP.API.DTOs.Accounting
{
    /// <summary>
    /// بيانات تعديل سند مالي موجود.
    /// لا يسمح من خلاله بتعديل رقم السند أو بيانات الترحيل والاعتماد مباشرة.
    /// </summary>
    public class UpdateFinancialVoucherDto
    {
        #region بيانات السند الأساسية

        [Required(ErrorMessage = "معرف السند مطلوب.")]
        [Range(1, long.MaxValue, ErrorMessage = "معرف السند غير صحيح.")]
        public long Voucher_ID { get; set; }

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

        [Range(
            0.000001,
            double.MaxValue,
            ErrorMessage = "سعر الصرف يجب أن يكون أكبر من صفر."
        )]
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

        #region بيانات المستخدم المعدل

        [Required(ErrorMessage = "معرف المستخدم المعدل مطلوب.")]
        [MaxLength(50)]
        public string Updated_By { get; set; } = string.Empty;

        #endregion

        #region التفاصيل والتوزيعات

        [Required(ErrorMessage = "يجب إدخال تفاصيل السند.")]
        [MinLength(
            2,
            ErrorMessage = "يجب أن يحتوي السند على سطرين محاسبيين على الأقل."
        )]
        public List<UpdateFinancialVoucherDetailDto> Details { get; set; }
            = new();

        public List<UpdateDocumentAllocationDto> Allocations { get; set; }
            = new();

        #endregion
    }

    /// <summary>
    /// بيانات تعديل سطر محاسبي داخل السند.
    /// إذا كان Voucher_Detail_ID = 0 يعتبر السطر جديدًا.
    /// </summary>
    public class UpdateFinancialVoucherDetailDto
    {
        public long Voucher_Detail_ID { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
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

        [MaxLength(50)]
        public string? Reference_Type { get; set; }

        [MaxLength(100)]
        public string? Reference_No { get; set; }

        [MaxLength(250)]
        public string? Reference_Name { get; set; }

        public DateTime? Reference_Date { get; set; }

        #endregion
        [Required(ErrorMessage = "العملة مطلوبة.")]
        public int Currency_ID { get; set; }

        [Range(
            0.000001,
            double.MaxValue,
            ErrorMessage = "سعر الصرف يجب أن يكون أكبر من صفر."
        )]
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
    /// بيانات تعديل توزيع السند على مستند تشغيلي.
    /// إذا كان Allocation_ID = 0 يعتبر التوزيع جديدًا.
    /// </summary>
    public class UpdateDocumentAllocationDto
    {
        public long Allocation_ID { get; set; }

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

        [Range(
            0.000001,
            double.MaxValue,
            ErrorMessage = "سعر الصرف يجب أن يكون أكبر من صفر."
        )]
        public decimal Exchange_Rate { get; set; } = 1.000000m;

        [Range(0, double.MaxValue)]
        public decimal Document_Total { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Collected_Before { get; set; }

        [Range(
            0.01,
            double.MaxValue,
            ErrorMessage = "المبلغ المحصل الآن يجب أن يكون أكبر من صفر."
        )]
        public decimal Collected_Now { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Remaining_Balance { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}