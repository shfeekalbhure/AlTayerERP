using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities
{
    [Table("chart_of_accounts")]
    public class ChartOfAccount
    {
        [Key]
        [Column("Account_ID")]
        public string Account_ID { get; set; } = Guid.NewGuid().ToString();

        [Column("Company_ID")]
        public string Company_ID { get; set; } = string.Empty;

        [Column("Parent_Account_ID")]
        public string? Parent_Account_ID { get; set; }

        [Column("Account_Code")]
        public string Account_Code { get; set; } = string.Empty;

        [Column("Account_Name_AR")]
        public string Account_Name_AR { get; set; } = string.Empty;

        [Column("Account_Name_EN")]
        public string? Account_Name_EN { get; set; }

        [Column("Account_Type")]
        public string Account_Type { get; set; } = string.Empty;

        [Column("Account_Level")]
        public int Account_Level { get; set; }

        [Column("Is_Postable")]
        public bool Is_Postable { get; set; }

        [Column("Currency_Code")]
        public string? Currency_Code { get; set; }

        [Column("Created_At")]
        public DateTime Created_At { get; set; } = DateTime.Now;

        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;

        [Column("Account_Category")]
        public string? Account_Category { get; set; }

        [Column("Normal_Balance")]
        public string? Normal_Balance { get; set; }

        [Column("Notes")]
        public string? Notes { get; set; }

        [Column("Allow_ManualEntry")]
        public bool Allow_ManualEntry { get; set; }

        [Column("System_Account")]
        public bool System_Account { get; set; }

        [Column("Requires_CostCenter")]
        public bool Requires_CostCenter { get; set; }

        [Column("Requires_Party")]
        public bool Requires_Party { get; set; }

        [Column("Requires_Project")]
        public bool Requires_Project { get; set; }

        [Column("Is_Summary_Account")]
        public bool Is_Summary_Account { get; set; }

        [Column("Affects_Balance_Sheet")]
        public bool Affects_Balance_Sheet { get; set; }

        [Column("Affects_Income_Statement")]
        public bool Affects_Income_Statement { get; set; }

        [Column("Multi_Currency")]
        public bool Multi_Currency { get; set; }

        /// <summary>حساب رقابي مرتبط بدفتر مساعد ولا يقبل القيود اليدوية المباشرة.</summary>
        [Column("Is_Control_Account")]
        public bool Is_Control_Account { get; set; }

        /// <summary>نوع الدفتر المساعد المرتبط بالحساب الرقابي.</summary>
        [Column("Control_Account_Type")]
        [StringLength(30)]
        public string? Control_Account_Type { get; set; }

        [Column("Account_Path")]
        public string? Account_Path { get; set; }

        [Column("Account_Serial")]
        public int Account_Serial { get; set; } = 0;

        [Column("Created_By")]
        public string? Created_By { get; set; }

        [Column("Updated_At")]
        public DateTime? Updated_At { get; set; }

        [Column("Updated_By")]
        public string? Updated_By { get; set; }
    }
}
