using AlTayerERP.API.Services;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// استعراض غير قابل للتعديل لسجل التدقيق. لا توجد أي نقطة API لحذف أو تعديل السجل.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public sealed class AuditLogsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ScreenAuthorizationService _authorization;

        public AuditLogsController(AppDbContext context, ScreenAuthorizationService authorization)
        {
            _context = context;
            _authorization = authorization;
        }

        [HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int? userId,
            [FromQuery] string? tableName,
            [FromQuery] string? actionType,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 200)
        {
            if (HttpContext.Items["ServerSession"] is not ServerSession session)
                return Unauthorized();

            if (!await _authorization.IsAllowedAsync(session, "AuditLogs", ScreenOperation.View))
                return Forbid();

            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 500);

            var query = _context.Audit_Logs.AsNoTracking().AsQueryable();
            if (from.HasValue) query = query.Where(x => x.Action_At >= from.Value.ToUniversalTime());
            if (to.HasValue) query = query.Where(x => x.Action_At <= to.Value.ToUniversalTime());
            if (userId.HasValue) query = query.Where(x => x.User_ID == userId.Value.ToString());
            if (!string.IsNullOrWhiteSpace(tableName)) query = query.Where(x => x.Table_Name == tableName.Trim());
            if (!string.IsNullOrWhiteSpace(actionType)) query = query.Where(x => x.Action_Type == actionType.Trim());

            // غير مدير النظام يرى سجله فقط؛ لا يقرأ تدقيق شركات أو فروع أخرى.
            if (!session.Is_System_Admin)
                query = query.Where(x => x.User_ID == session.User_ID.ToString() && x.Branch_ID == session.Branch_ID.ToString());

            var total = await query.CountAsync();
            var rows = await query
                .OrderByDescending(x => x.Action_At)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.Audit_ID, x.Table_Name, x.Record_ID, x.Action_Type, x.User_ID, x.Branch_ID,
                    x.Action_At, x.Action_Channel, x.Device_Name, x.IP_Address, x.Notes
                })
                .ToListAsync();

            return Ok(new { total, page, pageSize, rows });
        }
    }
}
