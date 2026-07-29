using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities;

/// <summary>الدولة المرجعية المستخدمة في الهيكل المؤسسي.</summary>
[Table("countries")]
public sealed class Country
{
    [Key] public int Country_ID { get; set; }
    [Required, MaxLength(10)] public string Country_Code { get; set; } = string.Empty;
    [Required, MaxLength(150)] public string Country_Name_AR { get; set; } = string.Empty;
    [MaxLength(150)] public string? Country_Name_EN { get; set; }
    [MaxLength(2)] public string? ISO2 { get; set; }
    [MaxLength(3)] public string? ISO3 { get; set; }
    [MaxLength(10)] public string? Phone_Code { get; set; }
    [MaxLength(10)] public string? Currency_Code { get; set; }
    [MaxLength(150)] public string? Nationality_Name_AR { get; set; }
    public int Sort_Order { get; set; }
    public bool Is_Active { get; set; } = true;
    [MaxLength(500)] public string? Notes { get; set; }
    public DateTime Created_At { get; set; } = DateTime.UtcNow;
    public DateTime? Updated_At { get; set; }
    public ICollection<Governorate> Governorates { get; set; } = new List<Governorate>();
}

/// <summary>المحافظة التابعة لدولة.</summary>
[Table("governorates")]
public sealed class Governorate
{
    [Key] public int Governorate_ID { get; set; }
    public int Country_ID { get; set; }
    [Required, MaxLength(20)] public string Governorate_Code { get; set; } = string.Empty;
    [Required, MaxLength(150)] public string Governorate_Name_AR { get; set; } = string.Empty;
    [MaxLength(150)] public string? Governorate_Name_EN { get; set; }
    public int Sort_Order { get; set; }
    public bool Is_Active { get; set; } = true;
    [MaxLength(500)] public string? Notes { get; set; }
    public DateTime Created_At { get; set; } = DateTime.UtcNow;
    public DateTime? Updated_At { get; set; }
    public Country Country { get; set; } = null!;
    public ICollection<City> Cities { get; set; } = new List<City>();
}

/// <summary>المدينة التابعة لمحافظة ودولة.</summary>
[Table("cities")]
public sealed class City
{
    [Key] public int City_ID { get; set; }
    public int Country_ID { get; set; }
    public int Governorate_ID { get; set; }
    [Required, MaxLength(20)] public string City_Code { get; set; } = string.Empty;
    [Required, MaxLength(150)] public string City_Name_AR { get; set; } = string.Empty;
    [MaxLength(150)] public string? City_Name_EN { get; set; }
    [MaxLength(20)] public string? Postal_Code { get; set; }
    public int Sort_Order { get; set; }
    public bool Is_Active { get; set; } = true;
    [MaxLength(500)] public string? Notes { get; set; }
    public DateTime Created_At { get; set; } = DateTime.UtcNow;
    public DateTime? Updated_At { get; set; }
    public Country Country { get; set; } = null!;
    public Governorate Governorate { get; set; } = null!;
}

/// <summary>نوع الفرع المرجعي.</summary>
[Table("branch_types")]
public sealed class BranchType
{
    [Key] public int Branch_Type_ID { get; set; }
    [Required, MaxLength(30)] public string Branch_Type_Code { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Branch_Type_Name_AR { get; set; } = string.Empty;
    [MaxLength(100)] public string? Branch_Type_Name_EN { get; set; }
    public int Sort_Order { get; set; }
    public bool Is_Active { get; set; } = true;
    public DateTime Created_At { get; set; } = DateTime.UtcNow;
    public DateTime? Updated_At { get; set; }
}
