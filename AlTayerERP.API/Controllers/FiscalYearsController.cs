using AlTayerERP.API.DTOs;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AlTayerERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FiscalYearsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public FiscalYearsController(AppDbContext context) => _context = context;

        private ServerSession? Session =>
            HttpContext.Items["ServerSession"] as ServerSession;

        // إدارة السنوات المالية عملية حساسة؛ لا يكفي وجود جلسة صحيحة.
        // الشركة تؤخذ دائماً من جلسة الخادم ولا تقبل من سطح المكتب.
        private IActionResult? RequireSystemAdmin(out ServerSession? session)
        {
            session = Session;
            if (session == null)
                return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة. سجل الدخول من جديد." });

            return session.Is_System_Admin ? null : Forbid();
        }

        // قائمة دخول محدودة قبل إنشاء الجلسة: الشركة مطلوبة وتُرجع السنوات النشطة غير المقفلة فقط.
        [HttpGet("Lookup")]
        public async Task<IActionResult> GetLoginLookup([FromQuery] string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest("معرف الشركة مطلوب.");

            var normalizedCompanyId = companyId.Trim();
            var companyIsActive = await _context.Companies
                .AsNoTracking()
                .AnyAsync(x => x.Company_ID == normalizedCompanyId && x.Is_Active);

            if (!companyIsActive)
                return NotFound("الشركة غير موجودة أو غير فعالة.");

            var years = await _context.Fiscal_Years
                .AsNoTracking()
                .Where(x => x.Company_ID == normalizedCompanyId && x.Is_Active && !x.Is_Closed)
                .OrderByDescending(x => x.Is_Default)
                .ThenByDescending(x => x.Start_Date)
                .Select(x => new
                {
                    x.Fiscal_Year_ID,
                    x.Year_Name,
                    x.Is_Default
                })
                .ToListAsync();

            return Ok(years);
        }

        // شاشة الإدارة تقرأ فقط سنوات الشركة الموجودة في جلسة الخادم.
        [HttpGet]
        public async Task<IActionResult> GetFiscalYears([FromQuery] bool includeClosed = false)
        {
            var accessError = RequireSystemAdmin(out var session);
            if (accessError != null || session == null)
                return accessError!;

            var query = _context.Fiscal_Years
                .AsNoTracking()
                .Where(x => x.Company_ID == session.Company_ID);

            if (!includeClosed)
                query = query.Where(x => x.Is_Active && !x.Is_Closed);

            return Ok(await query
                .OrderByDescending(x => x.Is_Default)
                .ThenByDescending(x => x.Start_Date)
                .ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> CreateFiscalYear([FromBody] CreateFiscalYearDto dto)
        {
            var accessError = RequireSystemAdmin(out var session);
            if (accessError != null || session == null)
                return accessError!;

            if (dto == null || string.IsNullOrWhiteSpace(dto.Year_Name))
                return BadRequest("اسم السنة المالية مطلوب.");

            if (dto.End_Date < dto.Start_Date)
                return BadRequest("تاريخ نهاية السنة لا يمكن أن يسبق تاريخ البداية.");

            var companyId = session.Company_ID;
            var existing = await _context.Fiscal_Years.AnyAsync(x =>
                x.Company_ID == companyId && x.Year_Name == dto.Year_Name.Trim());

            if (existing)
                return BadRequest("اسم السنة المالية مستخدم مسبقاً داخل الشركة.");

            var fiscalYear = new FiscalYear
            {
                // لا تستخدم Company_ID القادم من العميل؛ يمنع ذلك إنشاء سنة في شركة أخرى.
                Company_ID = companyId,
                Year_Name = dto.Year_Name.Trim(),
                Start_Date = dto.Start_Date,
                End_Date = dto.End_Date,
                Is_Default = dto.Is_Default,
                // إقفال أو إيقاف السنة لا يتم في حفظ عام؛ يخصص له إجراء مدقق مستقل.
                Is_Closed = false,
                Is_Active = true,
                Created_At = DateTime.Now
            };

            // لا يوجد أكثر من سنة افتراضية واحدة لكل شركة.
            if (fiscalYear.Is_Default)
            {
                var defaults = await _context.Fiscal_Years
                    .Where(x => x.Company_ID == companyId && x.Is_Default)
                    .ToListAsync();
                defaults.ForEach(x => x.Is_Default = false);
            }

            _context.Fiscal_Years.Add(fiscalYear);
            await _context.SaveChangesAsync();
            return Ok(fiscalYear);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFiscalYear(int id, [FromBody] CreateFiscalYearDto dto)
        {
            var accessError = RequireSystemAdmin(out var session);
            if (accessError != null || session == null)
                return accessError!;

            if (dto == null || string.IsNullOrWhiteSpace(dto.Year_Name))
                return BadRequest("اسم السنة المالية مطلوب.");

            if (dto.End_Date < dto.Start_Date)
                return BadRequest("تاريخ نهاية السنة لا يمكن أن يسبق تاريخ البداية.");

            // يمنع تعديل سنة تابعة لشركة أخرى أو نقلها بين الشركات.
            var fiscalYear = await _context.Fiscal_Years.FirstOrDefaultAsync(x =>
                x.Fiscal_Year_ID == id && x.Company_ID == session.Company_ID);
            if (fiscalYear == null)
                return NotFound("السنة المالية غير موجودة ضمن الشركة الحالية.");

            var nameExists = await _context.Fiscal_Years.AnyAsync(x =>
                x.Company_ID == session.Company_ID &&
                x.Year_Name == dto.Year_Name.Trim() &&
                x.Fiscal_Year_ID != id);
            if (nameExists)
                return BadRequest("اسم السنة المالية مستخدم مسبقاً داخل الشركة.");

            if (fiscalYear.Is_Closed)
                return Conflict("لا يمكن تعديل سنة مالية مقفلة. أعد فتحها بإجراء مدقق أولاً.");

            fiscalYear.Year_Name = dto.Year_Name.Trim();
            fiscalYear.Start_Date = dto.Start_Date;
            fiscalYear.End_Date = dto.End_Date;
            fiscalYear.Is_Default = dto.Is_Default;
            // تحفظ دورة الحياة الحالية؛ لا يسمح DTO العميل بإغلاق أو إيقاف السنة مباشرة.
            fiscalYear.Is_Closed = false;
            fiscalYear.Is_Active = true;
            fiscalYear.Updated_At = DateTime.Now;

            if (fiscalYear.Is_Default)
            {
                var defaults = await _context.Fiscal_Years
                    .Where(x => x.Company_ID == session.Company_ID &&
                                x.Fiscal_Year_ID != id &&
                                x.Is_Default)
                    .ToListAsync();
                defaults.ForEach(x => x.Is_Default = false);
            }

            await _context.SaveChangesAsync();
            return Ok(fiscalYear);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFiscalYear(int id)
        {
            var accessError = RequireSystemAdmin(out var session);
            if (accessError != null || session == null)
                return accessError!;

            var fiscalYear = await _context.Fiscal_Years.FirstOrDefaultAsync(x =>
                x.Fiscal_Year_ID == id && x.Company_ID == session.Company_ID);
            if (fiscalYear == null)
                return NotFound("السنة المالية غير موجودة ضمن الشركة الحالية.");

            // الإيقاف يحافظ على سلامة القيود المحاسبية بدلاً من الحذف الفعلي.
            fiscalYear.Is_Active = false;
            fiscalYear.Updated_At = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم إيقاف السنة المالية. لا تحذف السجلات المالية تاريخياً." });
        }
    }
}