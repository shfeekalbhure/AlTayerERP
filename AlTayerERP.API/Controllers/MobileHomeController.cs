using AlTayerERP.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlTayerERP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/mobile/home")]
public sealed class MobileHomeController : ControllerBase
{
    private readonly ScreenAuthorizationService _authorization;

    public MobileHomeController(ScreenAuthorizationService authorization)
    {
        _authorization = authorization;
    }

    [HttpGet("permissions")]
    public async Task<IActionResult> GetPermissions(CancellationToken cancellationToken)
    {
        if (HttpContext.Items["ServerSession"] is not ServerSession session)
            return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة." });

        var items = new[]
        {
            new MobilePermissionDefinition("PaymentRequest", "طلب صرف"),
            new MobilePermissionDefinition("ReceiptVoucher", "سند قبض"),
            new MobilePermissionDefinition("PaymentVoucher", "سند صرف"),
            new MobilePermissionDefinition("JournalVoucher", "قيد يومي"),
            new MobilePermissionDefinition("DocumentSearch", "البحث عن المستندات"),
            new MobilePermissionDefinition("ApprovalRequests", "طلبات الاعتماد"),
            new MobilePermissionDefinition("TrialBalance", "ميزان المراجعة"),
            new MobilePermissionDefinition("GeneralLedger", "الأستاذ العام")
        };

        var permissions = new List<object>();
        foreach (var item in items)
        {
            var canView = await _authorization.IsAllowedAsync(
                session, item.ScreenCode, ScreenOperation.View, cancellationToken);

            permissions.Add(new
            {
                screenCode = item.ScreenCode,
                title = item.Title,
                canView
            });
        }

        return Ok(new
        {
            session.User_ID,
            session.Company_ID,
            session.Branch_ID,
            yearId = session.Year_ID,
            permissions
        });
    }

    private sealed record MobilePermissionDefinition(string ScreenCode, string Title);
}
