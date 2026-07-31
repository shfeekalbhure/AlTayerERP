using AlTayerERP.API.DTOs;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>إدارة الصناديق وربطها بدفتر الأستاذ وفق ضوابط محاسبية.</summary>
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
        // يمنع تعارض الترقيم داخل نسخة الـ API الواحدة إلى أن تُضاف حماية فريدة على مستوى قاعدة البيانات في حزمة مستقلة.
        private static readonly System.Threading.SemaphoreSlim CashBoxCreationGate = new(1, 1);

        public CashBoxesController(AppDbContext context, AccountNumberService accounts, CashBoxNumberService numbers, ScreenAuthorizationService authorization, AuditTrailService audit)
        {
            _context = context;
            _accounts = accounts;
            _numbers = numbers;
            _authorization = authorization;
            _audit = audit;
        }

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

            var boxes = await (from box in _context.Cash_Boxes.AsNoTracking()
                               join account in _context.Chart_Of_Accounts.AsNoTracking() on box.Account_ID equals account.Account_ID into accounts
                               from account in accounts.DefaultIfEmpty()
                               join branch in _context.Tenant_Branches.AsNoTracking() on box.Branch_ID equals branch.Branch_ID into branches
                               from branch in branches.DefaultIfEmpty()
                               where box.Company_ID == Session.Company_ID && box.Branch_ID == Session.Branch_ID
                               orderby box.CashBox_Code
                               select new
                               {
                                   box.Cash_Box_ID,
                                   box.Company_ID,
                                   box.Branch_ID,
                                   Branch_Name = branch == null ? null : branch.Branch_Name,
                                   Account_ID = box.Account_ID,
                                   Linked_Account_ID = box.Account_ID,
                                   Account_Name_AR = account == null ? null : account.Account_Name_AR,
                                   box.Currency_Code,
                                   Code = box.CashBox_Code,
                                   NameAR = box.Box_Name_AR,
                                   NameEN = box.Box_Name_EN,
                                   box.Opening_Balance,
                                   box.Max_Limit,
                                   box.Min_Limit,
                                   box.Is_Active,
                                   box.Notes,
                                   box.Created_By,
                                   box.Created_At,
                                   box.Updated_By,
                                   box.Updated_At
                               }).ToListAsync();

            var accountIds = boxes.Select(x => x.Linked_Account_ID).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            var movements = await _context.Journal_Entry_Details.AsNoTracking()
                .Where(x => accountIds.Contains(x.Account_ID)
                            && x.JournalEntry.Is_Posted
                            && x.JournalEntry.Is_Active
                            && !x.JournalEntry.Is_Cancelled
                            && !x.JournalEntry.Is_Reversed)
                .GroupBy(x => x.Account_ID)
                .Select(g => new
                {
                    Account_ID = g.Key,
                    Current_Balance = g.Sum(x => x.Debit_Amount - x.Credit_Amount),
                    Has_Posted_Movement = true
                })
                .ToDictionaryAsync(x => x.Account_ID);

            return Ok(boxes.Select(box =>
            {
                movements.TryGetValue(box.Linked_Account_ID, out var movement);
                return new
                {
                    box.Cash_Box_ID,
                    box.Company_ID,
                    box.Branch_ID,
                    box.Branch_Name,
                    box.Account_ID,
                    box.Linked_Account_ID,
                    box.Account_Name_AR,
                    box.Currency_Code,
                    box.Code,
                    box.NameAR,
                    box.NameEN,
                    box.Opening_Balance,
                    Current_Balance = movement?.Current_Balance ?? 0m,
                    Has_Posted_Movement = movement?.Has_Posted_Movement ?? false,
                    box.Max_Limit,
                    box.Min_Limit,
                    box.Is_Active,
                    box.Notes,
                    box.Created_By,
                    box.Created_At,
                    box.Updated_By,
                    box.Updated_At
                };
            }));
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
            if (dto.Opening_Balance != 0)
                return BadRequest(new { message = "لا يسمح بإدخال رصيد افتتاحي من شاشة تعريف الصندوق. استخدم مستند الأرصدة الافتتاحية." });

            await CashBoxCreationGate.WaitAsync(HttpContext.RequestAborted);
            try
            {
                // يعاد التحقق بعد دخول البوابة حتى لا يمر اسم أو كود متكرر بسبب طلب متزامن.
                var validation = await ValidateAsync(dto, null);
                if (validation != null) return BadRequest(new { message = validation });

                await using var tx = await _context.Database.BeginTransactionAsync();

                var row = new CashBox
                {
                    Cash_Box_ID = Guid.NewGuid().ToString(),
                    Company_ID = Session.Company_ID,
                    Branch_ID = Session.Branch_ID,
                    Account_ID = dto.Account_ID.Trim(),
                    Currency_Code = dto.Currency_Code.Trim().ToUpperInvariant(),
                    CashBox_Code = await _numbers.GenerateCashBoxCodeAsync(Session.Company_ID),
                    Box_Name_AR = dto.Box_Name_AR.Trim(),
                    Box_Name_EN = Text(dto.Box_Name_EN),
                    Opening_Balance = 0,
                    Max_Limit = dto.Max_Limit,
                    Min_Limit = dto.Min_Limit,
                    Is_Active = true,
                    Notes = Text(dto.Notes),
                    Created_By = Session.User_ID.ToString(),
                    Created_At = DateTime.UtcNow
                };
                _context.Cash_Boxes.Add(row);
                _audit.Add(Session, HttpContext, "cash_boxes", row.Cash_Box_ID, "CREATE", null, new { row.CashBox_Code, row.Box_Name_AR, row.Account_ID, row.Currency_Code, row.Branch_ID });
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return CreatedAtAction(nameof(GetCashBox), new { id = row.Cash_Box_ID }, row);
            }
            finally
            {
                CashBoxCreationGate.Release();
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CreateCashBoxDto dto)
        {
            var error = await RequireAsync(ScreenOperation.Edit);
            if (error != null || Session == null) return error!;
            var validation = await ValidateAsync(dto, id);
            if (validation != null) return BadRequest(new { message = validation });

            var row = await _context.Cash_Boxes.FirstOrDefaultAsync(x => x.Cash_Box_ID == id && x.Company_ID == Session.Company_ID && x.Branch_ID == Session.Branch_ID);
            if (row == null) return NotFound(new { message = "الصندوق غير موجود في نطاق الفرع الحالي." });

            if (dto.Is_Active != row.Is_Active)
                return BadRequest(new { message = "لا يمكن تغيير حالة الصندوق من التعديل. استخدم زر الإيقاف أو إعادة التفعيل بعد استكمال الضوابط المطلوبة." });

            if (dto.Opening_Balance != row.Opening_Balance)
                return BadRequest(new { message = "لا يمكن تعديل الرصيد الافتتاحي من شاشة الصناديق؛ استخدم قيد تسوية أو مستند أرصدة افتتاحية." });

            var hasPostedMovement = await HasPostedMovementAsync(row.Account_ID);
            if (hasPostedMovement && !string.Equals(row.Account_ID, dto.Account_ID.Trim(), StringComparison.Ordinal))
                return BadRequest(new { message = "لا يمكن تغيير الحساب المالي لصندوق لديه حركات مالية مرحلة." });
            var requestedCurrency = dto.Currency_Code.Trim().ToUpperInvariant();
            if (hasPostedMovement && !string.Equals(row.Currency_Code, requestedCurrency, StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "لا يمكن تغيير عملة صندوق لديه حركات مالية مرحلة. أنشئ صندوقاً جديداً للعملة الأخرى." });

            var old = new { row.Box_Name_AR, row.Currency_Code, row.Is_Active, row.Max_Limit, row.Min_Limit };
            row.Account_ID = dto.Account_ID.Trim();
            row.Box_Name_AR = dto.Box_Name_AR.Trim();
            row.Box_Name_EN = Text(dto.Box_Name_EN);
            row.Currency_Code = requestedCurrency;
            row.Max_Limit = dto.Max_Limit;
            row.Min_Limit = dto.Min_Limit;
            row.Notes = Text(dto.Notes);
            row.Updated_By = Session.User_ID.ToString();
            row.Updated_At = DateTime.UtcNow;
            _audit.Add(Session, HttpContext, "cash_boxes", row.Cash_Box_ID, "UPDATE", old, new { row.Box_Name_AR, row.Currency_Code, row.Is_Active, row.Max_Limit, row.Min_Limit });
            await _context.SaveChangesAsync();
            return Ok(row);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deactivate(string id, [FromBody] CashBoxStatusReasonDto? dto)
        {
            var error = await RequireAsync(ScreenOperation.Delete);
            if (error != null || Session == null) return error!;
            var reason = NormalizeReason(dto?.Reason);
            if (reason == null)
                return BadRequest(new { message = "سبب إيقاف الصندوق مطلوب، وبحد أقصى 500 حرف." });

            var row = await _context.Cash_Boxes.FirstOrDefaultAsync(x => x.Cash_Box_ID == id && x.Company_ID == Session.Company_ID && x.Branch_ID == Session.Branch_ID);
            if (row == null) return NotFound(new { message = "الصندوق غير موجود في نطاق الفرع الحالي." });
            if (!row.Is_Active) return BadRequest(new { message = "الصندوق موقوف مسبقاً." });

            var unposted = await _context.Journal_Entry_Details.AsNoTracking().AnyAsync(x =>
                x.Account_ID == row.Account_ID && x.JournalEntry.Is_Active && !x.JournalEntry.Is_Cancelled && !x.JournalEntry.Is_Posted);
            if (unposted)
                return BadRequest(new { message = "لا يمكن إيقاف الصندوق لوجود قيود أو سندات غير مرحلة مرتبطة به." });

            var currentBalance = await GetCurrentBalanceAsync(row.Account_ID);
            if (Math.Abs(currentBalance) > 0.009m)
                return BadRequest(new { message = $"لا يمكن إيقاف الصندوق لأن رصيده الدفتري الحالي {currentBalance:N2}. يجب تصفير الرصيد وإقفال العهدة أولاً." });

            await using var transaction = await _context.Database.BeginTransactionAsync();
            row.Is_Active = false;
            row.Updated_By = Session.User_ID.ToString();
            row.Updated_At = DateTime.UtcNow;
            _audit.Add(Session, HttpContext, "cash_boxes", row.Cash_Box_ID, "DEACTIVATE", new { Is_Active = true }, new { Is_Active = false }, reason);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return Ok(new { message = "تم إيقاف الصندوق بعد التحقق من أن رصيده صفر ولا توجد حركات معلقة." });
        }

        private async Task<string?> ValidateAsync(CreateCashBoxDto? dto, string? currentId)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Account_ID) || string.IsNullOrWhiteSpace(dto.Box_Name_AR) || string.IsNullOrWhiteSpace(dto.Currency_Code))
                return "حساب الصناديق واسم الصندوق والعملة حقول مطلوبة.";
            if (Session == null) return "الجلسة غير صالحة.";
            if (dto.Opening_Balance < 0) return "الرصيد الافتتاحي لا يمكن أن يكون سالباً.";
            if (dto.Min_Limit < 0 || dto.Max_Limit < 0 || dto.Min_Limit > dto.Max_Limit)
                return "حدود الصندوق غير صحيحة؛ يجب أن يكون الحد الأدنى أقل من أو يساوي الحد الأعلى.";

            var account = await _context.Chart_Of_Accounts.AsNoTracking().FirstOrDefaultAsync(x => x.Account_ID == dto.Account_ID && x.Company_ID == Session.Company_ID);
            if (account == null || !account.Is_Active || !account.Is_Postable || account.Is_Summary_Account || !account.Allow_ManualEntry ||
                !string.Equals(account.Account_Type, "Asset", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(account.Account_Category, "Cash", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(account.Normal_Balance, "Debit", StringComparison.OrdinalIgnoreCase))
                return "اختر حساب نقدية نهائياً ونشطاً وقابلاً للترحيل.";

            var currency = dto.Currency_Code.Trim().ToUpperInvariant();
            if (!await _context.Currencies.AnyAsync(x => x.Company_ID == Session.Company_ID && x.Currency_Code == currency && x.Is_Active))
                return "العملة المختارة غير فعالة في الشركة الحالية.";
            if (!string.IsNullOrWhiteSpace(account.Currency_Code) && !string.Equals(account.Currency_Code, currency, StringComparison.OrdinalIgnoreCase))
                return "عملة الصندوق يجب أن تطابق عملة الحساب المالي المختار.";
            var name = dto.Box_Name_AR.Trim();
            if (await _context.Cash_Boxes.AsNoTracking().AnyAsync(x => x.Company_ID == Session.Company_ID && x.Branch_ID == Session.Branch_ID && x.Cash_Box_ID != currentId && x.Box_Name_AR == name))
                return "يوجد صندوق آخر بالاسم نفسه في الفرع الحالي.";

            return null;
        }

        private async Task<bool> HasPostedMovementAsync(string accountId) =>
            await _context.Journal_Entry_Details.AsNoTracking().AnyAsync(x =>
                x.Account_ID == accountId
                && x.JournalEntry.Is_Posted
                && x.JournalEntry.Is_Active
                && !x.JournalEntry.Is_Cancelled
                && !x.JournalEntry.Is_Reversed);

        private async Task<decimal> GetCurrentBalanceAsync(string accountId) =>
            await _context.Journal_Entry_Details.AsNoTracking()
                .Where(x => x.Account_ID == accountId
                            && x.JournalEntry.Is_Posted
                            && x.JournalEntry.Is_Active
                            && !x.JournalEntry.Is_Cancelled
                            && !x.JournalEntry.Is_Reversed)
                .SumAsync(x => (decimal?)(x.Debit_Amount - x.Credit_Amount)) ?? 0m;

        private static string? NormalizeReason(string? reason)
        {
            var value = reason?.Trim();
            return string.IsNullOrWhiteSpace(value) || value.Length > 500 ? null : value;
        }

        private static string? Text(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
