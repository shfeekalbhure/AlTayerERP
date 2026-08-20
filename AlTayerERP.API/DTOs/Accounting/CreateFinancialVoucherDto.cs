using System.ComponentModel.DataAnnotations;

namespace AlTayerERP.API.DTOs.Accounting;

public class CreateFinancialVoucherDto
{
    private string? _againstText;
    private string? _description;

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

    [MaxLength(50)]
    public string Cash_Account_ID { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Party_ID { get; set; }

    [MaxLength(200)]
    public string? Received_From_Name { get; set; }

    public int? Payment_Method_ID { get; set; }

    [Required(ErrorMessage = "العملة مطلوبة.")]
    public int Currency_ID { get; set; }

    [Range(0.000001, double.MaxValue, ErrorMessage = "سعر الصرف يجب أن يكون أكبر من صفر.")]
    public decimal Exchange_Rate { get; set; } = 1.000000m;

    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Foreign_Total { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Local_Total { get; set; }

    [MaxLength(100)]
    public string? Reference_No { get; set; }

    public DateTime? Reference_Date { get; set; }

    [MaxLength(500)]
    public string? Against_Text
    {
        get => _againstText;
        set => _againstText = value;
    }

    [MaxLength(500)]
    public string? Description
    {
        get => _description;
        set
        {
            _description = value;
            if (string.IsNullOrWhiteSpace(_againstText))
                _againstText = value;
        }
    }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public int? Module_ID { get; set; }
    public int? Document_Type_ID { get; set; }
    public long? Document_ID { get; set; }

    [MaxLength(100)]
    public string? Source_Document_No { get; set; }

    public bool Requires_Approval { get; set; }

    [MaxLength(50)]
    public string Created_By { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Updated_By { get; set; }

    [Required(ErrorMessage = "يجب إدخال تفاصيل السند.")]
    [MinLength(2, ErrorMessage = "يجب أن يحتوي السند على سطرين محاسبيين على الأقل.")]
    public List<CreateFinancialVoucherDetailDto> Details { get; set; } = [];

    public List<CreateDocumentAllocationDto> Allocations { get; set; } = [];
}

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

    [MaxLength(50)]
    public string? Reference_Type { get; set; }

    [MaxLength(100)]
    public string? Reference_No { get; set; }

    [MaxLength(250)]
    public string? Reference_Name { get; set; }

    public DateTime? Reference_Date { get; set; }

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
