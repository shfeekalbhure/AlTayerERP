using System;
using System.ComponentModel.DataAnnotations;

namespace AlTayerERP.API.DTOs
{
    public class CreateAccountDto
    {
        [Required(ErrorMessage = "معرف الشركة مطلوب.")]
        [StringLength(50, ErrorMessage = "معرف الشركة لا يمكن أن يتجاوز 50 حرف.")]
        public string Company_ID { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Parent_Account_ID { get; set; }

        [StringLength(50)]
        public string Account_Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم الحساب باللغة العربية مطلوب.")]
        [StringLength(200, ErrorMessage = "اسم الحساب طويل جداً.")]
        public string Account_Name_AR { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "الاسم الإنجليزي طويل جداً.")]
        public string? Account_Name_EN { get; set; }

        [Required(ErrorMessage = "نوع الحساب مطلوب.")]
        [StringLength(50)]
        public string Account_Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "تصنيف الحساب مطلوب.")]
        [StringLength(50)]
        public string Account_Category { get; set; } = string.Empty;

        [StringLength(20)]
        public string Normal_Balance { get; set; } = string.Empty;

        [Range(1, 20, ErrorMessage = "مستوى الحساب يجب أن يكون بين 1 و20.")]
        public int Account_Level { get; set; }

        public bool Is_Postable { get; set; } = true;

        [StringLength(10)]
        public string? Currency_Code { get; set; } = "YER";

        public bool Is_Active { get; set; } = true;
        public bool Allow_ManualEntry { get; set; }
        public bool System_Account { get; set; }
        public bool Requires_Party { get; set; }
        public bool Requires_CostCenter { get; set; }
        public bool Requires_Project { get; set; }
        public bool Is_Summary_Account { get; set; }
        public bool Multi_Currency { get; set; }
        public bool Affects_Balance_Sheet { get; set; }
        public bool Affects_Income_Statement { get; set; }

        /// <summary>حساب رقابي مرتبط بدفتر مساعد، ولا تقبل عليه القيود اليدوية المباشرة.</summary>
        public bool Is_Control_Account { get; set; }

        /// <summary>نوع الدفتر المساعد: Customer أو Vendor أو Employee أو Other.</summary>
        [StringLength(30)]
        public string? Control_Account_Type { get; set; }

        public string? Account_Path { get; set; }
        public int? Account_Serial { get; set; }

        [StringLength(100)]
        public string? Created_By { get; set; }

        public DateTime? Updated_At { get; set; }

        [StringLength(100)]
        public string? Updated_By { get; set; }

        [StringLength(500, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز 500 حرف.")]
        public string? Notes { get; set; }
    }
}
