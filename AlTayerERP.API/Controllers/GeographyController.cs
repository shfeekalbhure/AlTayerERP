using AlTayerERP.Core.Entities.Geography;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers;

[ApiController]
[Route("api/geography")]
public class GeographyController : ControllerBase
{
    private readonly AppDbContext _db;
    public GeographyController(AppDbContext db) => _db = db;

    [HttpGet("countries")]
    public async Task<IActionResult> GetCountries() =>
        Ok(await _db.Countries.AsNoTracking().OrderBy(x => x.Sort_Order).ThenBy(x => x.Country_Name_AR).ToListAsync());

    [HttpPost("countries")]
    public async Task<IActionResult> CreateCountry([FromBody] Country country)
    {
        country.Country_Code = country.Country_Code.Trim().ToUpperInvariant();
        country.Country_Name_AR = country.Country_Name_AR.Trim();
        if (string.IsNullOrWhiteSpace(country.Country_Code) || string.IsNullOrWhiteSpace(country.Country_Name_AR))
            return BadRequest(new { message = "رمز الدولة واسمها العربي مطلوبان." });
        if (await _db.Countries.AnyAsync(x => x.Country_Code == country.Country_Code || x.Country_Name_AR == country.Country_Name_AR))
            return Conflict(new { message = "رمز الدولة أو اسمها موجود مسبقاً." });
        country.Created_At = DateTime.UtcNow;
        _db.Countries.Add(country);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCountries), new { id = country.Country_ID }, country);
    }

    [HttpGet("governorates")]
    public async Task<IActionResult> GetGovernorates([FromQuery] long countryId) =>
        Ok(await _db.Governorates.AsNoTracking().Where(x => x.Country_ID == countryId && x.Is_Active).OrderBy(x => x.Sort_Order).ThenBy(x => x.Governorate_Name_AR).ToListAsync());

    [HttpGet("cities")]
    public async Task<IActionResult> GetCities([FromQuery] long governorateId) =>
        Ok(await _db.Cities.AsNoTracking().Where(x => x.Governorate_ID == governorateId && x.Is_Active).OrderBy(x => x.Sort_Order).ThenBy(x => x.City_Name_AR).ToListAsync());
}