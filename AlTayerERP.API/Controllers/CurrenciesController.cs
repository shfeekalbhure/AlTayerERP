using AlTayerERP.API.DTOs;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// إدارة العملات للشركة الموجودة في الجلسة الموثوقة فقط.
    /// Company_ID وحقول التدقيق القادمة من العميل لا يعتمد عليها الخادم.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public sealed class CurrenciesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ScreenAuthorizationService _authorization;
        private readonly AuditTrailService _audit;

        public CurrenciesController(AppDbContext context, ScreenAuthorizationService authorization, AuditTrailService audit)
        {
            _context = context;
            _authorization = authorization;
            _audit = audit;
        }

        private ServerSession? Session => HttpContext.Items["ServerSession"] as ServerSession;

        private async Task<IActionResult?> RequireAsync(ScreenOperation operation)
        {
            if (Session == null)
                return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة." });

            return await _authorization.IsAllowedAsync(Session, "Currencies", operation)
                ? null
                : Forbid();
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrencies()
        {
            var error = await RequireAsync(ScreenOperation.View);
            if (error != null || Session == null) return error!;

            var data = await _context.Currencies.AsNoTracking()
                .Where(x => x.Company_ID == Session.Company_ID)
                .OrderByDescending(x => x.Is_Local_Currency).ThenBy(x => x.Currency_Code)
                .Select(x => new
                {
                    x.Currency_ID, x.Company_ID, x.Currency_Code, x.Currency_Name_AR, x.Currency_Name_EN,
                    x.Currency_Symbol, x.Decimal_Places, x.Exchange_Rate, x.Min_Exchange_Rate,
                    x.Max_Exchange_Rate, x.Is_Local_Currency, x.Is_Default, x.Is_Active, x.Notes,
                    x.Created_By, x.Created_At, x.Updated_By, x.Updated_At
                }).ToListAsync();
            return Ok(data);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCurrencyById(int id)
        {
            var error = await RequireAsync(ScreenOperation.View);
            if (error != null || Session == null) return error!;

            var currency = await _context.Currencies.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Currency_ID == id && x.Company_ID == Session.Company_ID);
            return currency == null ? NotFound(new { message = "العملة غير موجودة ضمن الشركة الحالية." }) : Ok(currency);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCurrency([FromBody] CreateCurrencyDto dto)
        {
            var error = await RequireAsync(ScreenOperation.Add);
            if (error != null || Session == null) return error!;
            var validation = Validate(dto);
            if (validation != null) return BadRequest(new { message = validation });

            var code = dto.Currency_Code.Trim().ToUpperInvariant();
            if (await _context.Currencies.AnyAsync(x => x.Company_ID == Session.Company_ID && x.Currency_Code == code))
                return Conflict(new { message = "كود العملة مستخدم مسبقاً في الشركة الحالية." });

            await using var tx = await _context.Database.BeginTransactionAsync();
            await ClearFlagsAsync(dto);
            var row = Build(dto, Session.Company_ID, code);
            row.Created_By = Session.User_ID.ToString();
            row.Created_At = DateTime.UtcNow;
            _context.Currencies.Add(row);
            _audit.Add(Session, HttpContext, "currencies", row.Currency_ID.ToString(), "CREATE", null,
                new { row.Currency_Code, row.Currency_Name_AR, row.Is_Local_Currency, row.Is_Active });
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            return CreatedAtAction(nameof(GetCurrencyById), new { id = row.Currency_ID }, row);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCurrency(int id, [FromBody] CreateCurrencyDto dto)
        {
            var error = await RequireAsync(ScreenOperation.Edit);
            if (error != null || Session == null) return error!;
            var validation = Validate(dto);
            if (validation != null) return BadRequest(new { message = validation });

            var row = await _context.Currencies.FirstOrDefaultAsync(x => x.Currency_ID == id && x.Company_ID == Session.Company_ID);
            if (row == null) return NotFound(new { message = "العملة غير موجودة ضمن الشركة الحالية." });

            var code = dto.Currency_Code.Trim().ToUpperInvariant();
            if (await _context.Currencies.AnyAsync(x => x.Company_ID == Session.Company_ID && x.Currency_Code == code && x.Currency_ID != id))
                return Conflict(new { message = "كود العملة مستخدم مسبقاً في الشركة الحالية." });

            var old = new { row.Currency_Code, row.Currency_Name_AR, row.Is_Local_Currency, row.Is_Default, row.Is_Active };
            await using var tx = await _context.Database.BeginTransactionAsync();
            await ClearFlagsAsync(dto, id);
            Apply(row, dto, code);
            row.Updated_By = Session.User_ID.ToString();
            row.Updated_At = DateTime.UtcNow;
            _audit.Add(Session, HttpContext, "currencies", row.Currency_ID.ToString(), "UPDATE", old,
                new { row.Currency_Code, row.Currency_Name_AR, row.Is_Local_Currency, row.Is_Default, row.Is_Active });
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            return Ok(row);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Deactivate(int id, [FromQuery] string? reason)
        {
            var error = await RequireAsync(ScreenOperation.Delete);
            if (error != null || Session == null) return error!;

            var row = await _context.Currencies.FirstOrDefaultAsync(x => x.Currency_ID == id && x.Company_ID == Session.Company_ID);
            if (row == null) return NotFound(new { message = "العملة غير موجودة ضمن الشركة الحالية." });
            if (row.Is_Local_Currency) return BadRequest(new { message = "لا يمكن إيقاف العملة المحلية؛ عيّن عملة محلية بديلة أولاً." });

            row.Is_Active = false;
            row.Updated_By = Session.User_ID.ToString();
            row.Updated_At = DateTime.UtcNow;
            _audit.Add(Session, HttpContext, "currencies", row.Currency_ID.ToString(), "DEACTIVATE",
                new { Is_Active = true }, new { Is_Active = false }, reason);
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم إيقاف العملة دون حذف تاريخها المالي." });
        }

        private async Task ClearFlagsAsync(CreateCurrencyDto dto, int excludingId = 0)
        {
            if (Session == null) return;
            if (dto.Is_Local_Currency)
                await _context.Currencies.Where(x => x.Company_ID == Session.Company_ID && x.Is_Local_Currency && x.Currency_ID != excludingId)
                    .ExecuteUpdateAsync(x => x.SetProperty(v => v.Is_Local_Currency, false).SetProperty(v => v.Updated_At, DateTime.UtcNow));
            if (dto.Is_Default)
                await _context.Currencies.Where(x => x.Company_ID == Session.Company_ID && x.Is_Default && x.Currency_ID != excludingId)
                    .ExecuteUpdateAsync(x => x.SetProperty(v => v.Is_Default, false).SetProperty(v => v.Updated_At, DateTime.UtcNow));
        }

        private static Currency Build(CreateCurrencyDto dto, string companyId, string code)
        {
            var row = new Currency { Company_ID = companyId };
            Apply(row, dto, code);
            return row;
        }

        private static void Apply(Currency row, CreateCurrencyDto dto, string code)
        {
            row.Currency_Code = code;
            row.Currency_Name_AR = dto.Currency_Name_AR.Trim();
            row.Currency_Name_EN = NullIfWhiteSpace(dto.Currency_Name_EN);
            row.Currency_Symbol = NullIfWhiteSpace(dto.Currency_Symbol);
            row.Decimal_Places = dto.Decimal_Places;
            row.Is_Local_Currency = dto.Is_Local_Currency;
            row.Is_Default = dto.Is_Default;
            row.Exchange_Rate = dto.Is_Local_Currency ? 1m : dto.Exchange_Rate;
            row.Min_Exchange_Rate = dto.Is_Local_Currency ? 1m : dto.Min_Exchange_Rate;
            row.Max_Exchange_Rate = dto.Is_Local_Currency ? 1m : dto.Max_Exchange_Rate;
            row.Is_Active = dto.Is_Active;
            row.Notes = NullIfWhiteSpace(dto.Notes);
        }

        private static string? Validate(CreateCurrencyDto? dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Currency_Code) || string.IsNullOrWhiteSpace(dto.Currency_Name_AR))
                return "كود العملة واسمها العربي حقول مطلوبة.";
            if (dto.Currency_Code.Trim().Length > 20 || dto.Decimal_Places < 0 || dto.Decimal_Places > 6)
                return "كود العملة غير صالح أو عدد المنازل العشرية يجب أن يكون بين 0 و6.";
            if (dto.Exchange_Rate <= 0 || dto.Min_Exchange_Rate <= 0 || dto.Max_Exchange_Rate <= 0 || dto.Min_Exchange_Rate > dto.Max_Exchange_Rate)
                return "سعر الصرف وحدوده يجب أن تكون موجبة ومنضبطة.";
            return null;
        }

        private static string? NullIfWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}