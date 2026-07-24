using AlTayerERP.API.DTOs;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>دليل الحسابات الشجري. لا يعتمد Company_ID أو حقول التدقيق القادمة من التطبيق.</summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public sealed class ChartOfAccountsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AccountNumberService _numbers;
        private readonly ScreenAuthorizationService _authorization;
        private readonly AuditTrailService _audit;

        public ChartOfAccountsController(AppDbContext context, AccountNumberService numbers, ScreenAuthorizationService authorization, AuditTrailService audit)
        {
            _context = context; _numbers = numbers; _authorization = authorization; _audit = audit;
        }
        private ServerSession? Session => HttpContext.Items["ServerSession"] as ServerSession;
        private async Task<IActionResult?> RequireAsync(ScreenOperation operation)
        {
            if (Session == null) return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة." });
            return await _authorization.IsAllowedAsync(Session, "ChartOfAccounts", operation) ? null : Forbid();
        }

        [HttpGet]
        public async Task<IActionResult> GetAccounts()
        {
            var error = await RequireAsync(ScreenOperation.View);
            if (error != null || Session == null) return error!;
            return Ok(await _context.Chart_Of_Accounts.AsNoTracking().Where(x => x.Company_ID == Session.Company_ID)
                .OrderBy(x => x.Account_Code).ToListAsync());
        }

        [HttpGet("GetLookup")]
        public async Task<IActionResult> GetLookup()
        {
            var error = await RequireAsync(ScreenOperation.View);
            if (error != null || Session == null) return error!;
            return Ok(await _context.Chart_Of_Accounts.AsNoTracking()
                .Where(x => x.Company_ID == Session.Company_ID && x.Is_Active && x.Is_Postable && !x.Is_Summary_Account)
                .OrderBy(x => x.Account_Code)
                .Select(x => new { x.Account_ID, x.Account_Code, x.Account_Name_AR, x.Account_Name_EN, Account_Group = x.Account_Category })
                .ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccountById(string id)
        {
            var error = await RequireAsync(ScreenOperation.View);
            if (error != null || Session == null) return error!;
            var row = await _context.Chart_Of_Accounts.AsNoTracking().FirstOrDefaultAsync(x => x.Account_ID == id && x.Company_ID == Session.Company_ID);
            return row == null ? NotFound(new { message = "الحساب غير موجود ضمن الشركة الحالية." }) : Ok(row);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto dto)
        {
            var error = await RequireAsync(ScreenOperation.Add);
            if (error != null || Session == null) return error!;
            var validation = await ValidateAsync(dto, null);
            if (validation != null) return BadRequest(new { message = validation });

            var parent = await ParentAsync(dto.Parent_Account_ID);
            var code = await _numbers.GenerateAccountCodeAsync(Session.Company_ID, dto.Parent_Account_ID);
            if (await _context.Chart_Of_Accounts.AnyAsync(x => x.Company_ID == Session.Company_ID && x.Account_Code == code))
                return Conflict(new { message = "تعذر حجز رقم حساب فريد، أعد المحاولة." });

            var row = new ChartOfAccount { Account_ID = Guid.NewGuid().ToString(), Company_ID = Session.Company_ID, Account_Code = code, Created_At = DateTime.UtcNow, Created_By = Session.User_ID.ToString() };
            Apply(row, dto, parent);
            _context.Chart_Of_Accounts.Add(row);
            _audit.Add(Session, HttpContext, "chart_of_accounts", row.Account_ID, "CREATE", null, new { row.Account_Code, row.Account_Name_AR, row.Parent_Account_ID, row.Is_Postable });
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAccountById), new { id = row.Account_ID }, row);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccount(string id, [FromBody] CreateAccountDto dto)
        {
            var error = await RequireAsync(ScreenOperation.Edit);
            if (error != null || Session == null) return error!;
            var row = await _context.Chart_Of_Accounts.FirstOrDefaultAsync(x => x.Account_ID == id && x.Company_ID == Session.Company_ID);
            if (row == null) return NotFound(new { message = "الحساب غير موجود ضمن الشركة الحالية." });
            var validation = await ValidateAsync(dto, row);
            if (validation != null) return BadRequest(new { message = validation });

            var parent = await ParentAsync(dto.Parent_Account_ID);
            var old = new { row.Account_Code, row.Account_Name_AR, row.Parent_Account_ID, row.Is_Postable, row.Is_Active };
            Apply(row, dto, parent);
            row.Updated_By = Session.User_ID.ToString(); row.Updated_At = DateTime.UtcNow;
            _audit.Add(Session, HttpContext, "chart_of_accounts", row.Account_ID, "UPDATE", old, new { row.Account_Code, row.Account_Name_AR, row.Parent_Account_ID, row.Is_Postable, row.Is_Active });
            await _context.SaveChangesAsync();
            return Ok(row);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deactivate(string id, [FromQuery] string? reason)
        {
            var error = await RequireAsync(ScreenOperation.Delete);
            if (error != null || Session == null) return error!;
            var row = await _context.Chart_Of_Accounts.FirstOrDefaultAsync(x => x.Account_ID == id && x.Company_ID == Session.Company_ID);
            if (row == null) return NotFound(new { message = "الحساب غير موجود ضمن الشركة الحالية." });
            if (row.System_Account) return BadRequest(new { message = "لا يمكن إيقاف حساب نظامي." });
            if (await _context.Chart_Of_Accounts.AnyAsync(x => x.Parent_Account_ID == id && x.Is_Active))
                return BadRequest(new { message = "لا يمكن إيقاف حساب له حسابات أبناء نشطة." });
            if (await _context.Journal_Entry_Details.AnyAsync(x => x.Account_ID == id))
                return BadRequest(new { message = "لا يمكن حذف حساب له حركة؛ أبقه محفوظاً لأغراض المراجعة." });

            row.Is_Active = false; row.Updated_By = Session.User_ID.ToString(); row.Updated_At = DateTime.UtcNow;
            _audit.Add(Session, HttpContext, "chart_of_accounts", row.Account_ID, "DEACTIVATE", new { Is_Active = true }, new { Is_Active = false }, reason);
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم إيقاف الحساب دون حذف تاريخه." });
        }

        private async Task<string?> ValidateAsync(CreateAccountDto? dto, ChartOfAccount? current)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Account_Name_AR) || string.IsNullOrWhiteSpace(dto.Account_Type))
                return "اسم الحساب العربي ونوع الحساب حقول مطلوبة.";
            if (dto.Is_Summary_Account && dto.Is_Postable) return "الحساب التجميعي لا يمكن أن يكون قابلاً للترحيل.";
            if (Session == null) return "الجلسة غير صالحة.";
            if (!string.IsNullOrWhiteSpace(dto.Currency_Code) && !dto.Multi_Currency &&
                !await _context.Currencies.AnyAsync(x => x.Company_ID == Session.Company_ID && x.Currency_Code == dto.Currency_Code.Trim().ToUpperInvariant() && x.Is_Active))
                return "العملة الافتراضية غير فعالة في الشركة الحالية.";
            if (!string.IsNullOrWhiteSpace(dto.Parent_Account_ID))
            {
                if (current != null && dto.Parent_Account_ID == current.Account_ID) return "لا يمكن جعل الحساب أباً لنفسه.";
                var parent = await _context.Chart_Of_Accounts.AsNoTracking().FirstOrDefaultAsync(x => x.Account_ID == dto.Parent_Account_ID && x.Company_ID == Session.Company_ID);
                if (parent == null || !parent.Is_Active) return "الحساب الأب غير موجود أو موقوف.";
                if (parent.Is_Postable) return "لا يمكن إضافة حساب ابن تحت حساب قابل للترحيل.";
                if (current != null && await IsDescendantAsync(parent.Account_ID, current.Account_ID)) return "لا يمكن نقل الحساب تحت أحد أبنائه.";
            }
            return null;
        }

        private async Task<ChartOfAccount?> ParentAsync(string? id) => string.IsNullOrWhiteSpace(id) || Session == null ? null :
            await _context.Chart_Of_Accounts.FirstAsync(x => x.Account_ID == id && x.Company_ID == Session.Company_ID);

        private async Task<bool> IsDescendantAsync(string candidateParentId, string accountId)
        {
            var next = candidateParentId;
            while (!string.IsNullOrWhiteSpace(next))
            {
                if (next == accountId) return true;
                next = await _context.Chart_Of_Accounts.AsNoTracking().Where(x => x.Account_ID == next).Select(x => x.Parent_Account_ID).FirstOrDefaultAsync();
            }
            return false;
        }

        private static void Apply(ChartOfAccount row, CreateAccountDto dto, ChartOfAccount? parent)
        {
            row.Parent_Account_ID = parent?.Account_ID; row.Account_Level = parent == null ? 1 : parent.Account_Level + 1;
            row.Account_Name_AR = dto.Account_Name_AR.Trim(); row.Account_Name_EN = string.IsNullOrWhiteSpace(dto.Account_Name_EN) ? null : dto.Account_Name_EN.Trim();
            row.Account_Type = dto.Account_Type.Trim(); row.Account_Category = string.IsNullOrWhiteSpace(dto.Account_Category) ? null : dto.Account_Category.Trim();
            row.Normal_Balance = string.IsNullOrWhiteSpace(dto.Normal_Balance) ? null : dto.Normal_Balance.Trim();
            row.Is_Summary_Account = dto.Is_Summary_Account; row.Is_Postable = dto.Is_Summary_Account ? false : dto.Is_Postable;
            row.Currency_Code = dto.Multi_Currency ? null : dto.Currency_Code?.Trim().ToUpperInvariant();
            row.Is_Active = dto.Is_Active; row.Allow_ManualEntry = dto.Allow_ManualEntry; row.System_Account = dto.System_Account;
            row.Requires_Party = dto.Requires_Party; row.Requires_CostCenter = dto.Requires_CostCenter; row.Requires_Project = dto.Requires_Project;
            row.Multi_Currency = dto.Multi_Currency; row.Affects_Balance_Sheet = dto.Affects_Balance_Sheet; row.Affects_Income_Statement = dto.Affects_Income_Statement;
            row.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
        }
    }
}