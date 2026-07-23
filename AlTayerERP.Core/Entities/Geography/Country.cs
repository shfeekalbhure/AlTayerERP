using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities.Geography;

/// <summary>
/// الدولة المرجعية المشتركة بين الشركات والفروع والأطراف.
/// </summary>
[Table("countries")]
public class Country
{
    [Key]
    [Column("Country_ID")]
    public long Country_ID { get; set; }

    [Required, MaxLength(3)]
    [Column("Country_Code")]
    public string Country_Code { get; set; } = string.Empty;

    [MaxLength(2)]
    [Column("Country_Code2")]
    public string? Country_Code2 { get; set; }

    [Required, MaxLength(150)]
    [Column("Country_Name_AR")]
    public string Country_Name_AR { get; set; } = string.Empty;

    [MaxLength(150)]
    [Column("Country_Name_EN")]
    public string? Country_Name_EN { get; set; }

    [MaxLength(12)]
    [Column("Phone_Code")]
    public string? Phone_Code { get; set; }

    [Column("Sort_Order")]
    public int Sort_Order { get; set; }

    [Column("Is_Active")]
    public bool Is_Active { get; set; } = true;

    [Column("Created_At")]
    public DateTime Created_At { get; set; } = DateTime.UtcNow;

    [Column("Updated_At")]
    public DateTime? Updated_At { get; set; }

    public ICollection<Governorate> Governorates { get; set; } = new List<Governorate>();
}