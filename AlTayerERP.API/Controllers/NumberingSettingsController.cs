using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// إعدادات ومحرك الترقيم المركزي (NumberingSettings).
    /// لا يقبل الشركة أو الفرع أو السنة من العميل عند الحجز؛ تؤخذ من ServerSession.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public sealed class NumberingSettingsController : ControllerBase
    {
        private static readonly HashSet<string> ResetTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "NONE", "COMPANY", "BRANCH", "YEAR", "COMPANYYEAR", "BRANCHYEAR"
        };

        private readonly AppDbContext _context;
        private readonly NumberGeneratorService _numbers;
        private readonly ScreenAuthorizationService _authorization;
        private readonly AuditTrailService _audit;

        public NumberingSettingsController(
            AppDbContext context,
            NumberGeneratorService numbers,
            ScreenAuthorizationService authorization,
            AuditTrailService audit)
        {
            _context = context;
            _numbers = numbers;
            _authorization = authorization;
            _audit = audit;
        }

        private async Task<IActionResult?> DenyUnlessAsync(ScreenOperation operation)
        {
            if (HttpContext.Items["ServerSession"] is not ServerSession session)
                return Unauthorized();
            return await _authorization.IsAllowedAsync(session, "NumberingSettings", operation)
                ? null : Forbid();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var denied = await DenyUnlessAsync(ScreenOperation.View);
            if (denied != null) return denied;

            var data = await _context.Numbering_Settings.AsNoTracking()
                .OrderBy(x => x.Document_Type)
                .Select(x => new
                {
                    x.Numbering_ID, x.Document_Type, x.Prefix, x.Digits_Count, x.Reset_Type,
                    x.Use_Company, x.Use_Branch, x.Use_Year, x.Is_Active
                })
                .ToListAsync();
            return Ok(data);
        }

        /// <summary>
        /// حفظ قاعدة الترقيم فقط. لا يسمح بتعديل Last_Number لأن المصدر الوحيد له هو العداد.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SaveNumberingSettingRequest request)
        {
            var denied = await DenyUnlessAsync(request?.Numbering_ID > 0 ? ScreenOperation.Edit : ScreenOperation.Add);
            if (denied != null) return denied;
            var session = (ServerSession)HttpContext.Items["ServerSession"]!;

            if (request == null || string.IsNullOrWhiteSpace(request.Document_Type) ||
                string.IsNullOrWhiteSpace(request.Prefix))
                return BadRequest("نوع المستند والبادئة مطلوبان.");

            var documentType = request.Document_Type.Trim().ToUpperInvariant();
            var prefix = request.Prefix.Trim().ToUpperInvariant();
            var resetType = NormalizeResetType(request.Reset_Type);
            if (!ResetTypes.Contains(resetType))
                return BadRequest("طريقة التصفير غير صالحة.");
            if (request.Digits_Count is < 1 or > 9)
                return BadRequest("عدد الخانات يجب أن يكون من 1 إلى 9.");

            var duplicate = await _context.Numbering_Settings.AnyAsync(x =>
                x.Document_Type == documentType && x.Numbering_ID != request.Numbering_ID);
            if (duplicate) return Conflict("يوجد إعداد ترقيم لهذا النوع مسبقاً.");

            NumberingSetting setting;
            object? before = null;
            if (request.Numbering_ID > 0)
            {
                setting = await _context.Numbering_Settings.FirstOrDefaultAsync(x => x.Numbering_ID == request.Numbering_ID)
                    ?? throw new KeyNotFoundException("إعداد الترقيم غير موجود.");
                before = new { setting.Document_Type, setting.Prefix, setting.Digits_Count, setting.Reset_Type, setting.Is_Active };
            }
            else
            {
                setting = new NumberingSetting();
                _context.Numbering_Settings.Add(setting);
            }

            var flags = ResolveResetFlags(resetType);
            setting.Document_Type = documentType;
            setting.Prefix = prefix;
            setting.Digits_Count = request.Digits_Count;
            setting.Reset_Type = resetType;
            setting.Use_Company = flags.UseCompany;
            setting.Use_Branch = flags.UseBranch;
            setting.Use_Year = flags.UseYear;
            setting.Is_Active = request.Is_Active;

            _audit.Add(session, HttpContext, "numbering_settings",
                request.Numbering_ID == 0 ? "new" : request.Numbering_ID.ToString(),
                request.Numbering_ID == 0 ? "CREATE" : "UPDATE", before,
                new { setting.Document_Type, setting.Prefix, setting.Digits_Count, setting.Reset_Type, setting.Is_Active });

            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = request.Numbering_ID == 0 ? "تمت إضافة إعداد الترقيم." : "تم تعديل إعداد الترقيم.",
                setting.Numbering_ID
            });
        }

        /// <summary>
        /// إيقاف الإعداد فقط؛ لا يحذف العدادات أو الأرقام المحجوزة حفاظاً على عدم إعادة الاستخدام.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var denied = await DenyUnlessAsync(ScreenOperation.Delete);
            if (denied != null) return denied;
            var session = (ServerSession)HttpContext.Items["ServerSession"]!;

            var setting = await _context.Numbering_Settings.FirstOrDefaultAsync(x => x.Numbering_ID == id);
            if (setting == null) return NotFound("إعداد الترقيم غير موجود.");

            setting.Is_Active = false;
            _audit.Add(session, HttpContext, "numbering_settings", id.ToString(), "DEACTIVATE",
                new { Is_Active = true }, new { Is_Active = false });
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم إيقاف إعداد الترقيم. لم تُحذف أي أرقام أو عدادات." });
        }

        /// <summary>
        /// يحجز الرقم النهائي. POST وليس GET لأن الحجز عملية تغير حالة العداد.
        /// الرقم المحجوز لا يرجع إلى العداد عند الإلغاء أو حذف المسودة.
        /// </summary>
        [HttpPost("Reserve")]
        public async Task<IActionResult> Reserve([FromBody] ReserveNumberRequest request, CancellationToken cancellationToken)
        {
            var denied = await DenyUnlessAsync(ScreenOperation.Add);
            if (denied != null) return denied;
            var session = (ServerSession)HttpContext.Items["ServerSession"]!;

            if (request == null || string.IsNullOrWhiteSpace(request.Document_Type))
                return BadRequest("نوع المستند مطلوب.");

            try
            {
                var reservation = await _numbers.ReserveNextNumberAsync(
                    request.Document_Type, session.Company_ID, session.Branch_ID, session.Year_ID, cancellationToken);

                _audit.Add(session, HttpContext, "numbering_counters",
                    reservation.Counter_ID.ToString(), "NUMBER_RESERVED",
                    newValues: new
                    {
                        reservation.Document_Number, reservation.Document_Type,
                        reservation.Serial_Number, reservation.Company_ID,
                        reservation.Branch_ID, reservation.Fiscal_Year_ID
                    });
                await _audit.SaveChangesAsync(cancellationToken);

                return Ok(reservation);
            }
            catch (NumberingException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private static string NormalizeResetType(string? value) =>
            (value ?? string.Empty).Trim().Replace("_", string.Empty).Replace("-", string.Empty).ToUpperInvariant();

        private static (bool UseCompany, bool UseBranch, bool UseYear) ResolveResetFlags(string resetType) =>
            resetType switch
            {
                "NONE" => (false, false, false),
                "COMPANY" => (true, false, false),
                "BRANCH" => (true, true, false),
                "YEAR" => (false, false, true),
                "COMPANYYEAR" => (true, false, true),
                "BRANCHYEAR" => (true, true, true),
                _ => throw new InvalidOperationException("طريقة تصفير غير مدعومة.")
            };
    }

    /// <summary>عقد حفظ الإعداد؛ Last_Number غير موجود عمداً لأنه لا يعدل من UI.</summary>
    public sealed class SaveNumberingSettingRequest
    {
        public int Numbering_ID { get; set; }
        public string Document_Type { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;
        public int Digits_Count { get; set; } = 6;
        public string Reset_Type { get; set; } = "BRANCHYEAR";
        public bool Is_Active { get; set; } = true;
    }

    public sealed class ReserveNumberRequest
    {
        public string Document_Type { get; set; } = string.Empty;
    }
}
