using AlTayerERP.API.DTOs;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>دليل الحسابات الشجري المعزول حسب شركة الجلسة.</summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public sealed class ChartOfAccountsController : ControllerBase
    {
        private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
            { "Asset", "Liability", "Equity", "Revenue", "Expense" };
        private static readonly HashSet<string> AllowedBalances = new(StringComparer.OrdinalIgnoreCase)
            { "Debit", "Credit" };

        private readonly AppDbContext _context;
        private readonly AccountNumberService _numbers;
        private readonly ScreenAuthorizationService _authorization;
        private readonly AuditTrailService _audit;

        public ChartOfAccountsController(AppDbContext context, AccountNumberService numbers, ScreenAuthorizationService authorization, AuditTrailService audit)
        {
            _context = context;
            _numbers = numbers;
            _authorization = authorization;
            _audit = audit;
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

            var rows = await _context.Chart_Of_Accounts.AsNoTracking()
                .Where(x => x.Company_ID == Session.Company_ID)
                .OrderBy(x => x.Account_Code)
                .ToListAsync();
            return Ok(rows);
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

            var row = await _context.Chart_Of_Accounts.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Account_ID == id && x.Company_ID == Session.Company_ID);
            return row == null ? NotFound(new { message = "الحساب غير موجود ضمن الشركة الحالية." }) : Ok(row);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto dto)
        {
            var error = await RequireAsync(ScreenOperation.Add);
            if (error != null || Session == null) return error!;

            Normalize(dto);
            var validation = await ValidateAsync(dto, null);
            if (validation != null) return BadRequest(new { message = validation });

            var parent = await ParentAsync(dto.Parent_Account_ID);
            var code = await _numbers.GenerateAccountCodeAsync(Session.Company_ID, dto.Parent_Account_ID);
            if (await _context.Chart_Of_Accounts.AnyAsync(x => x.Company_ID == Session.Company_ID && x.Account_Code == code))
                return Conflict(new { message = "تعذر حجز رقم حساب فريد، أعد المحاولة." });

            var row = new ChartOfAccount
            {
                Account_ID = Guid.NewGuid().ToString(),
                Company_ID = Session.Company_ID,
                Account_Code = code,
                Created_At = DateTime.UtcNow,
                Created_By = Session.User_ID.ToString()
            };
            Apply(row, dto, parent);
            _context.Chart_Of_Accounts.Add(row);
            _audit.Add(Session, HttpContext, "chart_of_accounts", row.Account_ID, "CREATE", null,
                new { row.Account_Code, row.Account_Name_AR, row.Parent_Account_ID, row.Is_Postable, row.Is_Active });
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAccountById), new { id = row.Account_ID }, row);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccount(string id, [FromBody] CreateAccountDto dto)
        {
            var error = await RequireAsync(ScreenOperation.Edit);
            if (error != null || Session == null) return error!;

            var row = await _context.Chart_Of_Accounts
                .FirstOrDefaultAsync(x => x.Account_ID == id && x.Company_ID == Session.Company_ID);
            if (row == null) return NotFound(new { message = "الحساب غير موجود ضمن الشركة الحالية." });

            Normalize(dto);
            var validation = await ValidateAsync(dto, row);
            if (validation != null) return BadRequest(new { message = validation });

            bool hasChildren = await _context.Chart_Of_Accounts.AnyAsync(x => x.Company_ID == Session.Company_ID && x.Parent_Account_ID == id);
            bool hasMovement = await _context.Journal_Entry_Details.AnyAsync(x => x.Account_ID == id);

            if (hasChildren && (dto.Is_Postable || !dto.Is_Summary_Account))
                return BadRequest(new { message = "الحساب الذي لديه حسابات فرعية يجب أن يبقى تجميعياً وغير قابل للترحيل." });
            if (hasMovement && row.Parent_Account_ID != dto.Parent_Account_ID)
                return BadRequest(new { message = "لا يمكن نقل حساب سبق استخدامه في قيود محاسبية إلى أب آخر." });
            if (hasMovement && row.Account_Type != dto.Account_Type)
                return BadRequest(new { message = "لا يمكن تغيير نوع حساب سبق استخدامه في قيود محاسبية." });
            if (hasMovement && row.Normal_Balance != dto.Normal_Balance)
                return BadRequest(new { message = "لا يمكن تغيير طبيعة حساب سبق استخدامه في قيود محاسبية." });
            if (row.System_Account && !dto.System_Account)
                return BadRequest(new { message = "لا يمكن إلغاء صفة حساب النظام." });

            var parent = await ParentAsync(dto.Parent_Account_ID);
            var old = new { row.Account_Code, row.Account_Name_AR, row.Parent_Account_ID, row.Account_Type, row.Normal_Balance, row.Is_Postable, row.Is_Active };
            Apply(row, dto, parent);
            row.Updated_By = Session.User_ID.ToString();
            row.Updated_At = DateTime.UtcNow;
            _audit.Add(Session, HttpContext, "chart_of_accounts", row.Account_ID, "UPDATE", old,
                new { row.Account_Code, row.Account_Name_AR, row.Parent_Account_ID, row.Account_Type, row.Normal_Balance, row.Is_Postable, row.Is_Active });
            await _context.SaveChangesAsync();
            return Ok(row);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deactivate(string id, [FromQuery] string? reason)
        {
            var error = await RequireAsync(ScreenOperation.Delete);
            if (error != null || Session == null) return error!;

            var row = await _context.Chart_Of_Accounts
                .FirstOrDefaultAsync(x => x.Account_ID == id && x.Company_ID == Session.Company_ID);
            if (row == null) return NotFound(new { message = "الحساب غير موجود ضمن الشركة الحالية." });
            if (!row.Is_Active) return Ok(new { message = "الحساب موقوف مسبقاً." });
            if (row.System_Account) return BadRequest(new { message = "لا يمكن إيقاف حساب نظامي." });
            if (await _context.Chart_Of_Accounts.AnyAsync(x => x.Company_ID == Session.Company_ID && x.Parent_Account_ID == id && x.Is_Active))
                return BadRequest(new { message = "لا يمكن إيقاف حساب له حسابات أبناء نشطة." });

            row.Is_Active = false;
            row.Updated_By = Session.User_ID.ToString();
            row.Updated_At = DateTime.UtcNow;
            _audit.Add(Session, HttpContext, "chart_of_accounts", row.Account_ID, "DEACTIVATE", new { Is_Active = true }, new { Is_Active = false }, reason);
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم إيقاف الحساب دون حذف تاريخه أو حركاته." });
        }

        private async Task<string?> ValidateAsync(CreateAccountDto? dto, ChartOfAccount? current)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Account_Name_AR) || string.IsNullOrWhiteSpace(dto.Account_Type))
                return "اسم الحساب العربي ونوع الحساب حقول مطلوبة.";
            if (Session == null) return "الجلسة غير صالحة.";
            if (!AllowedTypes.Contains(dto.Account_Type)) return "نوع الحساب غير معتمد.";
            if (!AllowedBalances.Contains(dto.Normal_Balance)) return "طبيعة الحساب يجب أن تكون Debit أو Credit.";
            if (dto.Is_Summary_Account && dto.Is_Postable) return "الحساب التجميعي لا يمكن أن يكون قابلاً للترحيل.";
            if (dto.Multi_Currency && !string.IsNullOrWhiteSpace(dto.Currency_Code))
                return "الحساب متعدد العملات لا يحدد له رمز عملة افتراضية.";
            if (!dto.Multi_Currency && string.IsNullOrWhiteSpace(dto.Currency_Code))
                return "يجب تحديد العملة الافتراضية للحساب غير متعدد العملات.";
            if (!string.IsNullOrWhiteSpace(dto.Currency_Code) &&
                !await _context.Currencies.AnyAsync(x => x.Company_ID == Session.Company_ID && x.Currency_Code == dto.Currency_Code && x.Is_Active))
                return "العملة الافتراضية غير فعالة في الشركة الحالية.";

            var duplicateName = await _context.Chart_Of_Accounts.AsNoTracking().AnyAsync(x =>
                x.Company_ID == Session.Company_ID && x.Account_ID != (current == null ? "" : current.Account_ID) &&
                x.Parent_Account_ID == dto.Parent_Account_ID && x.Account_Name_AR == dto.Account_Name_AR);
            if (duplicateName) return "يوجد حساب آخر بالاسم العربي نفسه تحت الحساب الأب المحدد.";

            if (!string.IsNullOrWhiteSpace(dto.Parent_Account_ID))
            {
                if (current != null && dto.Parent_Account_ID == current.Account_ID) return "لا يمكن جعل الحساب أباً لنفسه.";
                var parent = await _context.Chart_Of_Accounts.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Account_ID == dto.Parent_Account_ID && x.Company_ID == Session.Company_ID);
                if (parent == null || !parent.Is_Active) return "الحساب الأب غير موجود أو موقوف.";
                if (parent.Is_Postable) return "لا يمكن إضافة حساب ابن تحت حساب قابل للترحيل.";
                if (current != null && await IsDescendantAsync(parent.Account_ID, current.Account_ID))
                    return "لا يمكن نقل الحساب تحت أحد أبنائه.";
            }
            return null;
        }

        private async Task<ChartOfAccount?> ParentAsync(string? id) =>
            string.IsNullOrWhiteSpace(id) || Session == null ? null :
            await _context.Chart_Of_Accounts.FirstAsync(x => x.Account_ID == id && x.Company_ID == Session.Company_ID);

        private async Task<bool> IsDescendantAsync(string candidateParentId, string accountId)
        {
            var next = candidateParentId;
            while (!string.IsNullOrWhiteSpace(next))
            {
                if (next == accountId) return true;
                next = await _context.Chart_Of_Accounts.AsNoTracking()
                    .Where(x => x.Account_ID == next && Session != null && x.Company_ID == Session.Company_ID)
                    .Select(x => x.Parent_Account_ID)
                    .FirstOrDefaultAsync();
            }
            return false;
        }

        private static void Normalize(CreateAccountDto dto)
        {
            dto.Parent_Account_ID = string.IsNullOrWhiteSpace(dto.Parent_Account_ID) ? null : dto.Parent_Account_ID.Trim();
            dto.Account_Name_AR = dto.Account_Name_AR?.Trim() ?? string.Empty;
            dto.Account_Name_EN = string.IsNullOrWhiteSpace(dto.Account_Name_EN) ? null : dto.Account_Name_EN.Trim();
            dto.Account_Type = dto.Account_Type?.Trim() ?? string.Empty;
            dto.Account_Category = dto.Account_Category?.Trim() ?? string.Empty;
            dto.Normal_Balance = (dto.Normal_Balance?.Trim()) switch
            {
                "مدين" => "Debit",
                "دائن" => "Credit",
                var value => value ?? string.Empty
            };
            dto.Currency_Code = string.IsNullOrWhiteSpace(dto.Currency_Code) ? null : dto.Currency_Code.Trim().ToUpperInvariant();
            dto.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
        }

        private static void Apply(ChartOfAccount row, CreateAccountDto dto, ChartOfAccount? parent)
        {
            row.Parent_Account_ID = parent?.Account_ID;
            row.Account_Level = parent == null ? 1 : parent.Account_Level + 1;
            row.Account_Name_AR = dto.Account_Name_AR;
            row.Account_Name_EN = dto.Account_Name_EN;
            row.Account_Type = dto.Account_Type;
            row.Account_Category = string.IsNullOrWhiteSpace(dto.Account_Category) ? null : dto.Account_Category;
            row.Normal_Balance = dto.Normal_Balance;
            row.Is_Summary_Account = dto.Is_Summary_Account;
            row.Is_Postable = dto.Is_Summary_Account ? false : dto.Is_Postable;
            row.Currency_Code = dto.Multi_Currency ? null : dto.Currency_Code;
            row.Is_Active = dto.Is_Active;
            row.Allow_ManualEntry = dto.Allow_ManualEntry;
            row.System_Account = dto.System_Account;
            row.Requires_Party = dto.Requires_Party;
            row.Requires_CostCenter = dto.Requires_CostCenter;
            row.Requires_Project = dto.Requires_Project;
            row.Multi_Currency = dto.Multi_Currency;
            row.Affects_Balance_Sheet = dto.Affects_Balance_Sheet;
            row.Affects_Income_Statement = dto.Affects_Income_Statement;
            row.Notes = dto.Notes;
        }
    }
}
