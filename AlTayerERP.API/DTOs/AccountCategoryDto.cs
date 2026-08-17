using System.ComponentModel.DataAnnotations;

namespace AlTayerERP.API.DTOs
{
    public sealed class AccountCategoryDto
    {
        [Required(ErrorMessage = "كود التصنيف مطلوب.")]
        [StringLength(50)]
        public string Category_Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم التصنيف بالعربية مطلوب.")]
        [StringLength(150)]
        public string Category_Name_AR { get; set; } = string.Empty;

        [StringLength(150)]
        public string? Category_Name_EN { get; set; }

        [Required(ErrorMessage = "نوع الحساب مطلوب.")]
        [StringLength(30)]
        public string Account_Type { get; set; } = string.Empty;

        public bool Is_Active { get; set; } = true;

        [Range(0, 9999)]
        public int Sort_Order { get; set; }
    }
}
