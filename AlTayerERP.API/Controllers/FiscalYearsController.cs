using AlTayerERP.API.DTOs;
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

        // تستخدم شاشة الدخول companyId لعزل سنوات كل شركة عن الأخرى. لتجنب عرض سنوات الشركات الأخرى.
        [HttpGet]
        public async Task<IActionResult> GetFiscalYears([FromQuery] string? companyId, [FromQuery] bool includeClosed = false)
        {
            var query = _context.Fiscal_Years.AsNoTracking().AsQueryable();

            // لا تُرجع إلا سنوات الشركة المحددة عند الاستدعاء من شاشة الدخول.
            if (!string.IsNullOrWhiteSpace(companyId))
                query = query.Where(x => x.Company_ID == companyId.Trim());

            // لا يسمح بالدخول إلى سنة مقفلة أو موقوفة.
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
            if (dto == null || string.IsNullOrWhiteSpace(dto.Company_ID) ||
                string.IsNullOrWhiteSpace(dto.Year_Name))
                return BadRequest("الشركة واسم السنة المالية مطلوبان.");

            if (dto.End_Date < dto.Start_Date)
                return BadRequest("تاريخ نهاية السنة لا يمكن أن يسبق تاريخ البداية.");

            var existing = await _context.Fiscal_Years.AnyAsync(x =>
                x.Company_ID == dto.Company_ID.Trim() &&
                x.Year_Name == dto.Year_Name.Trim());

            if (existing)
                return BadRequest("اسم السنة المالية مستخدم مسبقاً داخل الشركة.");

            var fiscalYear = new FiscalYear
            {
                Company_ID = dto.Company_ID.Trim(),
                Year_Name = dto.Year_Name.Trim(),
                Start_Date = dto.Start_Date,
                End_Date = dto.End_Date,
                Is_Default = dto.Is_Default,
                Is_Closed = dto.Is_Closed,
                Is_Active = dto.Is_Active,
                Created_At = DateTime.Now
            };

            // لا يوجد أكثر من سنة افتراضية واحدة لكل شركة.
            if (fiscalYear.Is_Default)
            {
                var defaults = await _context.Fiscal_Years
                    .Where(x => x.Company_ID == fiscalYear.Company_ID && x.Is_Default)
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
            if (dto == null || string.IsNullOrWhiteSpace(dto.Company_ID) || string.IsNullOrWhiteSpace(dto.Year_Name))
                return BadRequest("الشركة واسم السنة المالية مطلوبان.");

            if (dto.End_Date < dto.Start_Date)
                return BadRequest("تاريخ نهاية السنة لا يمكن أن يسبق تاريخ البداية.");

            var fiscalYear = await _context.Fiscal_Years.FindAsync(id);
            if (fiscalYear == null) return NotFound("السنة المالية غير موجودة.");

            fiscalYear.Company_ID = dto.Company_ID.Trim();
            fiscalYear.Year_Name = dto.Year_Name.Trim();
            fiscalYear.Start_Date = dto.Start_Date;
            fiscalYear.End_Date = dto.End_Date;
            fiscalYear.Is_Default = dto.Is_Default;
            fiscalYear.Is_Closed = dto.Is_Closed;
            fiscalYear.Is_Active = dto.Is_Active;
            fiscalYear.Updated_At = DateTime.Now;

            if (fiscalYear.Is_Default)
            {
                var defaults = await _context.Fiscal_Years
                    .Where(x => x.Company_ID == fiscalYear.Company_ID && x.Fiscal_Year_ID != id && x.Is_Default)
                    .ToListAsync();
                defaults.ForEach(x => x.Is_Default = false);
            }

            await _context.SaveChangesAsync();
            return Ok(fiscalYear);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFiscalYear(int id)
        {
            var fiscalYear = await _context.Fiscal_Years.FindAsync(id);
            if (fiscalYear == null) return NotFound("السنة المالية غير موجودة.");

            // الإيقاف يحافظ على سلامة القيود المحاسبية بدلاً من الحذف الفعلي.
            fiscalYear.Is_Active = false;
            fiscalYear.Updated_At = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم إيقاف السنة المالية. لا تحذف السجلات المالية تاريخياً." });
        }
    }
}