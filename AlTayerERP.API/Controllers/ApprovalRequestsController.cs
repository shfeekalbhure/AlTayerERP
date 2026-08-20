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
    private const string PaymentRequestApprovalType = "PAYMENT_REQUEST";
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

        var rows = await query
            .OrderByDescending(x => x.Requested_At)
            .Take(500)
            .ToListAsync();

        // إظهار الاسم بدلاً من رقم المستخدم في بيانات الإنشاء والتعديل.
        var recordIds = rows.Select(x => x.Approval_ID.ToString()).ToList();
        var auditLogs = recordIds.Count == 0
            ? new List<AlTayerERP.Core.Entities.Accounting.AuditLog>()
            : await _db.Audit_Logs.AsNoTracking()
                .Where(x => x.Table_Name == "approval_requests" && recordIds.Contains(x.Record_ID))
                .ToListAsync();

        var userIds = rows
            .SelectMany(x => new[] { x.Requested_By, x.Approved_By })
            .Where(x => int.TryParse(x, out _))
            .Select(x => int.Parse(x!))
            .Distinct()
            .ToList();
        var users = userIds.Count == 0
            ? new Dictionary<int, string>()
            : await _db.Users.AsNoTracking()
                .Where(x => x.Company_ID == Session().Company_ID && userIds.Contains(x.User_ID))
                .ToDictionaryAsync(x => x.User_ID, x => x.Full_Name);

        string UserName(string? id) =>
            int.TryParse(id, out var userId) && users.TryGetValue(userId, out var name)
                ? name
                : string.IsNullOrWhiteSpace(id) ? "—" : id;

        return Ok(rows.Select(x =>
        {
            var actions = auditLogs.Where(log => log.Record_ID == x.Approval_ID.ToString()).ToList();
            return new
            {
                x.Approval_ID,
                x.Request_Type,
                x.Reference_Type,
                x.Reference_ID,
                x.Entity_Type,
                x.Entity_ID,
                x.Currency_Code,
                x.Amount,
                x.Reason,
                x.Status,
                Requested_By = UserName(x.Requested_By),
                x.Requested_At,
                Approved_By = UserName(x.Approved_By),
                x.Approved_At,
                x.Approval_Notes,
                Edit_Count = actions.Count(log =>
                    log.Action_Type == "REVIEW" ||
                    log.Action_Type == "APPROVE" ||
                    log.Action_Type == "REJECT" ||
                    log.Action_Type == "RETURN"),
                // هذه الشاشة لا تطبع طلب الاعتماد حالياً؛ العداد يظهر صفراً.
                Print_Count = 0
            };
        }));
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

        // فصل الواجبات: مقدم الطلب لا يراجع أو يعتمد أو يرفض طلبه بنفسه.
        // الطلبات القديمة التي لا تحتوي هوية مقدم الطلب تبقى قابلة للمعالجة.
        if (!string.IsNullOrWhiteSpace(row.Requested_By) &&
            string.Equals(row.Requested_By, Session().User_ID.ToString(), StringComparison.OrdinalIgnoreCase))
            return Conflict(new { message = "لا يمكن لمقدم الطلب تنفيذ قرار على طلبه. اختر مستخدماً مخولاً آخر." });

        var linkedRequestValidation = await ValidateLinkedPaymentRequestTransitionAsync(row, target);
        if (linkedRequestValidation is not null)
            return Conflict(new { message = linkedRequestValidation });

        var before = new { row.Status, row.Approved_By, row.Approved_At, row.Approval_Notes };
        row.Status = target.ToString();
        // بيانات الاعتماد لا تسجل إلا عند الاعتماد النهائي؛ المراجعة والرفض
        // والإرجاع توثق في سجل التدقيق ولا ينبغي أن تظهر كاعتماد.
        if (target == ApprovalStatus.Approved)
        {
            row.Approved_By = Session().User_ID.ToString();
            row.Approved_At = DateTime.UtcNow;
        }
        else
        {
            row.Approved_By = null;
            row.Approved_At = null;
        }
        row.Approval_Notes = string.IsNullOrWhiteSpace(reason) ? row.Approval_Notes : reason.Trim();

        await SynchronizeLinkedPaymentRequestAsync(row, target, row.Approval_Notes);

        _audit.Add(Session(), HttpContext, "approval_requests", row.Approval_ID.ToString(), auditAction,
            before, new { row.Status, row.Approved_By, row.Approved_At, row.Approval_Notes }, row.Approval_Notes);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new
            {
                message = "تم تغيير حالة طلب الاعتماد من مستخدم آخر. حدّث القائمة ثم راجع الحالة الحالية قبل اتخاذ قرار جديد."
            });
        }

        return Ok(row);
    }

    private async Task<string?> ValidateLinkedPaymentRequestTransitionAsync(
        ApprovalRequest approval, ApprovalStatus target)
    {
        if (!string.Equals(approval.Reference_Type, PaymentRequestApprovalType, StringComparison.Ordinal) ||
            !long.TryParse(approval.Reference_ID, out var requestId))
            return null;

        var request = await _db.Payment_Requests.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Payment_Request_ID == requestId && x.Company_ID == Session().Company_ID);
        if (request is null)
            return "طلب الصرف المرتبط غير موجود ضمن الشركة الحالية.";

        return target switch
        {
            ApprovalStatus.UnderReview when request.Status != "PENDING_REVIEW" =>
                "حالة طلب الصرف المرتبط لا تسمح بالمراجعة.",
            ApprovalStatus.Approved or ApprovalStatus.Rejected or ApprovalStatus.Returned
                when request.Status != "PENDING_APPROVAL" =>
                "حالة طلب الصرف المرتبط لا تسمح بالقرار النهائي.",
            _ => null
        };
    }

    private async Task SynchronizeLinkedPaymentRequestAsync(
        ApprovalRequest approval, ApprovalStatus target, string? reason)
    {
        if (!string.Equals(approval.Reference_Type, PaymentRequestApprovalType, StringComparison.Ordinal) ||
            !long.TryParse(approval.Reference_ID, out var requestId))
            return;

        var request = await _db.Payment_Requests
            .Include(x => x.Details)
            .SingleOrDefaultAsync(x => x.Payment_Request_ID == requestId && x.Company_ID == Session().Company_ID);
        if (request is null) return;

        request.Status = target switch
        {
            ApprovalStatus.UnderReview => "PENDING_APPROVAL",
            ApprovalStatus.Approved => "APPROVED",
            ApprovalStatus.Rejected => "REJECTED",
            ApprovalStatus.Returned => "RETURNED",
            _ => request.Status
        };
        request.Review_Reason = reason;
        request.Updated_By = Session().User_ID.ToString();
        request.Updated_At = DateTime.UtcNow;
        if (target == ApprovalStatus.Approved)
            request.Approved_Local_Total = request.Details.Sum(x => x.Local_Amount);

        _audit.Add(Session(), HttpContext, "payment_requests", request.Payment_Request_ID.ToString(),
            $"APPROVAL_{target.ToString().ToUpperInvariant()}", null,
            new { request.Status, request.Approved_Local_Total }, reason);
    }
}

public sealed class ApprovalDecisionRequest
{
    public string? Reason { get; set; }
}
