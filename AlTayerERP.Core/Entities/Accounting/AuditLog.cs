using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities.Accounting
{
    /// <summary>
    /// سجل تدقيق جميع العمليات التي تتم داخل النظام.
    /// </summary>
    [Table("audit_logs")]
    public class AuditLog
    {
        [Key]
        [Column("Audit_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Audit_ID { get; set; }

        [Column("Module_ID")]
        public int? Module_ID { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("Table_Name")]
        public string Table_Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("Record_ID")]
        public string Record_ID { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        [Column("Action_Type")]
        public string Action_Type { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("User_ID")]
        public string? User_ID { get; set; }

        [MaxLength(50)]
        [Column("Branch_ID")]
        public string? Branch_ID { get; set; }

        [Required]
        [Column("Action_At")]
        public DateTime Action_At { get; set; } = DateTime.Now;

        [Column("Old_Values", TypeName = "json")]
        public string? Old_Values { get; set; }

        [Column("New_Values", TypeName = "json")]
        public string? New_Values { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("Action_Channel")]
        public string Action_Channel { get; set; } = "DESKTOP";

        [MaxLength(150)]
        [Column("Device_Name")]
        public string? Device_Name { get; set; }

        [MaxLength(50)]
        [Column("IP_Address")]
        public string? IP_Address { get; set; }

        [MaxLength(500)]
        [Column("Notes")]
        public string? Notes { get; set; }
    }
}