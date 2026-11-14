using System.ComponentModel.DataAnnotations;

namespace AlTayerERP.API.DTOs;

public sealed class TransportOfficeSettingsDto
{
    [StringLength(100)]
    public string? Country { get; set; }

    [StringLength(100)]
    public string? Governorate { get; set; }

    [StringLength(100)]
    public string? District { get; set; }

    [StringLength(100)]
    public string? Locality { get; set; }

    public decimal? Credit_Limit { get; set; }
    public bool Allow_Commission { get; set; }
    public OfficeCommissionDto? Passenger_Commission { get; set; }
    public OfficeCommissionDto? Outbound_Commission { get; set; }
    public OfficeCommissionDto? Inbound_Commission { get; set; }

    [StringLength(255)]
    public string? Analytical_Account_ID { get; set; }
}

public sealed record OfficeCommissionDto(string Method, decimal Value);
