using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers;

/// <summary>
/// يعرض بيانات التدقيق المختصرة الخاصة بالمستخدم من سجل التدقيق المركزي.
/// لا يعيد القيم القديمة أو الجديدة ولا أي بيانات حساسة.
/// </summary>
[Authorize]
[ApiController]
[Route("api/Users")]
public sealed class UserAuditController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserAuditController(AppDbContext context) => _context = context;

    [HttpGet("{id:int}/audit")]
    public async Task<IActionResult> GetUserAudit(int id)
    {
        var session = HttpContext.Items["ServerSession"] as ServerSession;
        if (session == null) return Unauthorized();

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.User_ID == id);
        if (user == null) return NotFound("المستخدم غير موجود.");

        if (!session.Is_System_Admin &&
            (user.Company_ID != session.Company_ID || user.Branch_ID != session.Branch_ID))
            return Forbid();

        var recordId = id.ToString();
        var directLogs = await _context.Audit_Logs.AsNoTracking()
            .Where(x => x.Table_Name == "users" && x.Record_ID == recordId)
            .OrderBy(x => x.Action_At)
            .ToListAsync();

        var creationLog = directLogs.FirstOrDefault(x => x.Action_Type == "INSERT");
        if (creationLog == null)
        {
            var loginToken = $"\"Login_Name\":\"{EscapeLike(user.Login_Name)}\"";
            creationLog = await _context.Audit_Logs.AsNoTracking()
                .Where(x => x.Table_Name == "users" && x.Action_Type == "INSERT" &&
                            x.New_Values != null && EF.Functions.Like(x.New_Values, $"%{loginToken}%"))
                .OrderBy(x => x.Action_At)
                .FirstOrDefaultAsync();
        }

        var updateLog = directLogs
            .Where(x => x.Action_Type == "UPDATE")
            .OrderByDescending(x => x.Action_At)
            .FirstOrDefault();

        var actorIds = new[] { creationLog?.User_ID, updateLog?.User_ID }
            .Select(x => int.TryParse(x, out var parsed) ? (int?)parsed : null)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();

        var actorNames = await _context.Users.AsNoTracking()
            .Where(x => actorIds.Contains(x.User_ID))
            .ToDictionaryAsync(x => x.User_ID, x => x.Full_Name);

        string ResolveName(string? actorId)
        {
            if (!int.TryParse(actorId, out var parsed)) return "—";
            return actorNames.TryGetValue(parsed, out var name) ? name : $"مستخدم #{parsed}";
        }

        return Ok(new
        {
            Created_By = ResolveName(creationLog?.User_ID),
            Created_At = user.Created_At,
            Updated_By = ResolveName(updateLog?.User_ID),
            Updated_At = user.Updated_At ?? updateLog?.Action_At
        });
    }

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
