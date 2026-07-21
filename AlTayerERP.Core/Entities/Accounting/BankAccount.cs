using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities.Accounting
{
    /// <summary>
    /// حساب بنكي تشغيلي تابع للشركة الحالية.
    /// </summary>
    [Table("bank_accounts")]
    public sealed class BankAccount
    {
        [Key]
        [Column("Bank_Account_ID")]
        public int Bank_Account_ID { get; set; }

        [Required, MaxLength(50)]
        [Column("Company_ID")]
        public string Company_ID { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("Bank_Code")]
        public string Bank_Code { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("Bank_Name_AR")]
        public string Bank_Name_AR { get; set; } = string.Empty;

        [MaxLength(200)]
        [Column("Bank_Name_EN")]
        public string? Bank_Name_EN { get; set; }

        [Required, MaxLength(100)]
        [Column("Account_No")]
        public string Account_No { get; set; } = string.Empty;

        [MaxLength(64)]
        [Column("IBAN")]
        public string? IBAN { get; set; }

        [MaxLength(20)]
        [Column("Currency_Code")]
        public string Currency_Code { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("GL_Account")]
        public string? GL_Account { get; set; }

        [MaxLength(200)]
        [Column("Branch_Name")]
        public string? Branch_Name { get; set; }

        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;

        [Column("Notes")]
        public string? Notes { get; set; }

        [Column("Created_At")]
        public DateTime Created_At { get; set; } = DateTime.Now;

        [Column("Updated_At")]
        public DateTime? Updated_At { get; set; }
    }
}
