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
    private const long MaxBytes = 10 * 1024 * 1024;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".pdf", ".png", ".jpg", ".jpeg", ".xlsx", ".docx" };

    public VoucherAttachmentsController(AppDbContext db, ScreenAuthorizationService authorization, AuditTrailService audit, IWebHostEnvironment environment)
    { _db = db; _authorization = authorization; _audit = audit; _environment = environment; }

    private ServerSession Session() => HttpContext.Items["ServerSession"] as ServerSession
        ?? throw new InvalidOperationException("جلسة الخادم غير متاحة.");

    private async Task<IActionResult?> AuthorizeAsync(long voucherId, ScreenOperation operation)
    {
        var session = Session();
        var voucher = await _db.Financial_Voucher_Headers.AsNoTracking()
            .Where(x => x.Voucher_ID == voucherId && x.Is_Active)
            .Select(x => new { x.Voucher_ID, x.Branch_ID, x.Fiscal_Year_ID, x.Voucher_Type_ID, x.Voucher_No })
            .SingleOrDefaultAsync();
        if (voucher == null || voucher.Branch_ID != session.Branch_ID || voucher.Fiscal_Year_ID != session.Year_ID)
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
        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension)) return BadRequest(new { message = "نوع الملف غير مسموح." });

        var attachment = new AttachmentRecord { Id = Guid.NewGuid().ToString("N"), Name = Path.GetFileName(file.FileName), Extension = extension.ToLowerInvariant(), Bytes = file.Length, Notes = Trim(notes, 500), UploadedAtUtc = DateTime.UtcNow, UploadedBy = Session().User_ID.ToString(), Active = true };
        var directory = Folder(voucherId); Directory.CreateDirectory(directory);
        var target = Path.Combine(directory, attachment.Id + attachment.Extension);
        await using (var output = System.IO.File.Create(target)) await file.CopyToAsync(output);
        var items = await ReadAsync(voucherId); items.Add(attachment); await WriteAsync(voucherId, items);
        _audit.Add(Session(), HttpContext, "voucher_attachments", attachment.Id, "CREATE", null, new { voucherId, attachment.Name, attachment.Bytes }, attachment.Notes);
        await _audit.SaveChangesAsync();
        return Ok(attachment);
    }

    [HttpGet("{attachmentId}/download")]
    public async Task<IActionResult> Download(long voucherId, string attachmentId)
    {
        var denial = await AuthorizeAsync(voucherId, ScreenOperation.Export); if (denial != null) return denial;
        var item = (await ReadAsync(voucherId)).SingleOrDefault(x => x.Id == attachmentId && x.Active);
        if (item == null) return NotFound();
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
        await WriteAsync(voucherId, items);
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
    private async Task WriteAsync(long voucherId, List<AttachmentRecord> items)
    {
        var folder = Folder(voucherId); Directory.CreateDirectory(folder);
        await using var stream = System.IO.File.Create(Path.Combine(folder, "manifest.json"));
        await JsonSerializer.SerializeAsync(stream, items);
    }
    private static string? Trim(string? value, int max) => string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(max, value.Trim().Length)];
    public sealed class AttachmentReasonRequest { public string Reason { get; set; } = string.Empty; }
    public sealed class AttachmentRecord { public string Id { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; public string Extension { get; set; } = string.Empty; public long Bytes { get; set; } public string? Notes { get; set; } public string UploadedBy { get; set; } = string.Empty; public DateTime UploadedAtUtc { get; set; } public bool Active { get; set; } public DateTime? DeletedAtUtc { get; set; } public string? DeleteReason { get; set; } }
}
