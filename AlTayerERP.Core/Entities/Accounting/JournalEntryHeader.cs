using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities.Accounting;

/// <summary>رأس القيد المحاسبي العام.</summary>
[Table("journal_entry_headers")]
public class JournalEntryHeader
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Journal_Entry_ID { get; set; }

    [Required, MaxLength(50)] public string Entry_No { get; set; } = string.Empty;
    public byte Entry_Type { get; set; } = 1;
    public int Entry_Status_ID { get; set; } = 1;

    /// <summary>معرف الفرع الداخلي الموحد من نوع INT.</summary>
    public int Branch_ID { get; set; }
    public int? Fiscal_Year_ID { get; set; }
    public DateTime Entry_Date { get; set; }
    public DateTime Transaction_Date { get; set; }

    [Required, MaxLength(30)] public string Source_System { get; set; } = "VOUCHER";
    public bool Is_System_Generated { get; set; } = true;
    public long? Source_Voucher_ID { get; set; }
    [MaxLength(50)] public string? Source_Document_Type { get; set; }
    [MaxLength(100)] public string? Source_Document_No { get; set; }
    [MaxLength(500)] public string? Description { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }

    [Column(TypeName = "decimal(18,2)")] public decimal Total_Debit { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Total_Credit { get; set; }

    public bool Is_Posted { get; set; }
    [MaxLength(50)] public string? Posted_By { get; set; }
    public DateTime? Posted_At { get; set; }

    public bool Is_Reversal { get; set; }
    public long? Original_Journal_Entry_ID { get; set; }
    public bool Is_Reversed { get; set; }
    public long? Reversal_Journal_Entry_ID { get; set; }
    [MaxLength(500)] public string? Reversal_Reason { get; set; }
    [MaxLength(50)] public string? Reversed_By { get; set; }
    public DateTime? Reversed_At { get; set; }

    public bool Is_Cancelled { get; set; }
    [MaxLength(500)] public string? Cancellation_Reason { get; set; }
    [MaxLength(50)] public string? Cancelled_By { get; set; }
    public DateTime? Cancelled_At { get; set; }

    public bool Is_Active { get; set; } = true;
    [MaxLength(50)] public string? Created_By { get; set; }
    public DateTime Created_At { get; set; } = DateTime.UtcNow;
    [MaxLength(50)] public string? Updated_By { get; set; }
    public DateTime? Updated_At { get; set; }

    public ICollection<JournalEntryDetail> Details { get; set; } = new List<JournalEntryDetail>();
}
