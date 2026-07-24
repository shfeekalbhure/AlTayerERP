using AlTayerERP.API.DTOs;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>إدارة الصناديق؛ ينشأ حساب الصندوق تلقائياً داخل معاملة واحدة.</summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public sealed class CashBoxesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AccountNumberService _accounts;
        private readonly CashBoxNumberService _numbers;
        private readonly ScreenAuthorizationService _authorization;
        private readonly AuditTrailService _audit;
        public CashBoxesController(AppDbContext context, AccountNumberService accounts, CashBoxNumberService numbers, ScreenAuthorizationService authorization, AuditTrailService audit)
        { _context = context; _accounts = accounts; _numbers = numbers; _authorization = authorization; _audit = audit; }
        private ServerSession? Session => HttpContext.Items["ServerSession"] as ServerSession;
        private async Task<IActionResult?> RequireAsync(ScreenOperation operation)
        {
            if (Session == null) return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة." });
            return await _authorization.IsAllowedAsync(Session, "CashBoxes", operation) ? null : Forbid();
        }

        [HttpGet]
        public async Task<IActionResult> GetCashBoxes()
        {
            var error = await RequireAsync(ScreenOperation.View);
            if (error != null || Session == null) return error!;
            return Ok(await (from box in _context.Cash_Boxes.AsNoTracking()
                             join account in _context.Chart_Of_Accounts.AsNoTracking() on box.Account_ID equals account.Account_ID into accounts
                             from account in accounts.DefaultIfEmpty()
                             where box.Company_ID == Session.Company_ID && box.Branch_ID == Session.Branch_ID
                             orderby box.CashBox_Code
                             select new { box.Cash_Box_ID, box.Company_ID, box.Branch_ID, box.Account_ID, Account_Name_AR = account == null ? null : account.Account_Name_AR,
                                 box.Currency_Code, Code = box.CashBox_Code, NameAR = box.Box_Name_AR, NameEN = box.Box_Name_EN, box.Opening_Balance, box.Max_Limit, box.Min_Limit, box.Is_Active, box.Notes })
                             .ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCashBox(string id)
        {
            var error = await RequireAsync(ScreenOperation.View);
            if (error != null || Session == null) return error!;
            var row = await _context.Cash_Boxes.AsNoTracking().FirstOrDefaultAsync(x => x.Cash_Box_ID == id && x.Company_ID == Session.Company_ID && x.Branch_ID == Session.Branch_ID);
            return row == null ? NotFound(new { message = "الصندوق غير موجود في نطاق الفرع الحالي." }) : Ok(row);
        }

        [HttpGet("GetNextCode")]
        public async Task<IActionResult> GetNextCode()
        {
            var error = await RequireAsync(ScreenOperation.Add);
            if (error != null || Session == null) return error!;
            return Ok(await _numbers.GenerateCashBoxCodeAsync(Session.Company_ID));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCashBoxDto dto)
        {
            var error = await RequireAsync(ScreenOperation.Add);
            if (error != null || Session == null) return error!;
            var validation = await ValidateAsync(dto);
            if (validation != null) return BadRequest(new { message = validation });

            var parent = await _context.Chart_Of_Accounts.FirstAsync(x => x.Account_ID == dto.Account_ID && x.Company_ID == Session.Company_ID);
            await using var tx = await _context.Database.BeginTransactionAsync();
            var account = new ChartOfAccount
            {
                Account_ID = Guid.NewGuid().ToString(), Company_ID = Session.Company_ID, Parent_Account_ID = parent.Account_ID,
                Account_Code = await _accounts.GenerateAccountCodeAsync(Session.Company_ID, parent.Account_ID),
                Account_Name_AR = dto.Box_Name_AR.Trim(), Account_Name_EN = Text(dto.Box_Name_EN),
                Account_Type = "Asset", Account_Category = "Cash", Normal_Balance = "Debit", Account_Level = parent.Account_Level + 1,
                Is_Postable = true, Is_Summary_Account = false, Currency_Code = dto.Currency_Code.Trim().ToUpperInvariant(),
                Is_Active = dto.Is_Active, Allow_ManualEntry = false, Created_By = Session.User_ID.ToString(), Created_At = DateTime.UtcNow
            };
            _context.Chart_Of_Accounts.Add(account);
            var row = new CashBox
            {
                Cash_Box_ID = Guid.NewGuid().ToString(), Company_ID = Session.Company_ID, Branch_ID = Session.Branch_ID, Account_ID = account.Account_ID,
                Currency_Code = account.Currency_Code!, CashBox_Code = await _numbers.GenerateCashBoxCodeAsync(Session.Company_ID),
                Box_Name_AR = dto.Box_Name_AR.Trim(), Box_Name_EN = Text(dto.Box_Name_EN), Opening_Balance = dto.Opening_Balance,
                Max_Limit = dto.Max_Limit, Min_Limit = dto.Min_Limit, Is_Active = dto.Is_Active, Notes = Text(dto.Notes),
                Created_By = Session.User_ID.ToString(), Created_At = DateTime.UtcNow
            };
            _context.Cash_Boxes.Add(row);
            _audit.Add(Session, HttpContext, "cash_boxes", row.Cash_Box_ID, "CREATE", null, new { row.CashBox_Code, row.Box_Name_AR, row.Account_ID, row.Currency_Code, row.Branch_ID });
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            return CreatedAtAction(nameof(GetCashBox), new { id = row.Cash_Box_ID }, row);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CreateCashBoxDto dto)
        {
            var error = await RequireAsync(ScreenOperation.Edit);
            if (error != null || Session == null) return error!;
            var validation = await ValidateAsync(dto);
            if (validation != null) return BadRequest(new { message = validation });
            var row = await _context.Cash_Boxes.FirstOrDefaultAsync(x => x.Cash_Box_ID == id && x.Company_ID == Session.Company_ID && x.Branch_ID == Session.Branch_ID);
            if (row == null) return NotFound(new { message = "الصندوق غير موجود في نطاق الفرع الحالي." });

            var account = await _context.Chart_Of_Accounts.FirstOrDefaultAsync(x => x.Account_ID == row.Account_ID && x.Company_ID == Session.Company_ID);
            var old = new { row.Box_Name_AR, row.Currency_Code, row.Is_Active, row.Max_Limit, row.Min_Limit };
            if (account != null)
            {
                account.Account_Name_AR = dto.Box_Name_AR.Trim(); account.Account_Name_EN = Text(dto.Box_Name_EN);
                account.Currency_Code = dto.Currency_Code.Trim().ToUpperInvariant(); account.Is_Active = dto.Is_Active;
                account.Updated_By = Session.User_ID.ToString(); account.Updated_At = DateTime.UtcNow;
            }
            row.Box_Name_AR = dto.Box_Name_AR.Trim(); row.Box_Name_EN = Text(dto.Box_Name_EN); row.Currency_Code = dto.Currency_Code.Trim().ToUpperInvariant();
            row.Opening_Balance = dto.Opening_Balance; row.Max_Limit = dto.Max_Limit; row.Min_Limit = dto.Min_Limit; row.Is_Active = dto.Is_Active; row.Notes = Text(dto.Notes);
            row.Updated_By = Session.User_ID.ToString(); row.Updated_At = DateTime.UtcNow;
            _audit.Add(Session, HttpContext, "cash_boxes", row.Cash_Box_ID, "UPDATE", old, new { row.Box_Name_AR, row.Currency_Code, row.Is_Active, row.Max_Limit, row.Min_Limit });
            await _context.SaveChangesAsync();
            return Ok(row);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deactivate(string id, [FromQuery] string? reason)
        {
            var error = await RequireAsync(ScreenOperation.Delete);
            if (error != null || Session == null) return error!;
            var row = await _context.Cash_Boxes.FirstOrDefaultAsync(x => x.Cash_Box_ID == id && x.Company_ID == Session.Company_ID && x.Branch_ID == Session.Branch_ID);
            if (row == null) return NotFound(new { message = "الصندوق غير موجود في نطاق الفرع الحالي." });
            if (await _context.Journal_Entry_Details.AnyAsync(x => x.Account_ID == row.Account_ID))
                return BadRequest(new { message = "لا يمكن حذف صندوق له حركة مالية؛ أوقفه فقط بعد إقفال العهدة." });
            row.Is_Active = false; row.Updated_By = Session.User_ID.ToString(); row.Updated_At = DateTime.UtcNow;
            var account = await _context.Chart_Of_Accounts.FirstOrDefaultAsync(x => x.Account_ID == row.Account_ID && x.Company_ID == Session.Company_ID);
            if (account != null) { account.Is_Active = false; account.Updated_By = Session.User_ID.ToString(); account.Updated_At = DateTime.UtcNow; }
            _audit.Add(Session, HttpContext, "cash_boxes", row.Cash_Box_ID, "DEACTIVATE", new { Is_Active = true }, new { Is_Active = false }, reason);
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم إيقاف الصندوق وحسابه المرتبط دون حذف أي بيانات." });
        }

        private async Task<string?> ValidateAsync(CreateCashBoxDto? dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Account_ID) || string.IsNullOrWhiteSpace(dto.Box_Name_AR) || string.IsNullOrWhiteSpace(dto.Currency_Code))
                return "حساب الصناديق واسم الصندوق والعملـة حقول مطلوبة.";
            if (Session == null) return "الجلسة غير صالحة.";
            var parent = await _context.Chart_Of_Accounts.AsNoTracking().FirstOrDefaultAsync(x => x.Account_ID == dto.Account_ID && x.Company_ID == Session.Company_ID);
            if (parent == null || !parent.Is_Active || parent.Is_Postable) return "حساب الصناديق الأب يجب أن يكون نشطاً وتجميعياً.";
            if (!await _context.Currencies.AnyAsync(x => x.Company_ID == Session.Company_ID && x.Currency_Code == dto.Currency_Code.Trim().ToUpperInvariant() && x.Is_Active))
                return "العملة المختارة غير فعالة في الشركة الحالية.";
            if (dto.Min_Limit < 0 || dto.Max_Limit < 0 || dto.Min_Limit > dto.Max_Limit) return "حدود الصندوق غير صحيحة.";
            return null;
        }
        private static string? Text(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();
    }
}