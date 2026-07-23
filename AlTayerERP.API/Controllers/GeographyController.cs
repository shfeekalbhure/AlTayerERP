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

    [HttpPut("countries/{id:long}")]
    public async Task<IActionResult> UpdateCountry(long id, [FromBody] Country input)
    {
        var country = await _db.Countries.FindAsync(id);
        if (country is null) return NotFound();
        var code = input.Country_Code?.Trim().ToUpperInvariant();
        var nameAr = input.Country_Name_AR?.Trim();
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(nameAr))
            return BadRequest(new { message = "رمز الدولة واسمها العربي مطلوبان." });
        if (await _db.Countries.AnyAsync(x => x.Country_ID != id && (x.Country_Code == code || x.Country_Name_AR == nameAr)))
            return Conflict(new { message = "رمز الدولة أو اسمها موجود مسبقاً." });
        country.Country_Code = code;
        country.Country_Name_AR = nameAr;
        country.Country_Name_EN = input.Country_Name_EN?.Trim();
        country.Country_Code2 = input.Country_Code2?.Trim().ToUpperInvariant();
        country.Phone_Code = input.Phone_Code?.Trim();
        country.Is_Active = input.Is_Active;
        country.Sort_Order = input.Sort_Order;
        country.Updated_At = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(country);
    }

    [HttpPut("governorates/{id:long}")]
    public async Task<IActionResult> UpdateGovernorate(long id, [FromBody] Governorate input)
    {
        var governorate = await _db.Governorates.FindAsync(id);
        if (governorate is null) return NotFound();
        var code = input.Governorate_Code?.Trim().ToUpperInvariant();
        var nameAr = input.Governorate_Name_AR?.Trim();
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(nameAr))
            return BadRequest(new { message = "رمز المحافظة واسمها العربي مطلوبان." });
        if (!await _db.Countries.AnyAsync(x => x.Country_ID == input.Country_ID && x.Is_Active))
            return BadRequest(new { message = "الدولة المختارة غير موجودة أو غير نشطة." });
        if (await _db.Governorates.AnyAsync(x => x.Governorate_ID != id && x.Country_ID == input.Country_ID && (x.Governorate_Code == code || x.Governorate_Name_AR == nameAr)))
            return Conflict(new { message = "رمز المحافظة أو اسمها موجود مسبقاً داخل الدولة." });
        governorate.Country_ID = input.Country_ID;
        governorate.Governorate_Code = code;
        governorate.Governorate_Name_AR = nameAr;
        governorate.Governorate_Name_EN = input.Governorate_Name_EN?.Trim();
        governorate.Is_Active = input.Is_Active;
        governorate.Sort_Order = input.Sort_Order;
        governorate.Updated_At = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(governorate);
    }

    [HttpPut("cities/{id:long}")]
    public async Task<IActionResult> UpdateCity(long id, [FromBody] City input)
    {
        var city = await _db.Cities.FindAsync(id);
        if (city is null) return NotFound();
        var code = input.City_Code?.Trim().ToUpperInvariant();
        var nameAr = input.City_Name_AR?.Trim();
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(nameAr))
            return BadRequest(new { message = "رمز المدينة واسمها العربي مطلوبان." });
        if (!await _db.Governorates.AnyAsync(x => x.Governorate_ID == input.Governorate_ID && x.Is_Active))
            return BadRequest(new { message = "المحافظة المختارة غير موجودة أو غير نشطة." });
        if (await _db.Cities.AnyAsync(x => x.City_ID != id && x.Governorate_ID == input.Governorate_ID && (x.City_Code == code || x.City_Name_AR == nameAr)))
            return Conflict(new { message = "رمز المدينة أو اسمها موجود مسبقاً داخل المحافظة." });
        if (input.Latitude is < -90 or > 90 || input.Longitude is < -180 or > 180)
            return BadRequest(new { message = "الإحداثيات الجغرافية خارج النطاق المسموح." });
        city.Governorate_ID = input.Governorate_ID;
        city.City_Code = code;
        city.City_Name_AR = nameAr;
        city.City_Name_EN = input.City_Name_EN?.Trim();
        city.Latitude = input.Latitude;
        city.Longitude = input.Longitude;
        city.Is_Active = input.Is_Active;
        city.Sort_Order = input.Sort_Order;
        city.Updated_At = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(city);
    }
}