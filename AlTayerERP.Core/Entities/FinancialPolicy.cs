using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities
{
    // ======================================================
    // هذا الكلاس يمثل سياسة رقابة مالية داخل النظام
    // مثال:
    // سقف صرف، سقف قبض، سقف مديونية، سقف خصم
    // ======================================================
    [Table("financial_limits")]
    public class FinancialPolicy
    {
        // رقم السياسة
        [Key]
        [Column("Limit_ID")]
        public int Limit_ID { get; set; }

        // الشركة التي تتبع لها السياسة
        [Column("Company_ID")]
        public string Company_ID { get; set; } = string.Empty;

        // نوع الجهة: عميل، مورد، فرع، حساب، صندوق، بنك
        [Column("Entity_Type")]
        public string Entity_Type { get; set; } = string.Empty;

        // رقم الجهة
        [Column("Entity_ID")]
        public string Entity_ID { get; set; } = string.Empty;

        // نوع السياسة: صرف، قبض، مديونية، خصم
        [Column("Limit_Type")]
        public string Limit_Type { get; set; } = string.Empty;

        // العملة
        [Column("Currency_Code")]
        public string Currency_Code { get; set; } = string.Empty;

        // قيمة السقف
        [Column("Limit_Amount")]
        public decimal Limit_Amount { get; set; }

        // المبلغ المستخدم من السقف
        [Column("Used_Amount")]
        public decimal Used_Amount { get; set; }

        // فترة السقف: يومي، شهري، سنوي
        [Column("Period_Type")]
        public string Period_Type { get; set; } = "Monthly";

        // هل يحتاج اعتماد عند التجاوز
        [Column("Requires_Approval")]
        public bool Requires_Approval { get; set; } = true;

        // هل السياسة فعالة
        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;

        // تاريخ الإنشاء
        [Column("Created_At")]
        public DateTime Created_At { get; set; } = DateTime.Now;

        // تاريخ آخر تعديل
        [Column("Updated_At")]
        public DateTime? Updated_At { get; set; }
    }
}