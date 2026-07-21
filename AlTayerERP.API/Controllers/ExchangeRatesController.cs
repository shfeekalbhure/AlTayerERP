using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// سجل أسعار الصرف التاريخية للشركة الحالية.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public sealed class ExchangeRatesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExchangeRatesController(AppDbContext context) => _context = context;

        private ServerSession? Session() =>
            HttpContext.Items["ServerSession"] as ServerSession;

        private IActionResult? RequireSystemAdmin(out ServerSession? session)
        {
            session = Session();
            if (session == null)
                return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة. سجل الدخول من جديد." });

            return session.Is_System_Admin ? null : Forbid();
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var accessError = RequireSystemAdmin(out var session);
            if (accessError != null || session == null)
                return accessError!;

            var rates = await _context.Exchange_Rates
                .AsNoTracking()
                .Where(x => x.Company_ID == session.Company_ID)
                .OrderByDescending(x => x.Rate_Date)
                .ThenBy(x => x.Currency_Code)
                .ToListAsync();

            return Ok(rates);
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SaveExchangeRateRequest request)
        {
            var accessError = RequireSystemAdmin(out var session);
            if (accessError != null || session == null)
                return accessError!;

            if (request == null || string.IsNullOrWhiteSpace(request.Currency_Code))
                return BadRequest(new { message = "كود العملة مطلوب." });

            var code = request.Currency_Code.Trim().ToUpperInvariant();
            if (request.Exchange_Rate <= 0)
                return BadRequest(new { message = "سعر الصرف يجب أن يكون أكبر من صفر." });

            decimal? minRate = request.Min_Rate > 0 ? request.Min_Rate : null;
            decimal? maxRate = request.Max_Rate > 0 ? request.Max_Rate : null;
            if (minRate.HasValue && maxRate.HasValue && minRate > maxRate)
                return BadRequest(new { message = "الحد الأدنى لا يمكن أن يتجاوز الحد الأعلى." });

            if (minRate.HasValue && request.Exchange_Rate < minRate ||
                maxRate.HasValue && request.Exchange_Rate > maxRate)
            {
                return BadRequest(new { message = "سعر الصرف خارج الحد الأدنى أو الأعلى المحدد." });
            }

            var currency = await _context.Currencies
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Company_ID == session.Company_ID &&
                                          x.Currency_Code == code &&
                                          x.Is_Active);
            if (currency == null)
                return BadRequest(new { message = "العملة غير موجودة أو غير فعالة في الشركة الحالية." });

            if (currency.Is_Local_Currency && request.Exchange_Rate != 1)
                return BadRequest(new { message = "سعر صرف العملة المحلية يجب أن يساوي 1." });

            ExchangeRate rate;
            if (request.Exchange_Rate_ID > 0)
            {
                var existing = await _context.Exchange_Rates.FirstOrDefaultAsync(x =>
                    x.Exchange_Rate_ID == request.Exchange_Rate_ID &&
                    x.Company_ID == session.Company_ID);
                if (existing == null)
                    return NotFound(new { message = "سجل سعر الصرف غير موجود ضمن الشركة الحالية." });

                rate = existing;
            }
            else
            {
                rate = new ExchangeRate
                {
                    Company_ID = session.Company_ID,
                    Created_At = DateTime.Now
                };
                _context.Exchange_Rates.Add(rate);
            }

            var rateDate = request.Rate_Date.Date;
            var duplicate = await _context.Exchange_Rates.AnyAsync(x =>
                x.Company_ID == session.Company_ID &&
                x.Currency_Code == code &&
                x.Rate_Date == rateDate &&
                x.Exchange_Rate_ID != rate.Exchange_Rate_ID);
            if (duplicate)
                return BadRequest(new { message = "يوجد سعر صرف لهذه العملة في تاريخ السريان نفسه." });

            if (request.Is_Default)
            {
                var defaults = await _context.Exchange_Rates
                    .Where(x => x.Company_ID == session.Company_ID &&
                                x.Currency_Code == code &&
                                x.Exchange_Rate_ID != rate.Exchange_Rate_ID &&
                                x.Is_Default)
                    .ToListAsync();
                defaults.ForEach(x => x.Is_Default = false);
            }

            rate.Currency_Code = code;
            rate.Rate_Date = rateDate;
            rate.Exchange_Rate_Value = request.Exchange_Rate;
            rate.Min_Rate = minRate;
            rate.Max_Rate = maxRate;
            rate.Is_Default = request.Is_Default;
            rate.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
            rate.Is_Active = request.Is_Active;
            rate.Updated_At = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = request.Exchange_Rate_ID > 0 ? "تم تعديل سعر الصرف." : "تمت إضافة سعر الصرف.",
                rate.Exchange_Rate_ID
            });
        }
    }

    public sealed class SaveExchangeRateRequest
    {
        public int Exchange_Rate_ID { get; set; }
        public string Currency_Code { get; set; } = string.Empty;
        public DateTime Rate_Date { get; set; }
        public decimal Exchange_Rate { get; set; }
        public decimal Min_Rate { get; set; }
        public decimal Max_Rate { get; set; }
        public bool Is_Default { get; set; }
        public string? Notes { get; set; }
        public bool Is_Active { get; set; } = true;
    }
}
