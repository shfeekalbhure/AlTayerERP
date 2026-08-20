using System.Text.Json;
using AlTayerERP.API.Services;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers;

[ApiController]
[Route("api/vouchers/{voucherId:long}/attachments")]
public sealed class VoucherAttachmentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ScreenAuthorizationService _authorization;
    private readonly AuditTrailService _audit;
    private readonly IWebHostEnvironment _environment;
    private readonly IAttachmentMalwareScanner _malwareScanner;
    private const long MaxBytes = 10 * 1024 * 1024;

    public VoucherAttachmentsController(AppDbContext db, ScreenAuthorizationService authorization, AuditTrailService audit, IWebHostEnvironment environment, IAttachmentMalwareScanner malwareScanner)
    { _db = db; _authorization = authorization; _audit = audit; _environment = environment; _malwareScanner = malwareScanner; }

    private ServerSession Session() => HttpContext.Items["ServerSession"] as ServerSession
        ?? throw new InvalidOperationException("جلسة الخادم غير متاحة.");

    private async Task<IActionResult?> AuthorizeAsync(long voucherId, ScreenOperation operation)
    {
        var session = Session();
        var voucher = await _db.Financial_Voucher_Headers.AsNoTracking()
            .Where(x => x.Voucher_ID == voucherId && x.Is_Active)
            .Select(x => new { x.Voucher_ID, x.Branch_ID, x.Fiscal_Year_ID, x.Voucher_Type_ID, x.Voucher_No })
            .SingleOrDefaultAsync();
        if (voucher == null || voucher.Branch_ID != session.Branch_ID.ToString() || voucher.Fiscal_Year_ID != session.Year_ID)
            return NotFound(new { message = "السند غير موجود ضمن نطاق الجلسة." });
        var type = await _db.Voucher_Types.AsNoTracking().Where(x => x.Voucher_Type_ID == voucher.Voucher_Type_ID)
            .Select(x => x.Voucher_Type_Code).SingleOrDefaultAsync();
        var screen = type?.Trim().ToUpperInvariant() switch { "RECEIPT" => "ReceiptVoucher", "PAYMENT" => "PaymentVoucher", "JOURNAL" => "JournalVoucher", _ => null };
        if (screen == null || !await _authorization.IsAllowedAsync(session, screen, operation)) return Forbid();
        return null;
    }

    [HttpGet]
    public async Task<IActionResult> List(long voucherId)
    {
        var denial = await AuthorizeAsync(voucherId, ScreenOperation.View); if (denial != null) return denial;
        return Ok(await ReadAsync(voucherId));
    }

    [HttpPost]
    [RequestSizeLimit(MaxBytes)]
    public async Task<IActionResult> Upload(long voucherId, IFormFile file, [FromForm] string? notes)
    {
        var denial = await AuthorizeAsync(voucherId, ScreenOperation.Add); if (denial != null) return denial;
        if (file == null || file.Length == 0 || file.Length > MaxBytes) return BadRequest(new { message = "الملف مطلوب ولا يتجاوز 10MB." });
        var validation = await AttachmentUploadPolicy.ValidateAsync(file, HttpContext.RequestAborted);
        if (!validation.IsValid) return BadRequest(new { message = validation.Message });
        var extension = Path.GetExtension(Path.GetFileName(file.FileName)).ToLowerInvariant();

        var attachment = new AttachmentRecord { Id = Guid.NewGuid().ToString("N"), Name = Path.GetFileName(file.FileName), Extension = extension, ContentType = validation.ContentType, Bytes = file.Length, Notes = Trim(notes, 500), UploadedAtUtc = DateTime.UtcNow, UploadedBy = Session().User_ID.ToString(), Active = true, ScanStatus = AttachmentScanStatus.Pending };
        var directory = Folder(voucherId); Directory.CreateDirectory(directory);
        var target = Path.Combine(directory, attachment.Id + attachment.Extension);
        var quarantineDirectory = Path.Combine(directory, ".quarantine");
        Directory.CreateDirectory(quarantineDirectory);
        var quarantineTarget = Path.Combine(quarantineDirectory, attachment.Id + attachment.Extension);
        var temporaryTarget = quarantineTarget + ".uploading";
        try
        {
            await using (var output = System.IO.File.Create(temporaryTarget))
            {
                await file.CopyToAsync(output, HttpContext.RequestAborted);
                await output.FlushAsync(HttpContext.RequestAborted);
            }
            System.IO.File.Move(temporaryTarget, quarantineTarget);

            var scan = await _malwareScanner.ScanAsync(quarantineTarget, HttpContext.RequestAborted);
            attachment.ScanStatus = scan.Outcome switch
            {
                AttachmentMalwareScanOutcome.Clean => AttachmentScanStatus.Clean,
                AttachmentMalwareScanOutcome.ThreatDetected => AttachmentScanStatus.Rejected,
                _ => AttachmentScanStatus.Pending
            };
            attachment.ScanMessage = scan.Message;

            if (attachment.ScanStatus == AttachmentScanStatus.Clean)
                System.IO.File.Move(quarantineTarget, target, overwrite: true);

            var items = await ReadAsync(voucherId); items.Add(attachment);
            await WriteManifestAtomicAsync(voucherId, items, HttpContext.RequestAborted);
        }
        catch
        {
            if (System.IO.File.Exists(temporaryTarget)) System.IO.File.Delete(temporaryTarget);
            if (System.IO.File.Exists(target)) System.IO.File.Delete(target);
            throw;
        }
        _audit.Add(Session(), HttpContext, "voucher_attachments", attachment.Id, "CREATE", null, new { voucherId, attachment.Name, attachment.Bytes, attachment.ScanStatus }, attachment.Notes);
        await _audit.SaveChangesAsync();
        return Ok(attachment);
    }

    [HttpGet("{attachmentId}/download")]
    public async Task<IActionResult> Download(long voucherId, string attachmentId)
    {
        var denial = await AuthorizeAsync(voucherId, ScreenOperation.Export); if (denial != null) return denial;
        var item = (await ReadAsync(voucherId)).SingleOrDefault(x => x.Id == attachmentId && x.Active);
        if (item == null) return NotFound();
        if (item.ScanStatus == AttachmentScanStatus.Pending)
            return StatusCode(StatusCodes.Status423Locked, new { message = "المرفق قيد فحص الحماية ولم يصبح متاحاً للتنزيل بعد." });
        if (item.ScanStatus == AttachmentScanStatus.Rejected)
            return StatusCode(StatusCodes.Status410Gone, new { message = "المرفق رُفض في فحص الحماية." });
        var path = Path.Combine(Folder(voucherId), item.Id + item.Extension);
        if (!System.IO.File.Exists(path)) return NotFound(new { message = "الملف غير متاح." });
        _audit.Add(Session(), HttpContext, "voucher_attachments", item.Id, "DOWNLOAD", null, new { voucherId, item.Name });
        await _audit.SaveChangesAsync();
        return PhysicalFile(path, "application/octet-stream", item.Name);
    }

    [HttpDelete("{attachmentId}")]
    public async Task<IActionResult> Delete(long voucherId, string attachmentId, [FromBody] AttachmentReasonRequest request)
    {
        var denial = await AuthorizeAsync(voucherId, ScreenOperation.Delete); if (denial != null) return denial;
        if (request == null || string.IsNullOrWhiteSpace(request.Reason)) return BadRequest(new { message = "سبب الحذف المنطقي مطلوب." });
        var items = await ReadAsync(voucherId); var item = items.SingleOrDefault(x => x.Id == attachmentId && x.Active);
        if (item == null) return NotFound();
        item.Active = false; item.DeletedAtUtc = DateTime.UtcNow; item.DeleteReason = Trim(request.Reason, 500);
        await WriteManifestAtomicAsync(voucherId, items, HttpContext.RequestAborted);
        _audit.Add(Session(), HttpContext, "voucher_attachments", item.Id, "DELETE", new { Active = true }, new { Active = false }, item.DeleteReason);
        await _audit.SaveChangesAsync();
        return Ok();
    }

    private string Folder(long voucherId) => Path.Combine(_environment.ContentRootPath, "App_Data", "attachments", "vouchers", Session().Company_ID, Session().Branch_ID.ToString(), Session().Year_ID.ToString(), voucherId.ToString());
    private async Task<List<AttachmentRecord>> ReadAsync(long voucherId)
    {
        var file = Path.Combine(Folder(voucherId), "manifest.json");
        if (!System.IO.File.Exists(file)) return new();
        await using var stream = System.IO.File.OpenRead(file);
        return await JsonSerializer.DeserializeAsync<List<AttachmentRecord>>(stream) ?? new();
    }
    private async Task WriteManifestAtomicAsync(long voucherId, List<AttachmentRecord> items, CancellationToken cancellationToken)
    {
        var folder = Folder(voucherId); Directory.CreateDirectory(folder);
        var manifest = Path.Combine(folder, "manifest.json");
        var temporaryManifest = manifest + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await using (var stream = System.IO.File.Create(temporaryManifest))
            {
                await JsonSerializer.SerializeAsync(stream, items, cancellationToken: cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }
            System.IO.File.Move(temporaryManifest, manifest, overwrite: true);
        }
        finally
        {
            if (System.IO.File.Exists(temporaryManifest)) System.IO.File.Delete(temporaryManifest);
        }
    }
    private static string? Trim(string? value, int max) => string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(max, value.Trim().Length)];
    public sealed class AttachmentReasonRequest { public string Reason { get; set; } = string.Empty; }
    public sealed class AttachmentRecord { public string Id { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; public string Extension { get; set; } = string.Empty; public string? ContentType { get; set; } public long Bytes { get; set; } public string? Notes { get; set; } public string UploadedBy { get; set; } = string.Empty; public DateTime UploadedAtUtc { get; set; } public bool Active { get; set; } public AttachmentScanStatus ScanStatus { get; set; } = AttachmentScanStatus.Clean; public string? ScanMessage { get; set; } public DateTime? DeletedAtUtc { get; set; } public string? DeleteReason { get; set; } }
}

public enum AttachmentScanStatus { Pending, Clean, Rejected }
