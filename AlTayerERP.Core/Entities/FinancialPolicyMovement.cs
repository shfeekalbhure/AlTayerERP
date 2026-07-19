using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities
{
    // ======================================================
    // هذا الكلاس يمثل حركة تمت على سياسة رقابة مالية
    // مثال:
    // صرف مبلغ، قبض مبلغ، استخدام سقف مديونية، تجاوز سقف
    // ======================================================
    [Table("financial_limit_movements")]
    public class FinancialPolicyMovement
    {
        // رقم الحركة
        [Key]
        [Column("Movement_ID")]
        public int Movement_ID { get; set; }

        // رقم السياسة المرتبطة بهذه الحركة
        [Column("Limit_ID")]
        public int Limit_ID { get; set; }

        // الشركة
        [Column("Company_ID")]
        public string Company_ID { get; set; } = string.Empty;

        // تاريخ الحركة
        [Column("Movement_Date")]
        public DateTime Movement_Date { get; set; } = DateTime.Now;

        // نوع الحركة: صرف، قبض، مديونية، اعتماد، تجاوز
        [Column("Movement_Type")]
        public string Movement_Type { get; set; } = string.Empty;

        // نوع المستند المرتبط بالحركة
        [Column("Reference_Type")]
        public string? Reference_Type { get; set; }

        // رقم المستند المرتبط بالحركة
        [Column("Reference_ID")]
        public string? Reference_ID { get; set; }

        // العملة
        [Column("Currency_Code")]
        public string Currency_Code { get; set; } = string.Empty;

        // مبلغ الحركة
        [Column("Amount")]
        public decimal Amount { get; set; }

        // الرصيد بعد الحركة
        [Column("Balance_After")]
        public decimal Balance_After { get; set; }

        // ملاحظات
        [Column("Notes")]
        public string? Notes { get; set; }

        // المستخدم الذي أنشأ الحركة
        [Column("Created_By")]
        public string? Created_By { get; set; }

        // تاريخ الإنشاء
        [Column("Created_At")]
        public DateTime Created_At { get; set; } = DateTime.Now;
    }
}