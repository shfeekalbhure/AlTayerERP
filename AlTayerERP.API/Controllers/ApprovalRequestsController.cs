using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers;

/// <summary>
/// مركز طلبات الاعتماد: يعرض ويقرر الطلبات ضمن الشركة الحالية فقط.
/// لا ينشئ الطلب من العميل؛ مصدره خدمات الأعمال مثل السقوف وطلبات الصرف.
/// </summary>
[ApiController]
[Route("api/approval-requests")]
public sealed class ApprovalRequestsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ScreenAuthorizationService _authorization;
    private readonly AuditTrailService _audit;

    public ApprovalRequestsController(AppDbContext db, ScreenAuthorizationService authorization, AuditTrailService audit)
    {
        _db = db;
        _authorization = authorization;
        _audit = audit;
    }

    private ServerSession Session() =>
        HttpContext.Items["ServerSession"] as ServerSession
        ?? throw new InvalidOperationException("جلسة الخادم غير متاحة.");

    private async Task<IActionResult?> RequireAsync(ScreenOperation operation) =>
        await _authorization.IsExplicitlyAllowedAsync(Session(), "ApprovalRequests", operation)
            ? null : Forbid();

    /// <summary>يعيد طلبات الشركة الحالية فقط؛ التصفية بالحالة اختيارية.</summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? status)
    {
        var denial = await RequireAsync(ScreenOperation.View);
        if (denial is not null) return denial;

        var query = _db.Approval_Requests.AsNoTracking()
            .Where(x => x.Company_ID == Session().Company_ID);

        if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "ALL", StringComparison.OrdinalIgnoreCase))
            query = query.Where(x => x.Status == status.Trim());

        return Ok(await query
            .OrderByDescending(x => x.Requested_At)
            .Take(500)
            .ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var denial = await RequireAsync(ScreenOperation.View);
        if (denial is not null) return denial;

        var row = await _db.Approval_Requests.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Approval_ID == id && x.Company_ID == Session().Company_ID);

        return row is null ? NotFound(new { message = "طلب الاعتماد غير موجود ضمن الشركة الحالية." }) : Ok(row);
    }

    [HttpPost("{id:int}/review")]
    public Task<IActionResult> Review(int id, [FromBody] ApprovalDecisionRequest request) =>
        ChangeStatusAsync(id, new[] { ApprovalStatus.Pending.ToString() }, ApprovalStatus.UnderReview,
            ScreenOperation.Approve, "REVIEW", request?.Reason, reasonRequired: false);

    [HttpPost("{id:int}/approve")]
    public Task<IActionResult> Approve(int id, [FromBody] ApprovalDecisionRequest request) =>
        ChangeStatusAsync(id, new[] { ApprovalStatus.Pending.ToString(), ApprovalStatus.UnderReview.ToString() },
            ApprovalStatus.Approved, ScreenOperation.Approve, "APPROVE", request?.Reason, reasonRequired: true);

    [HttpPost("{id:int}/reject")]
    public Task<IActionResult> Reject(int id, [FromBody] ApprovalDecisionRequest request) =>
        ChangeStatusAsync(id, new[] { ApprovalStatus.Pending.ToString(), ApprovalStatus.UnderReview.ToString() },
            ApprovalStatus.Rejected, ScreenOperation.Unapprove, "REJECT", request?.Reason, reasonRequired: true);

    [HttpPost("{id:int}/return")]
    public Task<IActionResult> Return(int id, [FromBody] ApprovalDecisionRequest request) =>
        ChangeStatusAsync(id, new[] { ApprovalStatus.Pending.ToString(), ApprovalStatus.UnderReview.ToString() },
            ApprovalStatus.Returned, ScreenOperation.Unapprove, "RETURN", request?.Reason, reasonRequired: true);

    // يضمن الانتقال الذري ويسجل هوية منفذ القرار من الجلسة الموثوقة فقط.
    private async Task<IActionResult> ChangeStatusAsync(
        int id, IReadOnlyCollection<string> allowedStatuses, ApprovalStatus target,
        ScreenOperation operation, string auditAction, string? reason, bool reasonRequired)
    {
        var denial = await RequireAsync(operation);
        if (denial is not null) return denial;

        if (reasonRequired && string.IsNullOrWhiteSpace(reason))
            return BadRequest(new { message = "سبب القرار إلزامي للتدقيق." });

        var row = await _db.Approval_Requests
            .SingleOrDefaultAsync(x => x.Approval_ID == id && x.Company_ID == Session().Company_ID);

        if (row is null)
            return NotFound(new { message = "طلب الاعتماد غير موجود ضمن الشركة الحالية." });

        if (!allowedStatuses.Contains(row.Status, StringComparer.OrdinalIgnoreCase))
            return Conflict(new { message = "الحالة الحالية لا تسمح بهذه العملية." });

        var before = new { row.Status, row.Approved_By, row.Approved_At, row.Approval_Notes };
        row.Status = target.ToString();
        row.Approved_By = Session().User_ID.ToString();
        row.Approved_At = DateTime.UtcNow;
        row.Approval_Notes = string.IsNullOrWhiteSpace(reason) ? row.Approval_Notes : reason.Trim();

        _audit.Add(Session(), HttpContext, "approval_requests", row.Approval_ID.ToString(), auditAction,
            before, new { row.Status, row.Approved_By, row.Approved_At, row.Approval_Notes }, row.Approval_Notes);

        await _db.SaveChangesAsync();
        return Ok(row);
    }
}

public sealed class ApprovalDecisionRequest
{
    public string? Reason { get; set; }
}
