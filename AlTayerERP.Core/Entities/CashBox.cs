using System;
using System.ComponentModel.DataAnnotations;

namespace AlTayerERP.Core.Entities
{
    public class CashBox
    {
        [Key]
        public string Cash_Box_ID { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string Company_ID { get; set; } = "";

        [Required]
  //      public string Branch_ID { get; set; } = "";
        public int Branch_ID { get; set; }
        [Required]
        public string Account_ID { get; set; } = "";

        [Required]
        public string Currency_Code { get; set; } = "";

        [Required]
        public string CashBox_Code { get; set; } = "";

        [Required]
        public string Box_Name_AR { get; set; } = "";

        public string? Box_Name_EN { get; set; }

        public decimal Opening_Balance { get; set; }

        public decimal Max_Limit { get; set; }

        public decimal Min_Limit { get; set; }

        public bool Is_Active { get; set; } = true;

        public string? Notes { get; set; }

        public DateTime Created_At { get; set; } = DateTime.Now;

        public DateTime? Updated_At { get; set; }

        public string? Created_By { get; set; }

        public string? Updated_By { get; set; }
        
    }
}