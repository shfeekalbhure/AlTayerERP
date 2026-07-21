using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// حسابات البنوك للشركة الحالية. الشركة لا تُرسل من سطح المكتب.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public sealed class BankAccountsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public BankAccountsController(AppDbContext context) => _context = context;

        private ServerSession? Session => HttpContext.Items["ServerSession"] as ServerSession;

        private IActionResult? RequireSystemAdmin(out ServerSession? session)
        {
            session = Session;
            if (session == null)
                return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة." });
            if (!session.Is_System_Admin)
                return Forbid();
            return null;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var error = RequireSystemAdmin(out var session);
            if (error != null || session == null) return error!;

            return Ok(await _context.Bank_Accounts.AsNoTracking()
                .Where(x => x.Company_ID == session.Company_ID)
                .OrderBy(x => x.Bank_Name_AR).ThenBy(x => x.Account_No)
                .ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SaveBankAccountRequest request)
        {
            var error = RequireSystemAdmin(out var session);
            if (error != null || session == null) return error!;

            if (request == null || string.IsNullOrWhiteSpace(request.Bank_Name_AR) ||
                string.IsNullOrWhiteSpace(request.Account_No) ||
                string.IsNullOrWhiteSpace(request.Currency_Code))
                return BadRequest(new { message = "اسم البنك ورقم الحساب وكود العملة حقول مطلوبة." });

            var accountNo = request.Account_No.Trim();
            var duplicate = await _context.Bank_Accounts.AnyAsync(x =>
                x.Company_ID == session.Company_ID &&
                x.Account_No == accountNo &&
                x.Bank_Account_ID != request.Bank_Account_ID);
            if (duplicate)
                return BadRequest(new { message = "رقم الحساب البنكي مستخدم مسبقاً داخل الشركة." });

            var row = request.Bank_Account_ID > 0
                ? await _context.Bank_Accounts.FirstOrDefaultAsync(x =>
                    x.Bank_Account_ID == request.Bank_Account_ID && x.Company_ID == session.Company_ID)
                : null;

            if (request.Bank_Account_ID > 0 && row == null)
                return NotFound(new { message = "الحساب البنكي غير موجود ضمن الشركة الحالية." });

            if (row == null)
            {
                row = new BankAccount { Company_ID = session.Company_ID, Created_At = DateTime.Now };
                _context.Bank_Accounts.Add(row);
            }

            row.Bank_Code = request.Bank_Code?.Trim().ToUpperInvariant() ?? string.Empty;
            row.Bank_Name_AR = request.Bank_Name_AR.Trim();
            row.Bank_Name_EN = NullIfWhiteSpace(request.Bank_Name_EN);
            row.Account_No = accountNo;
            row.IBAN = NullIfWhiteSpace(request.IBAN);
            row.Currency_Code = request.Currency_Code.Trim().ToUpperInvariant();
            row.GL_Account = NullIfWhiteSpace(request.GL_Account);
            row.Branch_Name = NullIfWhiteSpace(request.Branch_Name);
            row.Notes = NullIfWhiteSpace(request.Notes);
            row.Is_Active = request.Is_Active;
            row.Updated_At = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(new { message = "تم حفظ الحساب البنكي.", row.Bank_Account_ID });
        }

        private static string? NullIfWhiteSpace(string? value) =>
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
