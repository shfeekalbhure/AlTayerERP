using AlTayerERP.API.Services;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers;

[ApiController]
[Route("api/audit-logs")]
public sealed class AuditLogsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AuditLogsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string? entityName,
        [FromQuery] string? recordId,
        [FromQuery] string? actionCode,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var session = HttpContext.Items["ServerSession"] as ServerSession;
        if (session is null) return Unauthorized();
        if (!session.Is_System_Admin) return Forbid();

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 200);
        var query = _db.Audit_Logs.AsNoTracking()
            .Where(x => x.Company_ID == null || x.Company_ID == session.Company_ID);

        if (!string.IsNullOrWhiteSpace(entityName))
            query = query.Where(x => x.Table_Name == entityName.Trim());
        if (!string.IsNullOrWhiteSpace(recordId))
            query = query.Where(x => x.Record_ID == recordId.Trim());
        if (!string.IsNullOrWhiteSpace(actionCode))
            query = query.Where(x => x.Action_Type == actionCode.Trim().ToUpperInvariant());
        if (from.HasValue)
            query = query.Where(x => x.Action_At >= from.Value.ToUniversalTime());
        if (to.HasValue)
            query = query.Where(x => x.Action_At <= to.Value.ToUniversalTime());

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(x => x.Action_At)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }
}