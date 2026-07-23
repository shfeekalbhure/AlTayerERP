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

    [HttpPost("governorates")]
    public async Task<IActionResult> CreateGovernorate([FromBody] Governorate governorate)
    {
        governorate.Governorate_Code = governorate.Governorate_Code.Trim().ToUpperInvariant();
        governorate.Governorate_Name_AR = governorate.Governorate_Name_AR.Trim();
        if (governorate.Country_ID <= 0 || string.IsNullOrWhiteSpace(governorate.Governorate_Code) || string.IsNullOrWhiteSpace(governorate.Governorate_Name_AR))
            return BadRequest(new { message = "الدولة ورمز المحافظة واسمها العربي مطلوبة." });
        if (!await _db.Countries.AnyAsync(x => x.Country_ID == governorate.Country_ID && x.Is_Active))
            return BadRequest(new { message = "الدولة المختارة غير موجودة أو غير نشطة." });
        if (await _db.Governorates.AnyAsync(x => x.Country_ID == governorate.Country_ID && (x.Governorate_Code == governorate.Governorate_Code || x.Governorate_Name_AR == governorate.Governorate_Name_AR)))
            return Conflict(new { message = "رمز المحافظة أو اسمها موجود مسبقاً داخل الدولة." });
        governorate.Created_At = DateTime.UtcNow;
        _db.Governorates.Add(governorate);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetGovernorates), new { countryId = governorate.Country_ID }, governorate);
    }

    [HttpPost("cities")]
    public async Task<IActionResult> CreateCity([FromBody] City city)
    {
        city.City_Code = city.City_Code.Trim().ToUpperInvariant();
        city.City_Name_AR = city.City_Name_AR.Trim();
        if (city.Governorate_ID <= 0 || string.IsNullOrWhiteSpace(city.City_Code) || string.IsNullOrWhiteSpace(city.City_Name_AR))
            return BadRequest(new { message = "المحافظة ورمز المدينة واسمها العربي مطلوبة." });
        if (!await _db.Governorates.AnyAsync(x => x.Governorate_ID == city.Governorate_ID && x.Is_Active))
            return BadRequest(new { message = "المحافظة المختارة غير موجودة أو غير نشطة." });
        if (await _db.Cities.AnyAsync(x => x.Governorate_ID == city.Governorate_ID && (x.City_Code == city.City_Code || x.City_Name_AR == city.City_Name_AR)))
            return Conflict(new { message = "رمز المدينة أو اسمها موجود مسبقاً داخل المحافظة." });
        if (city.Latitude is < -90 or > 90 || city.Longitude is < -180 or > 180)
            return BadRequest(new { message = "الإحداثيات الجغرافية خارج النطاق المسموح." });
        city.Created_At = DateTime.UtcNow;
        _db.Cities.Add(city);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCities), new { governorateId = city.Governorate_ID }, city);
    }
}