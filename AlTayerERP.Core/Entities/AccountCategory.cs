using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities
{
    /// <summary>
    /// تصنيف تفصيلي للحساب مرتبط بنوع الحساب الأساسي.
    /// </summary>
    [Table("account_categories")]
    public sealed class AccountCategory
    {
        [Key]
        [Column("Category_ID")]
        [StringLength(50)]
        public string Category_ID { get; set; } = Guid.NewGuid().ToString();

        [Column("Company_ID")]
        [StringLength(50)]
        public string Company_ID { get; set; } = string.Empty;

        [Column("Category_Code")]
        [StringLength(50)]
        public string Category_Code { get; set; } = string.Empty;

        [Column("Category_Name_AR")]
        [StringLength(150)]
        public string Category_Name_AR { get; set; } = string.Empty;

        [Column("Category_Name_EN")]
        [StringLength(150)]
        public string? Category_Name_EN { get; set; }

        [Column("Account_Type")]
        [StringLength(30)]
        public string Account_Type { get; set; } = string.Empty;

        [Column("Normal_Balance")]
        [StringLength(10)]
        public string Normal_Balance { get; set; } = string.Empty;

        [Column("Is_System")]
        public bool Is_System { get; set; }

        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;

        [Column("Sort_Order")]
        public int Sort_Order { get; set; }

        [Column("Created_At")]
        public DateTime Created_At { get; set; } = DateTime.UtcNow;

        [Column("Created_By")]
        [StringLength(100)]
        public string? Created_By { get; set; }

        [Column("Updated_At")]
        public DateTime? Updated_At { get; set; }

        [Column("Updated_By")]
        [StringLength(100)]
        public string? Updated_By { get; set; }
    }
}
