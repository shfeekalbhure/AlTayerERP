using System.Text;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// استعراض سجل التدقيق المركزي بصورة قراءة فقط.
    /// السجل Append-Only ولا توجد أي نقطة تعديل أو حذف له.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public sealed class AuditLogsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ScreenAuthorizationService _authorization;
        private readonly AuditTrailService _audit;

        public AuditLogsController(AppDbContext context, ScreenAuthorizationService authorization, AuditTrailService audit)
        {
            _context = context;
            _authorization = authorization;
            _audit = audit;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] AuditLogSearchRequest request)
        {
            var denied = await DenyUnlessAsync(ScreenOperation.View);
            if (denied != null || Session == null) return denied!;
            var session = Session;

            var query = ApplyFilters(ApplyScope(_context.Audit_Logs.AsNoTracking(), session), request);
            var total = await query.CountAsync();
            var page = Math.Max(request.Page, 1);
            var pageSize = Math.Clamp(request.PageSize, 1, 500);
            var logs = await query.OrderByDescending(x => x.Action_At)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return Ok(new { total, page, pageSize, rows = await ToRowsAsync(logs) });
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetDetails(long id)
        {
            var denied = await DenyUnlessAsync(ScreenOperation.View);
            if (denied != null || Session == null) return denied!;
            var session = Session;
            var log = await ApplyScope(_context.Audit_Logs.AsNoTracking(), session)
                .FirstOrDefaultAsync(x => x.Audit_ID == id);
            if (log == null) return NotFound(new { message = "سجل التدقيق غير موجود أو خارج نطاق صلاحيتك." });

            var row = (await ToRowsAsync(new List<AuditLog> { log })).Single();
            return Ok(new
            {
                row,
                log.Old_Values,
                log.New_Values,
                log.Notes,
                log.IP_Address,
                log.Device_Name
            });
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] AuditLogSearchRequest request)
        {
            var denied = await DenyUnlessAsync(ScreenOperation.Export);
            if (denied != null || Session == null) return denied!;
            var session = Session;

            var logs = await ApplyFilters(ApplyScope(_context.Audit_Logs.AsNoTracking(), session), request)
                .OrderByDescending(x => x.Action_At).Take(5000).ToListAsync();
            var rows = await ToRowsAsync(logs);
            var csv = new StringBuilder();
            csv.AppendLine("رقم السجل,الشاشة,رقم العملية,العملية,المستخدم,الفرع,التاريخ والوقت,القناة,الجهاز,العنوان,ملاحظات");
            foreach (var row in rows)
                csv.AppendLine(string.Join(',', new[]
                {
                    Csv(row.Audit_ID.ToString()), Csv(row.Table_Name), Csv(row.Record_ID), Csv(row.Action_Type),
                    Csv(row.User_Name), Csv(row.Branch_Name), Csv(row.Action_At.ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss")),
                    Csv(row.Action_Channel), Csv(row.Device_Name), Csv(row.IP_Address), Csv(row.Notes)
                }));

            _audit.Add(session, HttpContext, "audit_logs", "LIST", "EXPORT", notes: "تصدير نتائج سجل التدقيق والرقابة.");
            await _audit.SaveChangesAsync();
            return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray(), "text/csv; charset=utf-8", "audit-log.csv");
        }

        [HttpPost("print")]
        public async Task<IActionResult> RegisterPrint()
        {
            var denied = await DenyUnlessAsync(ScreenOperation.Print);
            if (denied != null || Session == null) return denied!;
            var session = Session;
            _audit.Add(session, HttpContext, "audit_logs", "LIST", "PRINT", notes: "فتح معاينة طباعة سجل التدقيق والرقابة.");
            await _audit.SaveChangesAsync();
            return Ok(new { message = "تم تسجيل عملية الطباعة." });
        }

        private ServerSession? Session => HttpContext.Items["ServerSession"] as ServerSession;

        private async Task<IActionResult?> DenyUnlessAsync(ScreenOperation operation)
        {
            if (Session == null) return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة." });
            return await _authorization.IsAllowedAsync(Session, "AuditLogs", operation) ? null : Forbid();
        }

        private static IQueryable<AuditLog> ApplyScope(IQueryable<AuditLog> query, ServerSession session) =>
            session.Is_System_Admin
                ? query
                : query.Where(x => x.User_ID == session.User_ID.ToString() && x.Branch_ID == session.Branch_ID.ToString());

        private static IQueryable<AuditLog> ApplyFilters(IQueryable<AuditLog> query, AuditLogSearchRequest request)
        {
            if (request.From.HasValue) query = query.Where(x => x.Action_At >= request.From.Value.ToUniversalTime());
            if (request.To.HasValue) query = query.Where(x => x.Action_At <= request.To.Value.ToUniversalTime());
            if (request.User_ID.HasValue) query = query.Where(x => x.User_ID == request.User_ID.Value.ToString());
            if (request.Branch_ID.HasValue) query = query.Where(x => x.Branch_ID == request.Branch_ID.Value.ToString());
            if (!string.IsNullOrWhiteSpace(request.Table_Name)) query = query.Where(x => x.Table_Name == request.Table_Name.Trim());
            if (!string.IsNullOrWhiteSpace(request.Action_Type)) query = query.Where(x => x.Action_Type == request.Action_Type.Trim());
            if (!string.IsNullOrWhiteSpace(request.Record_ID)) query = query.Where(x => x.Record_ID.Contains(request.Record_ID.Trim()));
            if (!string.IsNullOrWhiteSpace(request.Action_Channel)) query = query.Where(x => x.Action_Channel == request.Action_Channel.Trim());
            if (!string.IsNullOrWhiteSpace(request.Device_Name)) query = query.Where(x => x.Device_Name != null && x.Device_Name.Contains(request.Device_Name.Trim()));
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(x => x.Record_ID.Contains(search) || x.Table_Name.Contains(search) || x.Action_Type.Contains(search) ||
                    (x.Notes != null && x.Notes.Contains(search)) || (x.Device_Name != null && x.Device_Name.Contains(search)));
            }
            return query;
        }

        private async Task<List<AuditLogRow>> ToRowsAsync(IReadOnlyCollection<AuditLog> logs)
        {
            var userIds = logs.Select(x => x.User_ID).Where(x => int.TryParse(x, out _)).Select(x => int.Parse(x!)).Distinct().ToList();
            var branchIds = logs.Select(x => x.Branch_ID).Where(x => int.TryParse(x, out _)).Select(x => int.Parse(x!)).Distinct().ToList();
            var users = userIds.Count == 0 ? new Dictionary<int, string>() : await _context.Users.AsNoTracking()
                .Where(x => userIds.Contains(x.User_ID)).ToDictionaryAsync(x => x.User_ID, x => x.Full_Name);
            var branches = branchIds.Count == 0 ? new Dictionary<int, string>() : await _context.Tenant_Branches.AsNoTracking()
                .Where(x => branchIds.Contains(x.Branch_ID)).ToDictionaryAsync(x => x.Branch_ID, x => x.Branch_Name);
            return logs.Select(x => new AuditLogRow
            {
                Audit_ID = x.Audit_ID, Table_Name = x.Table_Name, Record_ID = x.Record_ID, Action_Type = x.Action_Type,
                User_ID = x.User_ID, User_Name = int.TryParse(x.User_ID, out var userId) && users.TryGetValue(userId, out var userName) ? userName : "غير متاح",
                Branch_ID = x.Branch_ID, Branch_Name = int.TryParse(x.Branch_ID, out var branchId) && branches.TryGetValue(branchId, out var branchName) ? branchName : "غير متاح",
                Action_At = x.Action_At, Action_Channel = x.Action_Channel, Device_Name = x.Device_Name, IP_Address = x.IP_Address, Notes = x.Notes
            }).ToList();
        }

        private static string Csv(string? value) => "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
    }

    public sealed class AuditLogSearchRequest
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int? User_ID { get; set; }
        public int? Branch_ID { get; set; }
        public string? Table_Name { get; set; }
        public string? Action_Type { get; set; }
        public string? Record_ID { get; set; }
        public string? Action_Channel { get; set; }
        public string? Device_Name { get; set; }
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 100;
    }

    public sealed class AuditLogRow
    {
        public long Audit_ID { get; set; }
        public string Table_Name { get; set; } = string.Empty;
        public string Record_ID { get; set; } = string.Empty;
        public string Action_Type { get; set; } = string.Empty;
        public string? User_ID { get; set; }
        public string User_Name { get; set; } = "غير متاح";
        public string? Branch_ID { get; set; }
        public string Branch_Name { get; set; } = "غير متاح";
        public DateTime Action_At { get; set; }
        public string Action_Channel { get; set; } = string.Empty;
        public string? Device_Name { get; set; }
        public string? IP_Address { get; set; }
        public string? Notes { get; set; }
    }
}
