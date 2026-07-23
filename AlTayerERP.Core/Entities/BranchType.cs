using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities
{
    /// <summary>بيان مرجعي مستقل لأنواع الفروع. لا تُكتب الأنواع داخل الواجهة أو الكود.</summary>
    [Table("branch_types")]
    public class BranchType
    {
        [Key]
        [Column("Branch_Type_ID")]
        public int Branch_Type_ID { get; set; }

        [Required]
        [MaxLength(30)]
        [Column("Branch_Type_Code")]
        public string Branch_Type_Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [Column("Branch_Type_Name_AR")]
        public string Branch_Type_Name_AR { get; set; } = string.Empty;

        [MaxLength(150)]
        [Column("Branch_Type_Name_EN")]
        public string? Branch_Type_Name_EN { get; set; }

        [Column("Sort_Order")]
        public int Sort_Order { get; set; }

        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;

        [Column("Notes")]
        public string? Notes { get; set; }

        [Column("Created_At")]
        public DateTime Created_At { get; set; } = DateTime.UtcNow;

        [Column("Updated_At")]
        public DateTime? Updated_At { get; set; }
    }
}
