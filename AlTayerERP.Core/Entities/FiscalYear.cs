using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities
{
    [Table("fiscal_years")]
    public class FiscalYear
    {
        [Key]
        [Column("Fiscal_Year_ID")]
        public int Fiscal_Year_ID { get; set; }

        [Column("Company_ID")]
        public string Company_ID { get; set; } = string.Empty;

        [Column("Year_Name")]
        public string Year_Name { get; set; } = string.Empty;

        [Column("Start_Date")]
        public DateTime Start_Date { get; set; }

        [Column("End_Date")]
        public DateTime End_Date { get; set; }

        [Column("Is_Default")]
        public bool Is_Default { get; set; } = false;

        [Column("Is_Closed")]
        public bool Is_Closed { get; set; } = false;

        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;

        [Column("Created_At")]
        public DateTime Created_At { get; set; } = DateTime.Now;

        [Column("Updated_At")]
        public DateTime? Updated_At { get; set; }
    }
}