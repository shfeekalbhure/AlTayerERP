using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// إدارة الأطراف المالية ضمن الشركة الحالية.
    /// تشمل العملاء والموردين والموظفين والوكلاء والجهات الأخرى المستخدمة في السندات.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public sealed class PartiesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PartiesController(AppDbContext context) => _context = context;

        private ServerSession? GetSession() =>
            HttpContext.Items["ServerSession"] as ServerSession;

        private IActionResult? RequireSession(out ServerSession? session)
        {
            session = GetSession();
            return session == null
                ? Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة. سجل الدخول من جديد." })
                : null;
        }

        private IActionResult? RequireSystemAdmin(out ServerSession? session)
        {
            var sessionError = RequireSession(out session);
            if (sessionError != null)
                return sessionError;

            return session!.Is_System_Admin ? null : Forbid();
        }

        /// <summary>
        /// يعيد أطراف الشركة الحالية فقط، دون كشف بيانات شركة أخرى.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var accessError = RequireSession(out var session);
            if (accessError != null || session == null)
                return accessError!;

            var parties = await _context.Parties
                .AsNoTracking()
                .Where(x => x.Company_ID == session.Company_ID)
                .OrderBy(x => x.Party_Name_AR)
                .ToListAsync();

            return Ok(parties);
        }

        /// <summary>
        /// إضافة أو تعديل طرف مالي. الشركة وبيانات التدقيق تؤخذ من جلسة الخادم.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SavePartyRequest request)
        {
            var accessError = RequireSystemAdmin(out var session);
            if (accessError != null || session == null)
                return accessError!;

            if (request == null ||
                string.IsNullOrWhiteSpace(request.Party_Code) ||
                string.IsNullOrWhiteSpace(request.Party_Name_AR) ||
                string.IsNullOrWhiteSpace(request.Party_Type))
            {
                return BadRequest(new { message = "كود الطرف والاسم العربي والنوع مطلوبة." });
            }

            var code = request.Party_Code.Trim();
            if (code.Length > 50 || request.Party_Name_AR.Trim().Length > 200)
                return BadRequest(new { message = "كود الطرف أو اسمه أطول من الحد المسموح به." });

            Party party;
            if (!string.IsNullOrWhiteSpace(request.Party_ID))
            {
                var existingParty = await _context.Parties
                    .FirstOrDefaultAsync(x => x.Party_ID == request.Party_ID && x.Company_ID == session.Company_ID);
                if (existingParty == null)
                    return NotFound(new { message = "الطرف غير موجود ضمن الشركة الحالية." });

                party = existingParty;
            }
            else
            {
                party = new Party
                {
                    // معرف داخلي غير قابل للتخمين؛ كود الطرف يظل هو الكود الظاهر للمستخدم.
                    Party_ID = Guid.NewGuid().ToString("N"),
                    Company_ID = session.Company_ID,
                    Created_At = DateTime.Now,
                    Created_By = session.User_ID.ToString()
                };
                _context.Parties.Add(party);
            }

            var duplicate = await _context.Parties.AnyAsync(x =>
                x.Company_ID == session.Company_ID &&
                x.Party_Code == code &&
                x.Party_ID != party.Party_ID);
            if (duplicate)
                return BadRequest(new { message = "كود الطرف مستخدم مسبقاً داخل الشركة." });

            party.Party_Code = code;
            party.Party_Name_AR = request.Party_Name_AR.Trim();
            party.Party_Name_EN = TrimOrNull(request.Party_Name_EN, 200);
            party.Party_Type = request.Party_Type.Trim();
            party.Mobile_No = TrimOrNull(request.Mobile_No, 30);
            party.Phone_No = TrimOrNull(request.Phone_No, 30);
            party.Identity_No = TrimOrNull(request.Identity_No, 100);
            party.Tax_No = TrimOrNull(request.Tax_No, 100);
            party.Address = TrimOrNull(request.Address, 500);
            party.Account_ID = TrimOrNull(request.Account_ID, 50);
            party.Credit_Limit = request.Credit_Limit < 0 ? 0 : request.Credit_Limit;
            party.Notes = TrimOrNull(request.Notes, 500);
            party.Is_Active = request.Is_Active;
            party.Updated_At = DateTime.Now;
            party.Updated_By = session.User_ID.ToString();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = string.IsNullOrWhiteSpace(request.Party_ID) ? "تمت إضافة الطرف المالي." : "تم تعديل الطرف المالي.",
                party.Party_ID
            });
        }

        private static string? TrimOrNull(string? value, int maxLength)
        {
            var result = value?.Trim();
            return string.IsNullOrWhiteSpace(result) ? null : result[..Math.Min(result.Length, maxLength)];
        }
    }

    /// <summary>
    /// عقد حفظ الطرف؛ لا يحتوي على Company_ID أو بيانات تدقيق لأنها مفروضة من الجلسة.
    /// </summary>
    public sealed class SavePartyRequest
    {
        public string? Party_ID { get; set; }
        public string Party_Code { get; set; } = string.Empty;
        public string Party_Name_AR { get; set; } = string.Empty;
        public string? Party_Name_EN { get; set; }
        public string Party_Type { get; set; } = string.Empty;
        public string? Mobile_No { get; set; }
        public string? Phone_No { get; set; }
        public string? Identity_No { get; set; }
        public string? Tax_No { get; set; }
        public string? Address { get; set; }
        public string? Account_ID { get; set; }
        public decimal Credit_Limit { get; set; }
        public string? Notes { get; set; }
        public bool Is_Active { get; set; } = true;
    }
}
