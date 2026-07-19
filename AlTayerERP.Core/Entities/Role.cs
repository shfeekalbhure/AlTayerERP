using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities
{
    [Table("roles")]
    public class Role
    {
        [Key]
        [Column("Role_ID")]
        public int Role_ID { get; set; }

        [Column("Role_Name")]
        public string Role_Name { get; set; } = string.Empty;

        [Column("Description")]
        public string? Description { get; set; }

        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;

        [Column("Created_At")]
        public DateTime Created_At { get; set; } = DateTime.Now;

        [Column("Updated_At")]
        public DateTime? Updated_At { get; set; }

        [Column("Role_Code")]
        public string? Role_Code { get; set; }
        public bool Is_System_Admin { get; set; } = false;
    }
}