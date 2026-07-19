using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities
{
    public class SystemPermission
    {
        [Key]
        public int Permission_ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Permission_Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Permission_Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Permission_Type { get; set; } = string.Empty;
        // Data / Report / Extra

        [MaxLength(100)]
        public string? Module_Name { get; set; }

        public bool Is_Active { get; set; } = true;

        public int Sort_Order { get; set; } = 0;

        public DateTime Created_At { get; set; } = DateTime.Now;
    }
}