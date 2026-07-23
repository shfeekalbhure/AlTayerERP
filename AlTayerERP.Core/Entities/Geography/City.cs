using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities.Geography;

/// <summary>
/// مدينة تتبع محافظة واحدة وتستخدم في عناوين الفروع والأطراف.
/// </summary>
[Table("cities")]
public class City
{
    [Key]
    [Column("City_ID")]
    public long City_ID { get; set; }

    [Required]
    [Column("Governorate_ID")]
    public long Governorate_ID { get; set; }

    [Required, MaxLength(20)]
    [Column("City_Code")]
    public string City_Code { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    [Column("City_Name_AR")]
    public string City_Name_AR { get; set; } = string.Empty;

    [MaxLength(150)]
    [Column("City_Name_EN")]
    public string? City_Name_EN { get; set; }

    [MaxLength(20)]
    [Column("Postal_Code")]
    public string? Postal_Code { get; set; }

    [Column("Latitude", TypeName = "decimal(10,7)")]
    public decimal? Latitude { get; set; }

    [Column("Longitude", TypeName = "decimal(10,7)")]
    public decimal? Longitude { get; set; }

    [Column("Sort_Order")]
    public int Sort_Order { get; set; }

    [Column("Is_Active")]
    public bool Is_Active { get; set; } = true;

    [Column("Created_At")]
    public DateTime Created_At { get; set; } = DateTime.UtcNow;

    [Column("Updated_At")]
    public DateTime? Updated_At { get; set; }

    [ForeignKey(nameof(Governorate_ID))]
    public Governorate? Governorate { get; set; }
}