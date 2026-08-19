using AlTayerERP.API.Services;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/mobile/document-search")]
public sealed class MobileDocumentSearchController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ScreenAuthorizationService _authorization;

    public MobileDocumentSearchController(AppDbContext db, ScreenAuthorizationService authorization)
    {
        _db = db;
        _authorization = authorization;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] string? type, CancellationToken cancellationToken)
    {
        if (HttpContext.Items["ServerSession"] is not ServerSession session)
            return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة." });

        if (!await _authorization.IsAllowedAsync(session, "DocumentSearch", ScreenOperation.View, cancellationToken))
            return Forbid();

        query = query?.Trim() ?? string.Empty;
        if (query.Length < 1)
            return BadRequest(new { message = "أدخل رقم المستند أو المرجع للبحث." });

        var wantedType = type?.Trim().ToUpperInvariant();
        var results = new List<object>();

        if (string.IsNullOrWhiteSpace(wantedType) || wantedType == "PAYMENT_REQUEST")
        {
            var requests = await _db.Payment_Requests.AsNoTracking()
                .Where(x => x.Company_ID == session.Company_ID && x.Branch_ID == session.Branch_ID &&
                            x.Fiscal_Year_ID == session.Year_ID &&
                            (x.Request_No.Contains(query) || (x.Header_Reference_No != null && x.Header_Reference_No.Contains(query))))
                .OrderByDescending(x => x.Request_Date)
                .Take(100)
                .Select(x => new
                {
                    documentType = "PAYMENT_REQUEST",
                    documentTypeDisplay = "طلب صرف",
                    documentId = x.Payment_Request_ID,
                    documentNo = x.Request_No,
                    documentDate = x.Request_Date,
                    partyName = x.Beneficiary_Name,
                    description = x.Description,
                    referenceNo = x.Header_Reference_No,
                    localTotal = x.Approved_Local_Total,
                    status = x.Status,
                    isPosted = false
                }).ToListAsync(cancellationToken);
            results.AddRange(requests);
        }

        if (string.IsNullOrWhiteSpace(wantedType) || wantedType is "RECEIPT" or "PAYMENT" or "JOURNAL")
        {
            var vouchersQuery = from h in _db.Financial_Voucher_Headers.AsNoTracking()
                                join t in _db.Voucher_Types.AsNoTracking() on h.Voucher_Type_ID equals t.Voucher_Type_ID
                                where h.Branch_ID == session.Branch_ID.ToString() && h.Fiscal_Year_ID == session.Year_ID && h.Is_Active
                                   && (h.Voucher_No.Contains(query) || (h.Reference_No != null && h.Reference_No.Contains(query)) ||
                                       (h.Source_Document_No != null && h.Source_Document_No.Contains(query)))
                                select new { h, t.Voucher_Type_Code };

            if (!string.IsNullOrWhiteSpace(wantedType))
                vouchersQuery = vouchersQuery.Where(x => x.Voucher_Type_Code == wantedType);

            var vouchers = await vouchersQuery.OrderByDescending(x => x.h.Voucher_Date).Take(200)
                .Select(x => new
                {
                    documentType = x.Voucher_Type_Code,
                    documentTypeDisplay = x.Voucher_Type_Code == "RECEIPT" ? "سند قبض" : x.Voucher_Type_Code == "PAYMENT" ? "سند صرف" : "قيد يومي",
                    documentId = x.h.Voucher_ID,
                    documentNo = x.h.Voucher_No,
                    documentDate = x.h.Voucher_Date,
                    partyName = x.h.Received_From_Name,
                    description = x.h.Description,
                    referenceNo = x.h.Reference_No,
                    localTotal = x.h.Local_Total,
                    status = x.h.Is_Posted ? "POSTED" : "UNPOSTED",
                    isPosted = x.h.Is_Posted
                }).ToListAsync(cancellationToken);
            results.AddRange(vouchers);
        }

        return Ok(results);
    }
}
