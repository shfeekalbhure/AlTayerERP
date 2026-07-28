using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
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
            _context = context;
            _authorization = authorization;
            _audit = audit;
        }

        private ServerSession? Session => HttpContext.Items["ServerSession"] as ServerSession;

        private async Task<IActionResult?> RequireAsync(ScreenOperation operation)
        {
            if (Session == null)
                return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة." });

            return await _authorization.IsAllowedAsync(Session, "Banks", operation)
                ? null
                : Forbid();
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var error = await RequireAsync(ScreenOperation.View);
            if (error != null || Session == null) return error!;

            return Ok(await _context.Bank_Accounts.AsNoTracking()
                .Where(x => x.Company_ID == Session.Company_ID)
                .OrderBy(x => x.Bank_Name_AR)
                .ThenBy(x => x.Account_No)
                .ToListAsync());
        }

        [HttpGet("GetGLAccountsLookup")]
        public async Task<IActionResult> GetGLAccountsLookup()
        {
            var error = await RequireAsync(ScreenOperation.View);
            if (error != null || Session == null) return error!;

            bool bankCategoryExists = await _context.Set<AccountCategory>().AsNoTracking().AnyAsync(x =>
                x.Company_ID == Session.Company_ID &&
                x.Category_Code == "Bank" &&
                x.Account_Type == "Asset" &&
                x.Normal_Balance == "Debit" &&
                x.Is_Active);

            if (!bankCategoryExists)
                return BadRequest(new { message = "تصنيف البنوك Bank غير موجود أو غير فعال للشركة الحالية." });

            var rows = await _context.Chart_Of_Accounts.AsNoTracking()
                .Where(x => x.Company_ID == Session.Company_ID &&
                            x.Is_Active &&
                            x.Account_Type == "Asset" &&
                            x.Account_Category == "Bank" &&
                            x.Normal_Balance == "Debit" &&
                            x.Is_Postable &&
                            !x.Is_Summary_Account)
                .OrderBy(x => x.Account_Code)
                .Select(x => new
                {
                    x.Account_ID,
                    x.Account_Code,
                    x.Account_Name_AR,
                    x.Currency_Code,
                    x.Multi_Currency
                })
                .ToListAsync();

            return Ok(rows);
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SaveBankAccountRequest request)
        {
            var operation = request.Bank_Account_ID > 0 ? ScreenOperation.Edit : ScreenOperation.Add;
            var error = await RequireAsync(operation);
            if (error != null || Session == null) return error!;

            if (string.IsNullOrWhiteSpace(request.Bank_Name_AR) ||
                string.IsNullOrWhiteSpace(request.Account_No) ||
                string.IsNullOrWhiteSpace(request.Currency_Code) ||
                string.IsNullOrWhiteSpace(request.GL_Account))
                return BadRequest(new { message = "اسم البنك ورقم الحساب والعملة والحساب المحاسبي حقول مطلوبة." });

            string accountNo = request.Account_No.Trim();
            string currency = request.Currency_Code.Trim().ToUpperInvariant();
            string glAccountValue = request.GL_Account.Trim();

            if (!await _context.Currencies.AnyAsync(x =>
                    x.Company_ID == Session.Company_ID &&
                    x.Currency_Code == currency &&
                    x.Is_Active))
                return BadRequest(new { message = "العملة المختارة غير فعالة أو لا تتبع الشركة الحالية." });

            bool bankCategoryExists = await _context.Set<AccountCategory>().AsNoTracking().AnyAsync(x =>
                x.Company_ID == Session.Company_ID &&
                x.Category_Code == "Bank" &&
                x.Account_Type == "Asset" &&
                x.Normal_Balance == "Debit" &&
                x.Is_Active);

            if (!bankCategoryExists)
                return BadRequest(new { message = "تصنيف البنوك Bank غير موجود أو غير فعال للشركة الحالية. نفذ سكربت تصنيفات الحسابات أولاً." });

            var glAccount = await _context.Chart_Of_Accounts.AsNoTracking().FirstOrDefaultAsync(x =>
                x.Company_ID == Session.Company_ID &&
                (x.Account_ID == glAccountValue || x.Account_Code == glAccountValue));

            if (glAccount == null)
                return BadRequest(new { message = "الحساب المحاسبي المرتبط غير موجود ضمن الشركة الحالية." });
            if (!glAccount.Is_Active || !glAccount.Is_Postable || glAccount.Is_Summary_Account)
                return BadRequest(new { message = "حساب البنك المحاسبي يجب أن يكون نشطاً ونهائياً وقابلاً للترحيل." });
            if (!string.Equals(glAccount.Account_Type, "Asset", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(glAccount.Account_Category, "Bank", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(glAccount.Normal_Balance, "Debit", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "الحساب المحاسبي المختار ليس حساب بنك معتمداً من نوع Asset وتصنيف Bank وطبيعته Debit." });
            if (!glAccount.Multi_Currency &&
                !string.Equals(glAccount.Currency_Code, currency, StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "عملة الحساب البنكي لا تطابق عملة حساب الأستاذ المرتبط." });

            bool duplicate = await _context.Bank_Accounts.AnyAsync(x =>
                x.Company_ID == Session.Company_ID &&
                x.Account_No == accountNo &&
                x.Bank_Account_ID != request.Bank_Account_ID);
            if (duplicate)
                return Conflict(new { message = "رقم الحساب البنكي مستخدم مسبقاً داخل الشركة." });

            bool duplicateGl = await _context.Bank_Accounts.AnyAsync(x =>
                x.Company_ID == Session.Company_ID &&
                x.GL_Account == glAccount.Account_ID &&
                x.Bank_Account_ID != request.Bank_Account_ID);
            if (duplicateGl)
                return Conflict(new { message = "الحساب المحاسبي مرتبط مسبقاً بحساب بنكي آخر داخل الشركة." });

            var row = request.Bank_Account_ID > 0
                ? await _context.Bank_Accounts.FirstOrDefaultAsync(x =>
                    x.Bank_Account_ID == request.Bank_Account_ID &&
                    x.Company_ID == Session.Company_ID)
                : null;

            if (request.Bank_Account_ID > 0 && row == null)
                return NotFound(new { message = "الحساب البنكي غير موجود ضمن الشركة الحالية." });

            if (row != null && request.Is_Active != row.Is_Active)
                return BadRequest(new { message = "لا يمكن تغيير حالة الحساب البنكي من الحفظ العام. استخدم مسار الإيقاف المخصص." });

            var old = row == null ? null : new
            {
                row.Bank_Name_AR,
                row.Account_No,
                row.Currency_Code,
                row.GL_Account,
                row.Is_Active
            };

            if (row == null)
            {
                row = new BankAccount
                {
                    Company_ID = Session.Company_ID,
                    Created_At = DateTime.UtcNow,
                    Is_Active = true
                };
                _context.Bank_Accounts.Add(row);
            }

            row.Bank_Code = request.Bank_Code?.Trim().ToUpperInvariant() ?? string.Empty;
            row.Bank_Name_AR = request.Bank_Name_AR.Trim();
            row.Bank_Name_EN = Text(request.Bank_Name_EN);
            row.Account_No = accountNo;
            row.IBAN = Text(request.IBAN);
            row.Currency_Code = currency;
            row.GL_Account = glAccount.Account_ID;
            row.Branch_Name = Text(request.Branch_Name);
            row.Notes = Text(request.Notes);
            row.Updated_At = DateTime.UtcNow;

            _audit.Add(Session, HttpContext, "bank_accounts",
                request.Bank_Account_ID > 0 ? request.Bank_Account_ID.ToString() : accountNo,
                request.Bank_Account_ID > 0 ? "UPDATE" : "CREATE",
                old,
                new
                {
                    row.Bank_Name_AR,
                    row.Account_No,
                    row.Currency_Code,
                    row.GL_Account,
                    row.Is_Active
                });

            await _context.SaveChangesAsync();
            return Ok(new { message = "تم حفظ الحساب البنكي.", row.Bank_Account_ID });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Deactivate(int id, [FromQuery] string? reason)
        {
            var error = await RequireAsync(ScreenOperation.Delete);
            if (error != null || Session == null) return error!;

            var row = await _context.Bank_Accounts.FirstOrDefaultAsync(x =>
                x.Bank_Account_ID == id &&
                x.Company_ID == Session.Company_ID);

            if (row == null)
                return NotFound(new { message = "الحساب البنكي غير موجود ضمن الشركة الحالية." });
            if (!row.Is_Active)
                return BadRequest(new { message = "الحساب البنكي موقوف مسبقاً." });

            if (string.IsNullOrWhiteSpace(reason))
                return BadRequest(new { message = "سبب إيقاف الحساب البنكي مطلوب." });

            if (!string.IsNullOrWhiteSpace(row.GL_Account))
            {
                bool hasUnposted = await _context.Journal_Entry_Details.AsNoTracking().AnyAsync(x =>
                    x.Account_ID == row.GL_Account &&
                    x.JournalEntry.Is_Active &&
                    !x.JournalEntry.Is_Cancelled &&
                    !x.JournalEntry.Is_Posted);

                if (hasUnposted)
                    return BadRequest(new { message = "لا يمكن إيقاف الحساب البنكي لوجود قيود غير مرحلة مرتبطة بحساب الأستاذ." });
            }

            row.Is_Active = false;
            row.Updated_At = DateTime.UtcNow;

            _audit.Add(Session, HttpContext, "bank_accounts", id.ToString(), "DEACTIVATE",
                new { Is_Active = true },
                new { Is_Active = false },
                reason);

            await _context.SaveChangesAsync();
            return Ok(new { message = "تم إيقاف الحساب البنكي دون حذف تاريخه." });
        }

        private static string? Text(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
