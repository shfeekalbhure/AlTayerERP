using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities
{
    /// <summary>
    /// سجل تاريخي لأسعار الصرف حسب الشركة والعملـة وتاريخ السريان.
    /// </summary>
    [Table("exchange_rates")]
    public class ExchangeRate
    {
        [Key]
        [Column("Exchange_Rate_ID")]
        public int Exchange_Rate_ID { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("Company_ID")]
        public string Company_ID { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Column("Currency_Code")]
        public string Currency_Code { get; set; } = string.Empty;

        [Column("Rate_Date")]
        public DateTime Rate_Date { get; set; }

        [Column("Exchange_Rate")]
        public decimal Exchange_Rate_Value { get; set; }

        [Column("Min_Rate")]
        public decimal? Min_Rate { get; set; }

        [Column("Max_Rate")]
        public decimal? Max_Rate { get; set; }

        [Column("Is_Default")]
        public bool Is_Default { get; set; }

        [MaxLength(500)]
        [Column("Notes")]
        public string? Notes { get; set; }

        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;

        [Column("Created_At")]
        public DateTime Created_At { get; set; } = DateTime.Now;

        [Column("Updated_At")]
        public DateTime? Updated_At { get; set; }
    }
}
