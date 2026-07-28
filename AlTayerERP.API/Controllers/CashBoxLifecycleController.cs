using AlTayerERP.API.Services;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>عمليات دورة حياة الصندوق التي لا تدخل ضمن الحفظ العام.</summary>
    [Authorize]
    [ApiController]
    [Route("api/CashBoxes")]
    public sealed class CashBoxLifecycleController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ScreenAuthorizationService _authorization;
        private readonly AuditTrailService _audit;

        public CashBoxLifecycleController(
            AppDbContext context,
            ScreenAuthorizationService authorization,
            AuditTrailService audit)
        {
            _context = context;
            _authorization = authorization;
            _audit = audit;
        }

        private ServerSession? Session =>
            HttpContext.Items["ServerSession"] as ServerSession;

        [HttpPost("{id}/reactivate")]
        public async Task<IActionResult> Reactivate(string id, [FromBody] CashBoxStatusReasonDto? dto)
        {
            if (Session == null)
                return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة." });

            if (!await _authorization.IsAllowedAsync(Session, "CashBoxes", ScreenOperation.Reactivate))
                return Forbid();

            if (dto == null || string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest(new { message = "سبب إعادة تفعيل الصندوق مطلوب." });

            var row = await _context.Cash_Boxes.FirstOrDefaultAsync(x =>
                x.Cash_Box_ID == id &&
                x.Company_ID == Session.Company_ID &&
                x.Branch_ID == Session.Branch_ID);

            if (row == null)
                return NotFound(new { message = "الصندوق غير موجود في نطاق الفرع الحالي." });

            if (row.Is_Active)
                return BadRequest(new { message = "الصندوق فعال مسبقاً." });

            var account = await _context.Chart_Of_Accounts.FirstOrDefaultAsync(x =>
                x.Account_ID == row.Account_ID &&
                x.Company_ID == Session.Company_ID);

            if (account == null)
                return BadRequest(new { message = "الحساب المرتبط بالصندوق غير موجود؛ يلزم تصحيح الربط قبل إعادة التفعيل." });

            var parentIsValid = await _context.Chart_Of_Accounts.AsNoTracking().AnyAsync(x =>
                x.Account_ID == account.Parent_Account_ID &&
                x.Company_ID == Session.Company_ID &&
                x.Is_Active &&
                x.Is_Summary_Account &&
                !x.Is_Postable &&
                (x.Account_Category == "Cash" ||
                 x.Account_Name_AR.Contains("صندوق") ||
                 x.Account_Name_AR.Contains("نقد")));

            if (!parentIsValid)
                return BadRequest(new { message = "لا يمكن إعادة التفعيل لأن حساب الصناديق الأب موقوف أو غير تجميعي." });

            var currencyIsActive = await _context.Currencies.AsNoTracking().AnyAsync(x =>
                x.Company_ID == Session.Company_ID &&
                x.Currency_Code == row.Currency_Code &&
                x.Is_Active);

            if (!currencyIsActive)
                return BadRequest(new { message = "لا يمكن إعادة التفعيل قبل تفعيل عملة الصندوق." });

            await using var transaction = await _context.Database.BeginTransactionAsync();

            row.Is_Active = true;
            row.Updated_By = Session.User_ID.ToString();
            row.Updated_At = DateTime.UtcNow;

            account.Is_Active = true;
            account.Updated_By = Session.User_ID.ToString();
            account.Updated_At = DateTime.UtcNow;

            _audit.Add(
                Session,
                HttpContext,
                "cash_boxes",
                row.Cash_Box_ID,
                "REACTIVATE",
                new { Is_Active = false },
                new { Is_Active = true },
                dto.Reason.Trim());

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = "تمت إعادة تفعيل الصندوق وحسابه المرتبط بنجاح." });
        }

        /// <summary>يسجل معاينة طباعة قائمة الصناديق في التدقيق المركزي دون تعديل أي رصيد.</summary>
        [HttpPost("print")]
        public async Task<IActionResult> RegisterListPrint()
        {
            if (Session == null)
                return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة." });

            if (!await _authorization.IsAllowedAsync(Session, "CashBoxes", ScreenOperation.Print))
                return Forbid();

            _audit.Add(Session, HttpContext, "cash_boxes", "LIST", "PRINT", notes: "فتح معاينة طباعة قائمة الصناديق.");
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم تسجيل عملية الطباعة." });
        }
    }

    public sealed class CashBoxStatusReasonDto
    {
        public string Reason { get; set; } = string.Empty;
    }
}
