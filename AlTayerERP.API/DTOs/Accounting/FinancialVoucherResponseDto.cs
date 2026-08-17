using System;
using System.Collections.Generic;

namespace AlTayerERP.API.DTOs.Accounting
{
    /// <summary>
    /// DTO لإرجاع بيانات السند المالي إلى شاشة سند القبض أو الصرف.
    /// </summary>
    public class FinancialVoucherResponseDto
    {
        #region بيانات السند

        public long Voucher_ID { get; set; }

        public string Voucher_No { get; set; } = string.Empty;

        public int Voucher_Type_ID { get; set; }

        public string Voucher_Type_Name { get; set; } = string.Empty;

        public int Voucher_Status_ID { get; set; }

        public string Voucher_Status_Name { get; set; } = string.Empty;

        public int Branch_ID { get; set; }

        public int? Fiscal_Year_ID { get; set; }

        public DateTime Voucher_Date { get; set; }

        public DateTime Transaction_Date { get; set; }

        #endregion

        #region بيانات القبض أو الصرف

        public string Cash_Account_ID { get; set; } = string.Empty;

        public string Cash_Account_Name { get; set; } = string.Empty;

        public string? Party_ID { get; set; }

        public string? Party_Name { get; set; }

        public string? Received_From_Name { get; set; }

        public int? Payment_Method_ID { get; set; }

        public string? Payment_Method_Name { get; set; }

        public int Currency_ID { get; set; }

        public string Currency_Name { get; set; } = string.Empty;

        public decimal Exchange_Rate { get; set; }

        /// <summary>
        /// المبلغ الأصلي المدخل بعملة السند.
        /// </summary>
        public decimal Amount { get; set; }

        public decimal Foreign_Total { get; set; }

        public decimal Local_Total { get; set; }

        #endregion

        #region بيانات إضافية

        public string? Reference_No { get; set; }

        public DateTime? Reference_Date { get; set; }

        public string? Against_Text { get; set; }

        public string? Description { get; set; }

        public string? Notes { get; set; }

        #endregion
        /// <summary>
        ///   - Edit Count
        /// الاعتماد والترحيل.
        /// </summary>
        #region الاعتماد والترحيل

        public bool Requires_Approval { get; set; }

        public byte Approval_Status { get; set; }

        /// <summary>
        /// 0 غير مراجع، 1 قيد المراجعة، 2 تمت المراجعة، 3 معاد للتصحيح.
        /// </summary>
        public byte Review_Status { get; set; }

        public string? Reviewed_By_User_ID { get; set; }

        public DateTime? Reviewed_At { get; set; }

        public string? Review_Notes { get; set; }

        public bool Is_Posted { get; set; }

        public long? Journal_Entry_ID { get; set; }

        public string? Journal_Entry_No { get; set; }

        #endregion


        #region بيانات الرقابة والتعديل والطباعة والتراجع

        /// <summary>
        /// عدد التعديلات - Edit Count
        /// عدد مرات تعديل السند بعد إنشائه.
        /// </summary>
        public int Edit_Count { get; set; }

        /// <summary>
        /// عدد مرات الطباعة - Print Count
        /// عدد المرات التي تمت فيها طباعة السند.
        /// </summary>
        public int Print_Count { get; set; }

        /// <summary>
        /// آخر مستخدم قام بالطباعة - Last Printed By
        /// معرف المستخدم الذي نفذ آخر عملية طباعة.
        /// </summary>
        public string? Last_Printed_By { get; set; }

        /// <summary>
        /// تاريخ آخر طباعة - Last Print Date
        /// تاريخ ووقت آخر عملية طباعة للسند.
        /// </summary>
        public DateTime? Last_Print_Date { get; set; }

        /// <summary>
        /// عدد مرات التراجع - Undo Count
        /// عدد مرات تنفيذ التراجع على السند.
        /// </summary>
        public int Undo_Count { get; set; }

        /// <summary>
        /// آخر مستخدم نفذ التراجع - Last Undo By
        /// معرف المستخدم الذي نفذ آخر عملية تراجع.
        /// </summary>
        public string? Last_Undo_By { get; set; }

        /// <summary>
        /// تاريخ آخر تراجع - Last Undo At
        /// تاريخ ووقت آخر عملية تراجع.
        /// </summary>
        public DateTime? Last_Undo_At { get; set; }

        #endregion
        #region بيانات النظام

        public string? Created_By { get; set; }

        public DateTime Created_At { get; set; }

        public string? Updated_By { get; set; }

        public DateTime? Updated_At { get; set; }

        #endregion

        #region التفاصيل

        public List<FinancialVoucherDetailResponseDto> Details { get; set; }
            = new();

        public List<DocumentAllocationResponseDto> Allocations { get; set; }
            = new();

        #endregion
    }

    public class FinancialVoucherDetailResponseDto
    {
        public long Voucher_Detail_ID { get; set; }

        public int Line_No { get; set; }

        public string Account_ID { get; set; } = string.Empty;

        public string Account_Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Cost_Center_ID { get; set; }

        public string? Cost_Center_Name { get; set; }

        public string? Project_ID { get; set; }

        public string? Project_Name { get; set; }

        #region بيانات المرجع

        /// <summary>
        /// نوع المرجع.
        /// </summary>
        public string? Reference_Type { get; set; }

        /// <summary>
        /// رقم المرجع.
        /// </summary>
        public string? Reference_No { get; set; }

        /// <summary>
        /// اسم المرجع.
        /// </summary>
        public string? Reference_Name { get; set; }

        /// <summary>
        /// تاريخ المرجع.
        /// </summary>
        public DateTime? Reference_Date { get; set; }

        #endregion

        public int Currency_ID { get; set; }

        public string Currency_Name { get; set; } = string.Empty;

        public decimal Exchange_Rate { get; set; }

        public decimal Foreign_Amount { get; set; }

        public decimal Local_Amount { get; set; }

        public decimal Debit_Amount { get; set; }

        public decimal Credit_Amount { get; set; }

        public byte Line_Type { get; set; }

        public string? Notes { get; set; }
    }

    public class DocumentAllocationResponseDto
    {
        public long Allocation_ID { get; set; }

        public int Module_ID { get; set; }

        public string Module_Name { get; set; } = string.Empty;

        public int Document_Type_ID { get; set; }

        public string Document_Type_Name { get; set; } = string.Empty;

        public long Document_ID { get; set; }

        public string Document_No { get; set; } = string.Empty;

        public decimal Document_Total { get; set; }

        public decimal Collected_Before { get; set; }

        public decimal Collected_Now { get; set; }

        public decimal Remaining_Balance { get; set; }
    }
}
