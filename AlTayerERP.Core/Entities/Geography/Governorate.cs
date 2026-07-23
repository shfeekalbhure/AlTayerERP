using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities.Geography;

/// <summary>
/// محافظة أو ولاية تتبع دولة واحدة.
/// </summary>
[Table("governorates")]
public class Governorate
{
    [Key]
    [Column("Governorate_ID")]
    public long Governorate_ID { get; set; }

    [Required]
    [Column("Country_ID")]
    public long Country_ID { get; set; }

    [Required, MaxLength(20)]
    [Column("Governorate_Code")]
    public string Governorate_Code { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    [Column("Governorate_Name_AR")]
    public string Governorate_Name_AR { get; set; } = string.Empty;

    [MaxLength(150)]
    [Column("Governorate_Name_EN")]
    public string? Governorate_Name_EN { get; set; }

    [Column("Sort_Order")]
    public int Sort_Order { get; set; }

    [Column("Is_Active")]
    public bool Is_Active { get; set; } = true;

    [Column("Created_At")]
    public DateTime Created_At { get; set; } = DateTime.UtcNow;

    [Column("Updated_At")]
    public DateTime? Updated_At { get; set; }

    [ForeignKey(nameof(Country_ID))]
    public Country? Country { get; set; }

    public ICollection<City> Cities { get; set; } = new List<City>();
}