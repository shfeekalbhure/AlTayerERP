using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>حسابات البنوك ضمن نطاق الشركة الحالية وصلاحية شاشة البنوك.</summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public sealed class BankAccountsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ScreenAuthorizationService _authorization;
        private readonly AuditTrailService _audit;

        public BankAccountsController(AppDbContext context, ScreenAuthorizationService authorization, AuditTrailService audit)
        {
            _context = context; _authorization = authorization; _audit = audit;
        }

        private ServerSession? Session => HttpContext.Items["ServerSession"] as ServerSession;
        private async Task<IActionResult?> RequireAsync(ScreenOperation operation)
        {
            if (Session == null) return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة." });
            return await _authorization.IsAllowedAsync(Session, "Banks", operation) ? null : Forbid();
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var error = await RequireAsync(ScreenOperation.View);
            if (error != null || Session == null) return error!;
            return Ok(await _context.Bank_Accounts.AsNoTracking()
                .Where(x => x.Company_ID == Session.Company_ID)
                .OrderBy(x => x.Bank_Name_AR).ThenBy(x => x.Account_No).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SaveBankAccountRequest request)
        {
            var operation = request.Bank_Account_ID > 0 ? ScreenOperation.Edit : ScreenOperation.Add;
            var error = await RequireAsync(operation);
            if (error != null || Session == null) return error!;
            if (string.IsNullOrWhiteSpace(request.Bank_Name_AR) || string.IsNullOrWhiteSpace(request.Account_No) || string.IsNullOrWhiteSpace(request.Currency_Code))
                return BadRequest(new { message = "اسم البنك ورقم الحساب وكود العملة حقول مطلوبة." });

            var accountNo = request.Account_No.Trim();
            var currency = request.Currency_Code.Trim().ToUpperInvariant();
            if (!await _context.Currencies.AnyAsync(x => x.Company_ID == Session.Company_ID && x.Currency_Code == currency && x.Is_Active))
                return BadRequest(new { message = "العملة المختارة غير فعالة أو لا تتبع الشركة الحالية." });

            var duplicate = await _context.Bank_Accounts.AnyAsync(x => x.Company_ID == Session.Company_ID && x.Account_No == accountNo && x.Bank_Account_ID != request.Bank_Account_ID);
            if (duplicate) return Conflict(new { message = "رقم الحساب البنكي مستخدم مسبقاً داخل الشركة." });

            var row = request.Bank_Account_ID > 0
                ? await _context.Bank_Accounts.FirstOrDefaultAsync(x => x.Bank_Account_ID == request.Bank_Account_ID && x.Company_ID == Session.Company_ID)
                : null;
            if (request.Bank_Account_ID > 0 && row == null) return NotFound(new { message = "الحساب البنكي غير موجود ضمن الشركة الحالية." });

            var old = row == null ? null : new { row.Bank_Name_AR, row.Account_No, row.Currency_Code, row.Is_Active };
            if (row == null)
            {
                row = new BankAccount { Company_ID = Session.Company_ID, Created_At = DateTime.UtcNow };
                _context.Bank_Accounts.Add(row);
            }
            row.Bank_Code = request.Bank_Code?.Trim().ToUpperInvariant() ?? string.Empty;
            row.Bank_Name_AR = request.Bank_Name_AR.Trim();
            row.Bank_Name_EN = Text(request.Bank_Name_EN);
            row.Account_No = accountNo; row.IBAN = Text(request.IBAN); row.Currency_Code = currency;
            row.GL_Account = Text(request.GL_Account); row.Branch_Name = Text(request.Branch_Name);
            row.Notes = Text(request.Notes); row.Is_Active = request.Is_Active; row.Updated_At = DateTime.UtcNow;

            _audit.Add(Session, HttpContext, "bank_accounts", request.Bank_Account_ID > 0 ? request.Bank_Account_ID.ToString() : accountNo,
                request.Bank_Account_ID > 0 ? "UPDATE" : "CREATE", old, new { row.Bank_Name_AR, row.Account_No, row.Currency_Code, row.Is_Active });
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم حفظ الحساب البنكي.", row.Bank_Account_ID });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Deactivate(int id, [FromQuery] string? reason)
        {
            var error = await RequireAsync(ScreenOperation.Delete);
            if (error != null || Session == null) return error!;
            var row = await _context.Bank_Accounts.FirstOrDefaultAsync(x => x.Bank_Account_ID == id && x.Company_ID == Session.Company_ID);
            if (row == null) return NotFound(new { message = "الحساب البنكي غير موجود ضمن الشركة الحالية." });

            row.Is_Active = false; row.Updated_At = DateTime.UtcNow;
            _audit.Add(Session, HttpContext, "bank_accounts", id.ToString(), "DEACTIVATE", new { Is_Active = true }, new { Is_Active = false }, reason);
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم إيقاف الحساب البنكي دون حذف تاريخه." });
        }

        private static string? Text(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public sealed class SaveBankAccountRequest
    {
        public int Bank_Account_ID { get; set; }
        public string? Bank_Code { get; set; }
        public string Bank_Name_AR { get; set; } = string.Empty;
        public string? Bank_Name_EN { get; set; }
        public string Account_No { get; set; } = string.Empty;
        public string? IBAN { get; set; }
        public string Currency_Code { get; set; } = string.Empty;
        public string? GL_Account { get; set; }
        public string? Branch_Name { get; set; }
        public string? Notes { get; set; }
        public bool Is_Active { get; set; } = true;
    }
}