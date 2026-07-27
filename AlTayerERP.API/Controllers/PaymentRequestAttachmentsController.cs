using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/payment-requests/{requestId:long}/attachments")]
public sealed class PaymentRequestAttachmentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ScreenAuthorizationService _auth;
    private readonly AuditTrailService _audit;
    private readonly IWebHostEnvironment _env;

    public PaymentRequestAttachmentsController(
        AppDbContext db,
        ScreenAuthorizationService auth,
        AuditTrailService audit,
        IWebHostEnvironment env)
    {
        _db = db;
        _auth = auth;
        _audit = audit;
        _env = env;
    }

    private ServerSession Session() =>
        HttpContext.Items["ServerSession"] as ServerSession
        ?? throw new InvalidOperationException("جلسة الخادم غير متاحة.");

    private Task<bool> Allowed(ScreenOperation operation) =>
        _auth.IsExplicitlyAllowedAsync(Session(), "PaymentRequestAttachments", operation);

    private Task<bool> RequestExists(long id) =>
        _db.Payment_Requests.AnyAsync(x =>
            x.Payment_Request_ID == id &&
            x.Company_ID == Session().Company_ID &&
            x.Branch_ID == Session().Branch_ID &&
            x.Fiscal_Year_ID == Session().Year_ID);

    [HttpGet]
    public async Task<IActionResult> List(long requestId)
    {
        if (!await Allowed(ScreenOperation.View)) return Forbid();
        if (!await RequestExists(requestId)) return NotFound();

        return Ok(await _db.Payment_Request_Attachments
            .Where(x => x.Payment_Request_ID == requestId &&
                        x.Company_ID == Session().Company_ID &&
                        x.Branch_ID == Session().Branch_ID &&
                        x.Fiscal_Year_ID == Session().Year_ID &&
                        x.Is_Active)
            .AsNoTracking()
            .OrderByDescending(x => x.Created_At)
            .ToListAsync());
    }

    [HttpGet("{attachmentId:long}/download")]
    public async Task<IActionResult> Download(long requestId, long attachmentId)
    {
        if (!await Allowed(ScreenOperation.View)) return Forbid();

        var row = await _db.Payment_Request_Attachments.AsNoTracking()
            .SingleOrDefaultAsync(x =>
                x.Payment_Request_Attachment_ID == attachmentId &&
                x.Payment_Request_ID == requestId &&
                x.Company_ID == Session().Company_ID &&
                x.Branch_ID == Session().Branch_ID &&
                x.Fiscal_Year_ID == Session().Year_ID &&
                x.Is_Active);

        if (row == null) return NotFound(new { message = "المرفق غير موجود." });

        var fullPath = Path.GetFullPath(Path.Combine(_env.ContentRootPath, row.Storage_Key));
        var contentRoot = Path.GetFullPath(_env.ContentRootPath);
        if (!fullPath.StartsWith(contentRoot, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(fullPath))
            return NotFound(new { message = "ملف المرفق غير موجود في التخزين." });

        return PhysicalFile(fullPath, row.Content_Type, row.Original_File_Name, enableRangeProcessing: true);
    }

    [HttpPost]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Upload(long requestId, IFormFile file)
    {
        if (!await Allowed(ScreenOperation.Add)) return Forbid();
        if (!await RequestExists(requestId)) return NotFound();
        if (file == null || file.Length == 0 || file.Length > 20 * 1024 * 1024)
            return BadRequest(new { message = "ملف المرفق غير صالح أو يتجاوز 20MB." });

        var session = Session();
        var safeName = Path.GetFileName(file.FileName);
        var storageKey = Path.Combine(
            "payment-request-attachments",
            session.Company_ID,
            session.Branch_ID.ToString(),
            session.Year_ID.ToString(),
            requestId.ToString(),
            Guid.NewGuid().ToString("N") + Path.GetExtension(safeName));

        var fullPath = Path.Combine(_env.ContentRootPath, storageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await using (var stream = System.IO.File.Create(fullPath))
            await file.CopyToAsync(stream);

        var row = new PaymentRequestAttachment
        {
            Payment_Request_ID = requestId,
            Company_ID = session.Company_ID,
            Branch_ID = session.Branch_ID,
            Fiscal_Year_ID = session.Year_ID,
            Original_File_Name = safeName,
            Storage_Key = storageKey,
            Content_Type = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
            File_Size = file.Length,
            Created_By = session.User_ID.ToString(),
            Created_At = DateTime.UtcNow,
            Is_Active = true
        };

        _db.Payment_Request_Attachments.Add(row);
        _audit.Add(session, HttpContext, "payment_request_attachments", "new", "UPLOAD", null,
            new { requestId, safeName, file.Length, row.Content_Type });
        await _db.SaveChangesAsync();
        return Ok(row);
    }

    [HttpDelete("{attachmentId:long}")]
    public async Task<IActionResult> Delete(long requestId, long attachmentId, [FromQuery] string reason)
    {
        if (!await Allowed(ScreenOperation.Delete)) return Forbid();
        if (string.IsNullOrWhiteSpace(reason))
            return BadRequest(new { message = "سبب حذف المرفق إلزامي." });

        var row = await _db.Payment_Request_Attachments.SingleOrDefaultAsync(x =>
            x.Payment_Request_Attachment_ID == attachmentId &&
            x.Payment_Request_ID == requestId &&
            x.Company_ID == Session().Company_ID &&
            x.Branch_ID == Session().Branch_ID &&
            x.Fiscal_Year_ID == Session().Year_ID &&
            x.Is_Active);

        if (row == null) return NotFound();

        row.Is_Active = false;
        _audit.Add(Session(), HttpContext, "payment_request_attachments", attachmentId.ToString(), "DELETE",
            new { row.Original_File_Name }, new { Is_Active = false }, reason.Trim());
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
