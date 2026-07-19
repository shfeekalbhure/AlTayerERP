using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities
{
    [Table("numbering_settings")]
    public class NumberingSetting
    {
        [Key]
        [Column("Numbering_ID")]
        public int Numbering_ID { get; set; }

        [Column("Document_Type")]
        public string Document_Type { get; set; } = string.Empty;

        [Column("Prefix")]
        public string Prefix { get; set; } = string.Empty;

        [Column("Digits_Count")]
        public int Digits_Count { get; set; }

        [Column("Reset_Type")]
        public string Reset_Type { get; set; } = string.Empty;

        [Column("Last_Number")]
        public int Last_Number { get; set; }

        [Column("Use_Company")]
        public bool Use_Company { get; set; }

        [Column("Use_Branch")]
        public bool Use_Branch { get; set; }

        [Column("Use_Year")]
        public bool Use_Year { get; set; }

        [Column("Is_Active")]
        public bool Is_Active { get; set; }
    }
}