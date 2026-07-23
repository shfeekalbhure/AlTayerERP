using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities
{
    /// <summary>
    /// فترة محاسبية داخل سنة مالية محددة وفرع محدد.
    /// لا يحفظ النظام عمليات مستقبلية إلا ضمن فترة فعالة وغير مقفلة.
    /// </summary>
    [Table("fiscal_periods")]
    public class FiscalPeriod
    {
        [Key]
        [Column("Fiscal_Period_ID")]
        public int Fiscal_Period_ID { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("Company_ID")]
        public string Company_ID { get; set; } = string.Empty;

        [Column("Branch_ID")]
        public int Branch_ID { get; set; }

        [Column("Fiscal_Year_ID")]
        public int Fiscal_Year_ID { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("Period_Code")]
        public string Period_Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [Column("Period_Name")]
        public string Period_Name { get; set; } = string.Empty;

        [Column("Start_Date")]
        public DateTime Start_Date { get; set; }

        [Column("End_Date")]
        public DateTime End_Date { get; set; }

        [Column("Is_Closed")]
        public bool Is_Closed { get; set; }

        [Column("Close_Date")]
        public DateTime? Close_Date { get; set; }

        [MaxLength(500)]
        [Column("Close_Reason")]
        public string? Close_Reason { get; set; }

        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;

        [Column("Created_At")]
        public DateTime Created_At { get; set; } = DateTime.Now;

        [Column("Updated_At")]
        public DateTime? Updated_At { get; set; }
    }
}
