using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities.Accounting
{
    /// <summary>
    /// يربط أي مستند في النظام بمستند آخر.
    /// </summary>
    [Table("document_links")]
    public class DocumentLink
    {
        [Key]
        [Column("Document_Link_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Document_Link_ID { get; set; }

        [Required]
        [Column("From_Module_ID")]
        public int From_Module_ID { get; set; }

        [Required]
        [Column("From_Document_Type_ID")]
        public int From_Document_Type_ID { get; set; }

        [Required]
        [Column("From_Document_ID")]
        public long From_Document_ID { get; set; }

        [MaxLength(100)]
        [Column("From_Document_No")]
        public string? From_Document_No { get; set; }

        [Required]
        [Column("To_Module_ID")]
        public int To_Module_ID { get; set; }

        [Required]
        [Column("To_Document_Type_ID")]
        public int To_Document_Type_ID { get; set; }

        [Required]
        [Column("To_Document_ID")]
        public long To_Document_ID { get; set; }

        [MaxLength(100)]
        [Column("To_Document_No")]
        public string? To_Document_No { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("Link_Type")]
        public string Link_Type { get; set; } = "RELATED";

        [Required]
        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;

        [MaxLength(500)]
        [Column("Notes")]
        public string? Notes { get; set; }

        [MaxLength(50)]
        [Column("Created_By")]
        public string? Created_By { get; set; }

        [Required]
        [Column("Created_At")]
        public DateTime Created_At { get; set; } = DateTime.Now;

        [MaxLength(50)]
        [Column("Updated_By")]
        public string? Updated_By { get; set; }

        [Column("Updated_At")]
        public DateTime? Updated_At { get; set; }
    }
}