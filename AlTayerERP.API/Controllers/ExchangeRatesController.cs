using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>سجل أسعار الصرف التاريخية للشركة الحالية، مع تدقيق وتفويض شاشة العملات.</summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public sealed class ExchangeRatesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ScreenAuthorizationService _authorization;
        private readonly AuditTrailService _audit;

        public ExchangeRatesController(AppDbContext context, ScreenAuthorizationService authorization, AuditTrailService audit)
        {
            _context = context;
            _authorization = authorization;
            _audit = audit;
        }

        private ServerSession? Session => HttpContext.Items["ServerSession"] as ServerSession;
        private async Task<IActionResult?> RequireAsync(ScreenOperation operation)
        {
            if (Session == null) return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة." });
            return await _authorization.IsAllowedAsync(Session, "ExchangeRates", operation) ? null : Forbid();
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var error = await RequireAsync(ScreenOperation.View);
            if (error != null || Session == null) return error!;
            return Ok(await _context.Exchange_Rates.AsNoTracking()
                .Where(x => x.Company_ID == Session.Company_ID).OrderByDescending(x => x.Rate_Date)
                .ThenBy(x => x.Currency_Code).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SaveExchangeRateRequest request)
        {
            var operation = request.Exchange_Rate_ID > 0 ? ScreenOperation.Edit : ScreenOperation.Add;
            var error = await RequireAsync(operation);
            if (error != null || Session == null) return error!;

            if (string.IsNullOrWhiteSpace(request.Currency_Code) || request.Exchange_Rate <= 0)
                return BadRequest(new { message = "كود العملة وسعر صرف موجب حقول مطلوبة." });

            var code = request.Currency_Code.Trim().ToUpperInvariant();
            decimal? min = request.Min_Rate > 0 ? request.Min_Rate : null;
            decimal? max = request.Max_Rate > 0 ? request.Max_Rate : null;
            if (min.HasValue && max.HasValue && min > max || min.HasValue && request.Exchange_Rate < min || max.HasValue && request.Exchange_Rate > max)
                return BadRequest(new { message = "سعر الصرف خارج الحدود المسموح بها." });

            var currency = await _context.Currencies.AsNoTracking().FirstOrDefaultAsync(x =>
                x.Company_ID == Session.Company_ID && x.Currency_Code == code && x.Is_Active);
            if (currency == null) return BadRequest(new { message = "العملة غير موجودة أو موقوفة في الشركة الحالية." });
            if (currency.Is_Local_Currency && request.Exchange_Rate != 1m)
                return BadRequest(new { message = "سعر صرف العملة المحلية يجب أن يساوي 1." });

            var rate = request.Exchange_Rate_ID > 0
                ? await _context.Exchange_Rates.FirstOrDefaultAsync(x => x.Exchange_Rate_ID == request.Exchange_Rate_ID && x.Company_ID == Session.Company_ID)
                : null;
            if (request.Exchange_Rate_ID > 0 && rate == null) return NotFound(new { message = "سجل سعر الصرف غير موجود." });

            var rateDate = request.Rate_Date == default ? DateTime.UtcNow.Date : request.Rate_Date.Date;
            if (await _context.Exchange_Rates.AnyAsync(x => x.Company_ID == Session.Company_ID && x.Currency_Code == code && x.Rate_Date == rateDate && x.Exchange_Rate_ID != request.Exchange_Rate_ID))
                return Conflict(new { message = "يوجد سعر صرف للعملة نفسها في تاريخ السريان ذاته." });

            await using var tx = await _context.Database.BeginTransactionAsync();
            if (request.Is_Default)
                await _context.Exchange_Rates.Where(x => x.Company_ID == Session.Company_ID && x.Currency_Code == code && x.Is_Default && x.Exchange_Rate_ID != request.Exchange_Rate_ID)
                    .ExecuteUpdateAsync(x => x.SetProperty(v => v.Is_Default, false).SetProperty(v => v.Updated_At, DateTime.UtcNow));

            var old = rate == null ? null : new { rate.Currency_Code, rate.Rate_Date, rate.Exchange_Rate_Value, rate.Is_Active };
            if (rate == null)
            {
                rate = new ExchangeRate { Company_ID = Session.Company_ID, Created_At = DateTime.UtcNow };
                _context.Exchange_Rates.Add(rate);
            }
            rate.Currency_Code = code; rate.Rate_Date = rateDate; rate.Exchange_Rate_Value = request.Exchange_Rate;
            rate.Min_Rate = min; rate.Max_Rate = max; rate.Is_Default = request.Is_Default;
            rate.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
            rate.Is_Active = request.Is_Active; rate.Updated_At = DateTime.UtcNow;
            _audit.Add(Session, HttpContext, "exchange_rates", request.Exchange_Rate_ID > 0 ? request.Exchange_Rate_ID.ToString() : $"{code}:{rateDate:yyyyMMdd}",
                request.Exchange_Rate_ID > 0 ? "UPDATE" : "CREATE", old, new { rate.Currency_Code, rate.Rate_Date, rate.Exchange_Rate_Value, rate.Is_Active });
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            return Ok(new { message = request.Exchange_Rate_ID > 0 ? "تم تعديل سعر الصرف." : "تمت إضافة سعر الصرف.", rate.Exchange_Rate_ID });
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