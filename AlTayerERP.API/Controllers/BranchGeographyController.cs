using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace AlTayerERP.API.Controllers;

/// <summary>إدارة الربط الجغرافي للفرع: الدولة ← المحافظة ← المدينة.</summary>
[ApiController]
[Route("api/branch-geography")]
public sealed class BranchGeographyController : ControllerBase
{
    private readonly AppDbContext _db;

    public BranchGeographyController(AppDbContext db) => _db = db;

    private bool TryGetAdmin(out ServerSession session)
    {
        session = HttpContext.Items["ServerSession"] as ServerSession
            ?? new ServerSession(string.Empty, 0, 0, false, string.Empty, 0, 0, "unknown", DateTime.MinValue, DateTime.MinValue);
        return session.Is_System_Admin;
    }

    [HttpGet("{branchId:int}")]
    public async Task<IActionResult> Get(int branchId)
    {
        if (!TryGetAdmin(out _)) return Forbid();
        var row = await QuerySingleAsync(
            "SELECT Branch_ID, Country_ID, Governorate_ID, City_ID FROM Tenant_Branches WHERE Branch_ID=@Branch_ID",
            ("@Branch_ID", branchId));
        return row is null ? NotFound("الفرع غير موجود.") : Ok(row);
    }

    [HttpPut("{branchId:int}")]
    public async Task<IActionResult> Update(int branchId, [FromBody] BranchGeographyDto dto)
    {
        if (!TryGetAdmin(out var session)) return Forbid();
        if (dto is null || dto.Country_ID <= 0 || dto.Governorate_ID <= 0 || dto.City_ID <= 0)
            return BadRequest("الدولة والمحافظة والمدينة مطلوبة للفرع.");

        var branchExists = await ScalarAsync<int>(
            "SELECT COUNT(*) FROM Tenant_Branches WHERE Branch_ID=@Branch_ID",
            ("@Branch_ID", branchId));
        if (branchExists == 0) return NotFound("الفرع غير موجود.");

        var countryValid = await ScalarAsync<int>(
            "SELECT COUNT(*) FROM countries WHERE Country_ID=@Country_ID AND Is_Active=1",
            ("@Country_ID", dto.Country_ID));
        if (countryValid == 0) return BadRequest("الدولة غير موجودة أو موقوفة.");

        var governorateValid = await ScalarAsync<int>(
            "SELECT COUNT(*) FROM governorates WHERE Governorate_ID=@Governorate_ID AND Country_ID=@Country_ID AND Is_Active=1",
            ("@Governorate_ID", dto.Governorate_ID), ("@Country_ID", dto.Country_ID));
        if (governorateValid == 0) return BadRequest("المحافظة لا تتبع الدولة المختارة أو أنها موقوفة.");

        var cityValid = await ScalarAsync<int>(
            "SELECT COUNT(*) FROM cities WHERE City_ID=@City_ID AND Governorate_ID=@Governorate_ID AND Country_ID=@Country_ID AND Is_Active=1",
            ("@City_ID", dto.City_ID), ("@Governorate_ID", dto.Governorate_ID), ("@Country_ID", dto.Country_ID));
        if (cityValid == 0) return BadRequest("المدينة لا تتبع المحافظة والدولة المختارتين أو أنها موقوفة.");

        await using var tx = await _db.Database.BeginTransactionAsync();
        var changed = await ExecuteAsync(
            "UPDATE Tenant_Branches SET Country_ID=@Country_ID, Governorate_ID=@Governorate_ID, City_ID=@City_ID, Updated_Date=UTC_TIMESTAMP() WHERE Branch_ID=@Branch_ID",
            ("@Country_ID", dto.Country_ID), ("@Governorate_ID", dto.Governorate_ID), ("@City_ID", dto.City_ID), ("@Branch_ID", branchId));
        if (changed == 0) return NotFound("الفرع غير موجود.");

        _db.Audit_Logs.Add(new AuditLog
        {
            Table_Name = "tenant_branches",
            Record_ID = branchId.ToString(),
            Action_Type = "UPDATE_GEOGRAPHY",
            User_ID = session.User_ID.ToString(),
            Branch_ID = session.Branch_ID.ToString(),
            Action_At = DateTime.UtcNow,
            New_Values = System.Text.Json.JsonSerializer.Serialize(dto),
            Action_Channel = "DESKTOP",
            Device_Name = Request.Headers["X-Device-ID"].ToString(),
            IP_Address = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Notes = "تحديث المراجع الجغرافية للفرع"
        });
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(new { message = "تم حفظ الموقع الجغرافي للفرع." });
    }

    private async Task<Dictionary<string, object?>?> QuerySingleAsync(string sql, params (string Name, object? Value)[] args)
    {
        await using var connection = _db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open) await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        AddParameters(command, args);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < reader.FieldCount; i++) result[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
        return result;
    }

    private async Task<T> ScalarAsync<T>(string sql, params (string Name, object? Value)[] args)
    {
        await using var connection = _db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open) await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        AddParameters(command, args);
        var value = await command.ExecuteScalarAsync();
        return (T)Convert.ChangeType(value ?? default(T)!, typeof(T));
    }

    private async Task<int> ExecuteAsync(string sql, params (string Name, object? Value)[] args)
    {
        var connection = _db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open) await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.Transaction = _db.Database.CurrentTransaction?.GetDbTransaction();
        command.CommandText = sql;
        AddParameters(command, args);
        return await command.ExecuteNonQueryAsync();
    }

    private static void AddParameters(DbCommand command, IEnumerable<(string Name, object? Value)> args)
    {
        foreach (var (name, value) in args)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }
    }

    public sealed class BranchGeographyDto
    {
        public int Country_ID { get; set; }
        public int Governorate_ID { get; set; }
        public int City_ID { get; set; }
    }
}
