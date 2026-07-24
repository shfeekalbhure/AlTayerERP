using AlTayerERP.API.Services;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace AlTayerERP.API.Controllers;

/// <summary>قوائم شاشة الفروع: أنواع الفروع والعملات النشطة للشركة المختارة.</summary>
[ApiController]
[Route("api/branch-reference-lookups")]
public sealed class BranchReferenceLookupsController : ControllerBase
{
    private readonly AppDbContext _db;
    public BranchReferenceLookupsController(AppDbContext db) => _db = db;

    private bool TryGetAdmin(out ServerSession session)
    {
        session = HttpContext.Items["ServerSession"] as ServerSession
            ?? new ServerSession(string.Empty, 0, 0, false, string.Empty, 0, 0, "unknown", DateTime.MinValue, DateTime.MinValue);
        return session.Is_System_Admin;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string companyId)
    {
        if (!TryGetAdmin(out _)) return Forbid();
        if (string.IsNullOrWhiteSpace(companyId)) return BadRequest("معرف الشركة مطلوب.");
        var companyExists = await _db.Companies.AsNoTracking().AnyAsync(x => x.Company_ID == companyId.Trim() && x.Is_Active);
        if (!companyExists) return NotFound("الشركة غير موجودة أو موقوفة.");

        var currencies = await _db.Currencies.AsNoTracking()
            .Where(x => x.Company_ID == companyId.Trim() && x.Is_Active)
            .OrderByDescending(x => x.Is_Local_Currency)
            .ThenBy(x => x.Currency_Code)
            .Select(x => new { x.Currency_ID, x.Currency_Code, x.Currency_Name_AR, x.Is_Local_Currency, x.Is_Default })
            .ToListAsync();

        var branchTypes = new List<object>();
        var connection = _db.Database.GetDbConnection();
        await EnsureOpenAsync(connection);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Branch_Type_ID, Branch_Type_Code, Branch_Type_Name_AR FROM branch_types WHERE Is_Active=1 ORDER BY Sort_Order, Branch_Type_Name_AR";
        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                branchTypes.Add(new
                {
                    Branch_Type_ID = reader.GetInt32(0),
                    Branch_Type_Code = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Branch_Type_Name_AR = reader.IsDBNull(2) ? string.Empty : reader.GetString(2)
                });
            }
        }
        catch (DbException)
        {
            return Conflict("جدول أنواع الفروع غير مهيأ. شغّل ترقية branch_types أولاً.");
        }

        return Ok(new { BranchTypes = branchTypes, Currencies = currencies });
    }

    private static async Task EnsureOpenAsync(DbConnection connection)
    {
        if (connection.State != System.Data.ConnectionState.Open) await connection.OpenAsync();
    }
}
