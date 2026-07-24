using AlTayerERP.API.DTOs;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>السنوات المالية للشركة الحالية؛ الإقفال وإعادة الفتح إجراءات مستقلة مدققة.</summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public sealed class FiscalYearsController : ControllerBase
    {
        private readonly AppDbContext _context; private readonly ScreenAuthorizationService _authorization; private readonly AuditTrailService _audit;
        public FiscalYearsController(AppDbContext context, ScreenAuthorizationService authorization, AuditTrailService audit)
        { _context = context; _authorization = authorization; _audit = audit; }
        private ServerSession? Session => HttpContext.Items["ServerSession"] as ServerSession;
        private async Task<IActionResult?> RequireAsync(ScreenOperation op)
        {
            if (Session == null) return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة." });
            return await _authorization.IsAllowedAsync(Session, "FiscalYears", op) ? null : Forbid();
        }

        /// <summary>قائمة آمنة قبل تسجيل الدخول لا تعرض إلا سنوات الشركة النشطة المفتوحة.</summary>
        [AllowAnonymous, HttpGet("Lookup")]
        public async Task<IActionResult> GetLoginLookup([FromQuery] string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId)) return BadRequest(new { message = "معرف الشركة مطلوب." });
            var company = companyId.Trim();
            if (!await _context.Companies.AsNoTracking().AnyAsync(x => x.Company_ID == company && x.Is_Active))
                return NotFound(new { message = "الشركة غير موجودة أو موقوفة." });
            return Ok(await _context.Fiscal_Years.AsNoTracking().Where(x => x.Company_ID == company && x.Is_Active && !x.Is_Closed)
                .OrderByDescending(x => x.Is_Default).ThenByDescending(x => x.Start_Date)
                .Select(x => new { x.Fiscal_Year_ID, x.Year_Name, x.Is_Default }).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> GetFiscalYears([FromQuery] bool includeClosed = false)
        {
            var error = await RequireAsync(ScreenOperation.View); if (error != null || Session == null) return error!;
            var q = _context.Fiscal_Years.AsNoTracking().Where(x => x.Company_ID == Session.Company_ID);
            if (!includeClosed) q = q.Where(x => x.Is_Active && !x.Is_Closed);
            return Ok(await q.OrderByDescending(x => x.Is_Default).ThenByDescending(x => x.Start_Date).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> CreateFiscalYear([FromBody] CreateFiscalYearDto dto)
        {
            var error = await RequireAsync(ScreenOperation.Add); if (error != null || Session == null) return error!;
            var invalid = await ValidateAsync(dto, 0); if (invalid != null) return BadRequest(new { message = invalid });
            await using var tx = await _context.Database.BeginTransactionAsync();
            if (dto.Is_Default) await _context.Fiscal_Years.Where(x => x.Company_ID == Session.Company_ID && x.Is_Default)
                .ExecuteUpdateAsync(x => x.SetProperty(v => v.Is_Default, false).SetProperty(v => v.Updated_At, DateTime.UtcNow));
            var row = new FiscalYear { Company_ID = Session.Company_ID, Year_Name = dto.Year_Name.Trim(), Start_Date = dto.Start_Date.Date, End_Date = dto.End_Date.Date, Is_Default = dto.Is_Default, Is_Active = true, Is_Closed = false, Created_At = DateTime.UtcNow };
            _context.Fiscal_Years.Add(row);
            _audit.Add(Session, HttpContext, "fiscal_years", dto.Year_Name.Trim(), "CREATE", null, new { row.Year_Name, row.Start_Date, row.End_Date, row.Is_Default });
            await _context.SaveChangesAsync(); await tx.CommitAsync();
            return CreatedAtAction(nameof(GetFiscalYears), new { id = row.Fiscal_Year_ID }, row);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateFiscalYear(int id, [FromBody] CreateFiscalYearDto dto)
        {
            var error = await RequireAsync(ScreenOperation.Edit); if (error != null || Session == null) return error!;
            var row = await _context.Fiscal_Years.FirstOrDefaultAsync(x => x.Fiscal_Year_ID == id && x.Company_ID == Session.Company_ID);
            if (row == null) return NotFound(new { message = "السنة غير موجودة ضمن الشركة الحالية." });
            if (row.Is_Closed) return Conflict(new { message = "لا تعدّل سنة مقفلة؛ استخدم إعادة الفتح المدققة أولاً." });
            var invalid = await ValidateAsync(dto, id); if (invalid != null) return BadRequest(new { message = invalid });
            if (await _context.Financial_Voucher_Headers.AnyAsync(x => x.Fiscal_Year_ID == id))
                return Conflict(new { message = "لا يمكن تغيير تواريخ سنة لها مستندات مالية." });
            var old = new { row.Year_Name, row.Start_Date, row.End_Date, row.Is_Default };
            await using var tx = await _context.Database.BeginTransactionAsync();
            if (dto.Is_Default) await _context.Fiscal_Years.Where(x => x.Company_ID == Session.Company_ID && x.Fiscal_Year_ID != id && x.Is_Default)
                .ExecuteUpdateAsync(x => x.SetProperty(v => v.Is_Default, false).SetProperty(v => v.Updated_At, DateTime.UtcNow));
            row.Year_Name = dto.Year_Name.Trim(); row.Start_Date = dto.Start_Date.Date; row.End_Date = dto.End_Date.Date; row.Is_Default = dto.Is_Default; row.Updated_At = DateTime.UtcNow;
            _audit.Add(Session, HttpContext, "fiscal_years", id.ToString(), "UPDATE", old, new { row.Year_Name, row.Start_Date, row.End_Date, row.Is_Default });
            await _context.SaveChangesAsync(); await tx.CommitAsync(); return Ok(row);
        }

        [HttpPost("{id:int}/Close")]
        public async Task<IActionResult> Close(int id, [FromBody] FiscalLifecycleRequest request)
        {
            var error = await RequireAsync(ScreenOperation.Approve); if (error != null || Session == null) return error!;
            if (string.IsNullOrWhiteSpace(request?.Reason)) return BadRequest(new { message = "سبب الإقفال مطلوب." });
            var row = await _context.Fiscal_Years.FirstOrDefaultAsync(x => x.Fiscal_Year_ID == id && x.Company_ID == Session.Company_ID);
            if (row == null) return NotFound(new { message = "السنة غير موجودة." });
            if (await _context.Fiscal_Periods.AnyAsync(x => x.Company_ID == Session.Company_ID && x.Fiscal_Year_ID == id && x.Is_Active && !x.Is_Closed))
                return Conflict(new { message = "يجب إقفال كل الفترات النشطة قبل إقفال السنة." });
            if (await _context.Financial_Voucher_Headers.AnyAsync(x => x.Fiscal_Year_ID == id && !x.Is_Posted && x.Is_Active))
                return Conflict(new { message = "لا يمكن الإقفال مع وجود مستندات معلقة غير مرحلة." });
            row.Is_Closed = true; row.Updated_At = DateTime.UtcNow;
            _audit.Add(Session, HttpContext, "fiscal_years", id.ToString(), "CLOSE", new { Is_Closed = false }, new { Is_Closed = true }, request.Reason);
            await _context.SaveChangesAsync(); return Ok(new { message = "تم إقفال السنة المالية." });
        }

        [HttpPost("{id:int}/Reopen")]
        public async Task<IActionResult> Reopen(int id, [FromBody] FiscalLifecycleRequest request)
        {
            var error = await RequireAsync(ScreenOperation.Unapprove); if (error != null || Session == null) return error!;
            if (string.IsNullOrWhiteSpace(request?.Reason)) return BadRequest(new { message = "سبب إعادة الفتح مطلوب." });
            var row = await _context.Fiscal_Years.FirstOrDefaultAsync(x => x.Fiscal_Year_ID == id && x.Company_ID == Session.Company_ID);
            if (row == null) return NotFound(new { message = "السنة غير موجودة." });
            row.Is_Closed = false; row.Updated_At = DateTime.UtcNow;
            _audit.Add(Session, HttpContext, "fiscal_years", id.ToString(), "REOPEN", new { Is_Closed = true }, new { Is_Closed = false }, request.Reason);
            await _context.SaveChangesAsync(); return Ok(new { message = "تمت إعادة فتح السنة المالية." });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Deactivate(int id, [FromQuery] string? reason)
        {
            var error = await RequireAsync(ScreenOperation.Delete); if (error != null || Session == null) return error!;
            var row = await _context.Fiscal_Years.FirstOrDefaultAsync(x => x.Fiscal_Year_ID == id && x.Company_ID == Session.Company_ID);
            if (row == null) return NotFound(new { message = "السنة غير موجودة." });
            if (await _context.Financial_Voucher_Headers.AnyAsync(x => x.Fiscal_Year_ID == id)) return Conflict(new { message = "لا يمكن إيقاف سنة لها مستندات مالية." });
            row.Is_Active = false; row.Updated_At = DateTime.UtcNow;
            _audit.Add(Session, HttpContext, "fiscal_years", id.ToString(), "DEACTIVATE", new { Is_Active = true }, new { Is_Active = false }, reason);
            await _context.SaveChangesAsync(); return Ok(new { message = "تم إيقاف السنة دون حذف تاريخها." });
        }

        private async Task<string?> ValidateAsync(CreateFiscalYearDto? dto, int id)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Year_Name) || dto.End_Date.Date < dto.Start_Date.Date) return "اسم السنة وتواريخها الصحيحة حقول مطلوبة.";
            if (Session == null) return "الجلسة غير صالحة.";
            if (await _context.Fiscal_Years.AnyAsync(x => x.Company_ID == Session.Company_ID && x.Year_Name == dto.Year_Name.Trim() && x.Fiscal_Year_ID != id)) return "اسم السنة مستخدم مسبقاً.";
            return null;
        }
    }
    public sealed class FiscalLifecycleRequest { public string? Reason { get; set; } }
}