using AlTayerERP.API.Services;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace AlTayerERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class GeographicReferencesController : ControllerBase
{
    private readonly AppDbContext _context;

    public GeographicReferencesController(AppDbContext context) => _context = context;

    private IActionResult? RequireSystemAdmin()
    {
        var session = HttpContext.Items["ServerSession"] as ServerSession;
        if (session == null) return Unauthorized("انتهت الجلسة أو أنها غير صالحة.");
        return session.Is_System_Admin ? null : Forbid();
    }

    [HttpGet("countries")]
    public async Task<IActionResult> GetCountries([FromQuery] bool activeOnly = false)
    {
        var access = RequireSystemAdmin();
        if (access != null) return access;
        return Ok(await QueryAsync($"SELECT Country_ID, Country_Code, Country_Name_AR, Country_Name_EN, ISO2, ISO3, Phone_Code, Currency_Code, Nationality_Name_AR, Sort_Order, Is_Active, Notes FROM countries {(activeOnly ? "WHERE Is_Active = 1" : string.Empty)} ORDER BY Sort_Order, Country_Name_AR"));
    }

    [HttpGet("governorates")]
    public async Task<IActionResult> GetGovernorates([FromQuery] int? countryId = null, [FromQuery] bool activeOnly = false)
    {
        var access = RequireSystemAdmin();
        if (access != null) return access;
        var where = new List<string>();
        if (countryId.HasValue) where.Add("g.Country_ID = @Country_ID");
        if (activeOnly) where.Add("g.Is_Active = 1");
        var sql = "SELECT g.Governorate_ID, g.Country_ID, c.Country_Name_AR, g.Governorate_Code, g.Governorate_Name_AR, g.Governorate_Name_EN, g.Sort_Order, g.Is_Active, g.Notes FROM governorates g INNER JOIN countries c ON c.Country_ID = g.Country_ID" +
                  (where.Count > 0 ? " WHERE " + string.Join(" AND ", where) : string.Empty) +
                  " ORDER BY c.Sort_Order, c.Country_Name_AR, g.Sort_Order, g.Governorate_Name_AR";
        return Ok(await QueryAsync(sql, ("@Country_ID", countryId)));
    }

    [HttpGet("cities")]
    public async Task<IActionResult> GetCities([FromQuery] int? governorateId = null, [FromQuery] int? countryId = null, [FromQuery] bool activeOnly = false)
    {
        var access = RequireSystemAdmin();
        if (access != null) return access;
        var where = new List<string>();
        if (countryId.HasValue) where.Add("ci.Country_ID = @Country_ID");
        if (governorateId.HasValue) where.Add("ci.Governorate_ID = @Governorate_ID");
        if (activeOnly) where.Add("ci.Is_Active = 1");
        var sql = "SELECT ci.City_ID, ci.Country_ID, co.Country_Name_AR, ci.Governorate_ID, g.Governorate_Name_AR, ci.City_Code, ci.City_Name_AR, ci.City_Name_EN, ci.Postal_Code, ci.Sort_Order, ci.Is_Active, ci.Notes FROM cities ci INNER JOIN countries co ON co.Country_ID = ci.Country_ID INNER JOIN governorates g ON g.Governorate_ID = ci.Governorate_ID" +
                  (where.Count > 0 ? " WHERE " + string.Join(" AND ", where) : string.Empty) +
                  " ORDER BY co.Sort_Order, g.Sort_Order, ci.Sort_Order, ci.City_Name_AR";
        return Ok(await QueryAsync(sql, ("@Country_ID", countryId), ("@Governorate_ID", governorateId)));
    }

    [HttpPost("countries")]
    public async Task<IActionResult> SaveCountry([FromBody] CountryRequest dto)
    {
        var access = RequireSystemAdmin();
        if (access != null) return access;
        if (string.IsNullOrWhiteSpace(dto.Country_Code) || string.IsNullOrWhiteSpace(dto.Country_Name_AR))
            return BadRequest("كود الدولة واسمها العربي مطلوبان.");

        if (dto.Country_ID > 0)
        {
            await ExecuteAsync("UPDATE countries SET Country_Code=@Code, Country_Name_AR=@NameAR, Country_Name_EN=@NameEN, ISO2=@ISO2, ISO3=@ISO3, Phone_Code=@Phone, Currency_Code=@Currency, Nationality_Name_AR=@Nationality, Sort_Order=@Sort, Is_Active=@Active, Notes=@Notes, Updated_At=UTC_TIMESTAMP() WHERE Country_ID=@ID",
                ("@Code", dto.Country_Code.Trim().ToUpperInvariant()), ("@NameAR", dto.Country_Name_AR.Trim()), ("@NameEN", dto.Country_Name_EN), ("@ISO2", dto.ISO2), ("@ISO3", dto.ISO3), ("@Phone", dto.Phone_Code), ("@Currency", dto.Currency_Code), ("@Nationality", dto.Nationality_Name_AR), ("@Sort", dto.Sort_Order), ("@Active", dto.Is_Active), ("@Notes", dto.Notes), ("@ID", dto.Country_ID));
            return Ok(new { message = "تم تعديل الدولة." });
        }

        await ExecuteAsync("INSERT INTO countries (Country_Code, Country_Name_AR, Country_Name_EN, ISO2, ISO3, Phone_Code, Currency_Code, Nationality_Name_AR, Sort_Order, Is_Active, Notes) VALUES (@Code,@NameAR,@NameEN,@ISO2,@ISO3,@Phone,@Currency,@Nationality,@Sort,@Active,@Notes)",
            ("@Code", dto.Country_Code.Trim().ToUpperInvariant()), ("@NameAR", dto.Country_Name_AR.Trim()), ("@NameEN", dto.Country_Name_EN), ("@ISO2", dto.ISO2), ("@ISO3", dto.ISO3), ("@Phone", dto.Phone_Code), ("@Currency", dto.Currency_Code), ("@Nationality", dto.Nationality_Name_AR), ("@Sort", dto.Sort_Order), ("@Active", dto.Is_Active), ("@Notes", dto.Notes));
        return Ok(new { message = "تم حفظ الدولة." });
    }

    [HttpPost("governorates")]
    public async Task<IActionResult> SaveGovernorate([FromBody] GovernorateRequest dto)
    {
        var access = RequireSystemAdmin();
        if (access != null) return access;
        if (dto.Country_ID <= 0 || string.IsNullOrWhiteSpace(dto.Governorate_Code) || string.IsNullOrWhiteSpace(dto.Governorate_Name_AR))
            return BadRequest("الدولة وكود المحافظة واسمها العربي مطلوبة.");

        if (dto.Governorate_ID > 0)
        {
            await ExecuteAsync("UPDATE governorates SET Country_ID=@Country, Governorate_Code=@Code, Governorate_Name_AR=@NameAR, Governorate_Name_EN=@NameEN, Sort_Order=@Sort, Is_Active=@Active, Notes=@Notes, Updated_At=UTC_TIMESTAMP() WHERE Governorate_ID=@ID",
                ("@Country", dto.Country_ID), ("@Code", dto.Governorate_Code.Trim().ToUpperInvariant()), ("@NameAR", dto.Governorate_Name_AR.Trim()), ("@NameEN", dto.Governorate_Name_EN), ("@Sort", dto.Sort_Order), ("@Active", dto.Is_Active), ("@Notes", dto.Notes), ("@ID", dto.Governorate_ID));
            return Ok(new { message = "تم تعديل المحافظة." });
        }

        await ExecuteAsync("INSERT INTO governorates (Country_ID, Governorate_Code, Governorate_Name_AR, Governorate_Name_EN, Sort_Order, Is_Active, Notes) VALUES (@Country,@Code,@NameAR,@NameEN,@Sort,@Active,@Notes)",
            ("@Country", dto.Country_ID), ("@Code", dto.Governorate_Code.Trim().ToUpperInvariant()), ("@NameAR", dto.Governorate_Name_AR.Trim()), ("@NameEN", dto.Governorate_Name_EN), ("@Sort", dto.Sort_Order), ("@Active", dto.Is_Active), ("@Notes", dto.Notes));
        return Ok(new { message = "تم حفظ المحافظة." });
    }

    [HttpPost("cities")]
    public async Task<IActionResult> SaveCity([FromBody] CityRequest dto)
    {
        var access = RequireSystemAdmin();
        if (access != null) return access;
        if (dto.Country_ID <= 0 || dto.Governorate_ID <= 0 || string.IsNullOrWhiteSpace(dto.City_Code) || string.IsNullOrWhiteSpace(dto.City_Name_AR))
            return BadRequest("الدولة والمحافظة وكود المدينة واسمها العربي مطلوبة.");

        if (dto.City_ID > 0)
        {
            await ExecuteAsync("UPDATE cities SET Country_ID=@Country, Governorate_ID=@Governorate, City_Code=@Code, City_Name_AR=@NameAR, City_Name_EN=@NameEN, Postal_Code=@Postal, Sort_Order=@Sort, Is_Active=@Active, Notes=@Notes, Updated_At=UTC_TIMESTAMP() WHERE City_ID=@ID",
                ("@Country", dto.Country_ID), ("@Governorate", dto.Governorate_ID), ("@Code", dto.City_Code.Trim().ToUpperInvariant()), ("@NameAR", dto.City_Name_AR.Trim()), ("@NameEN", dto.City_Name_EN), ("@Postal", dto.Postal_Code), ("@Sort", dto.Sort_Order), ("@Active", dto.Is_Active), ("@Notes", dto.Notes), ("@ID", dto.City_ID));
            return Ok(new { message = "تم تعديل المدينة." });
        }

        await ExecuteAsync("INSERT INTO cities (Country_ID, Governorate_ID, City_Code, City_Name_AR, City_Name_EN, Postal_Code, Sort_Order, Is_Active, Notes) VALUES (@Country,@Governorate,@Code,@NameAR,@NameEN,@Postal,@Sort,@Active,@Notes)",
            ("@Country", dto.Country_ID), ("@Governorate", dto.Governorate_ID), ("@Code", dto.City_Code.Trim().ToUpperInvariant()), ("@NameAR", dto.City_Name_AR.Trim()), ("@NameEN", dto.City_Name_EN), ("@Postal", dto.Postal_Code), ("@Sort", dto.Sort_Order), ("@Active", dto.Is_Active), ("@Notes", dto.Notes));
        return Ok(new { message = "تم حفظ المدينة." });
    }

    [HttpDelete("countries/{id:int}")]
    public async Task<IActionResult> DeleteCountry(int id)
    {
        var access = RequireSystemAdmin(); if (access != null) return access;
        if (await ScalarAsync<int>("SELECT COUNT(*) FROM governorates WHERE Country_ID=@ID", ("@ID", id)) > 0)
            return BadRequest("لا يمكن حذف دولة مرتبطة بمحافظات. أوقفها بدلاً من الحذف.");
        await ExecuteAsync("DELETE FROM countries WHERE Country_ID=@ID", ("@ID", id));
        return Ok();
    }

    [HttpDelete("governorates/{id:int}")]
    public async Task<IActionResult> DeleteGovernorate(int id)
    {
        var access = RequireSystemAdmin(); if (access != null) return access;
        if (await ScalarAsync<int>("SELECT COUNT(*) FROM cities WHERE Governorate_ID=@ID", ("@ID", id)) > 0)
            return BadRequest("لا يمكن حذف محافظة مرتبطة بمدن. أوقفها بدلاً من الحذف.");
        await ExecuteAsync("DELETE FROM governorates WHERE Governorate_ID=@ID", ("@ID", id));
        return Ok();
    }

    [HttpDelete("cities/{id:int}")]
    public async Task<IActionResult> DeleteCity(int id)
    {
        var access = RequireSystemAdmin(); if (access != null) return access;
        await ExecuteAsync("DELETE FROM cities WHERE City_ID=@ID", ("@ID", id));
        return Ok();
    }

    private async Task<List<Dictionary<string, object?>>> QueryAsync(string sql, params (string Name, object? Value)[] parameters)
    {
        await _context.Database.OpenConnectionAsync();
        await using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;
        AddParameters(command, parameters);
        await using var reader = await command.ExecuteReaderAsync();
        var rows = new List<Dictionary<string, object?>>();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < reader.FieldCount; i++) row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            rows.Add(row);
        }
        return rows;
    }

    private async Task<int> ExecuteAsync(string sql, params (string Name, object? Value)[] parameters)
    {
        await _context.Database.OpenConnectionAsync();
        await using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;
        AddParameters(command, parameters);
        return await command.ExecuteNonQueryAsync();
    }

    private async Task<T> ScalarAsync<T>(string sql, params (string Name, object? Value)[] parameters)
    {
        await _context.Database.OpenConnectionAsync();
        await using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;
        AddParameters(command, parameters);
        var result = await command.ExecuteScalarAsync();
        return result == null || result == DBNull.Value ? default! : (T)Convert.ChangeType(result, typeof(T));
    }

    private static void AddParameters(DbCommand command, IEnumerable<(string Name, object? Value)> parameters)
    {
        foreach (var (name, value) in parameters)
        {
            if (value == null) continue;
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }
    }

    public sealed class CountryRequest
    {
        public int Country_ID { get; set; }
        public string Country_Code { get; set; } = "";
        public string Country_Name_AR { get; set; } = "";
        public string? Country_Name_EN { get; set; }
        public string? ISO2 { get; set; }
        public string? ISO3 { get; set; }
        public string? Phone_Code { get; set; }
        public string? Currency_Code { get; set; }
        public string? Nationality_Name_AR { get; set; }
        public int Sort_Order { get; set; }
        public bool Is_Active { get; set; } = true;
        public string? Notes { get; set; }
    }

    public sealed class GovernorateRequest
    {
        public int Governorate_ID { get; set; }
        public int Country_ID { get; set; }
        public string Governorate_Code { get; set; } = "";
        public string Governorate_Name_AR { get; set; } = "";
        public string? Governorate_Name_EN { get; set; }
        public int Sort_Order { get; set; }
        public bool Is_Active { get; set; } = true;
        public string? Notes { get; set; }
    }

    public sealed class CityRequest
    {
        public int City_ID { get; set; }
        public int Country_ID { get; set; }
        public int Governorate_ID { get; set; }
        public string City_Code { get; set; } = "";
        public string City_Name_AR { get; set; } = "";
        public string? City_Name_EN { get; set; }
        public string? Postal_Code { get; set; }
        public int Sort_Order { get; set; }
        public bool Is_Active { get; set; } = true;
        public string? Notes { get; set; }
    }
}