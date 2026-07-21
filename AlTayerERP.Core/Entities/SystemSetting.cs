using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities
{
    /// <summary>
    /// إعداد قابل للضبط بأحد النطاقات: النظام أو الشركة أو الفرع أو السنة المالية.
    /// تُخزن مفاتيح النطاق غير المستخدمة بصفر/قيمة فارغة حتى يظل المفتاح المركب فريداً.
    /// </summary>
    [Table("system_settings")]
    public class SystemSetting
    {
        [Key]
        [Column("Setting_ID")]
        public int Setting_ID { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("Setting_Key")]
        public string Setting_Key { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        [Column("Setting_Name")]
        public string Setting_Name { get; set; } = string.Empty;

        [Column("Setting_Value")]
        public string Setting_Value { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Column("Scope")]
        public string Scope { get; set; } = "SYSTEM";

        [MaxLength(50)]
        [Column("Company_ID")]
        public string Company_ID { get; set; } = string.Empty;

        [Column("Branch_ID")]
        public int Branch_ID { get; set; }

        [Column("Fiscal_Year_ID")]
        public int Fiscal_Year_ID { get; set; }

        [Column("Effective_Date")]
        public DateTime? Effective_Date { get; set; }

        [Column("Description")]
        public string Description { get; set; } = string.Empty;

        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;

        [Column("Created_At")]
        public DateTime Created_At { get; set; } = DateTime.Now;

        [Column("Updated_At")]
        public DateTime? Updated_At { get; set; }
    }
}
