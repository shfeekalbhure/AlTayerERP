using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities
{
    [Table("system_screens")]
    public class SystemScreen
    {
        [Key]
        [Column("Screen_ID")]
        public int Screen_ID { get; set; }

        [Column("Screen_Code")]
        public string Screen_Code { get; set; } = string.Empty;

        [Column("Screen_Name")]
        public string Screen_Name { get; set; } = string.Empty;

        [Column("Module_Name")]
        public string Module_Name { get; set; } = string.Empty;

        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;

        [Column("Sort_Order")]
        public int Sort_Order { get; set; }

        [Column("Created_At")]
        public DateTime Created_At { get; set; } = DateTime.Now;
    }
}