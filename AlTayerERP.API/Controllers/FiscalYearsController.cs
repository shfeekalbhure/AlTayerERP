using AlTayerERP.API.DTOs;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlTayerERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FiscalYearsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FiscalYearsController(AppDbContext context)
        {
            _context = context;
        }

        // ======================================================
        // 1. جلب كل السنوات المالية مرتبة تنازلياً (GET: api/FiscalYears)
        // ======================================================
        [HttpGet]
        public async Task<IActionResult> GetFiscalYears()
        {
            var years = await _context.Fiscal_Years
                .OrderByDescending(x => x.Start_Date)
                .ToListAsync();

            return Ok(years);
        }

        // ======================================================
        // 2. حفظ سنة مالية جديدة (POST: api/FiscalYears)
        // ======================================================
        [HttpPost]
        public async Task<IActionResult> CreateFiscalYear([FromBody] CreateFiscalYearDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Year_Name))
                return BadRequest("اسم السنة المالية مطلوب.");

            var fiscalYear = new FiscalYear
            {
                Company_ID = dto.Company_ID,
                Year_Name = dto.Year_Name.Trim(),
                Start_Date = dto.Start_Date,
                End_Date = dto.End_Date,
                Is_Default = dto.Is_Default,
                Is_Closed = dto.Is_Closed,
                Is_Active = dto.Is_Active,
                Created_At = DateTime.Now
            };

            await _context.Fiscal_Years.AddAsync(fiscalYear);
            await _context.SaveChangesAsync();

            return Ok(fiscalYear);
        }

        // ======================================================
        // 3. تعديل سنة مالية معتمدة على الـ CreateDto (PUT: api/FiscalYears/{id})
        // ======================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFiscalYear(int id, [FromBody] CreateFiscalYearDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Year_Name))
                return BadRequest("بيانات التعديل غير مكتملة أو اسم السنة فارغ.");

            var existingYear = await _context.Fiscal_Years.FindAsync(id);
            if (existingYear == null)
                return NotFound($"السنة المالية ذات الرقم {id} غير موجودة بالسيرفر.");

            // تحديث الحقول مباشرة بالقيم المحدثة
            existingYear.Company_ID = dto.Company_ID;
            existingYear.Year_Name = dto.Year_Name.Trim();
            existingYear.Start_Date = dto.Start_Date;
            existingYear.End_Date = dto.End_Date;
            existingYear.Is_Default = dto.Is_Default;
            existingYear.Is_Closed = dto.Is_Closed;
            existingYear.Is_Active = dto.Is_Active;

            _context.Fiscal_Years.Update(existingYear);
            await _context.SaveChangesAsync();

            return Ok(existingYear);
        }

        // ======================================================
        // 4. حذف سنة مالية نهائياً من النظام (DELETE: api/FiscalYears/{id})
        // ======================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFiscalYear(int id)
        {
            var fiscalYear = await _context.Fiscal_Years.FindAsync(id);
            if (fiscalYear == null)
                return NotFound($"لا يمكن الحذف، السنة المالية ذات الرقم {id} غير موجودة.");

            _context.Fiscal_Years.Remove(fiscalYear);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم حذف السنة المالية بنجاح من قاعدة البيانات." });
        }
    }
}