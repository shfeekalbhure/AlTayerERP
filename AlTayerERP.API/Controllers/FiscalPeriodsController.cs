using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// إدارة الفترات المالية داخل سياق الشركة والفرع والسنة من جلسة الخادم.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public sealed class FiscalPeriodsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FiscalPeriodsController(AppDbContext context) => _context = context;

        private ServerSession? Session() =>
            HttpContext.Items["ServerSession"] as ServerSession;

        private IActionResult? RequireSystemAdmin(out ServerSession? session)
        {
            session = Session();
            if (session == null)
                return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة. سجل الدخول من جديد." });

            return session.Is_System_Admin ? null : Forbid();
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var accessError = RequireSystemAdmin(out var session);
            if (accessError != null || session == null)
                return accessError!;

            var periods = await _context.Fiscal_Periods
                .AsNoTracking()
                .Where(x => x.Company_ID == session.Company_ID &&
                            x.Branch_ID == session.Branch_ID &&
                            x.Fiscal_Year_ID == session.Year_ID)
                .OrderBy(x => x.Start_Date)
                .ToListAsync();

            return Ok(periods);
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SaveFiscalPeriodRequest request)
        {
            var accessError = RequireSystemAdmin(out var session);
            if (accessError != null || session == null)
                return accessError!;

            if (request == null ||
                string.IsNullOrWhiteSpace(request.Period_Code) ||
                string.IsNullOrWhiteSpace(request.Period_Name))
            {
                return BadRequest(new { message = "كود الفترة واسمها مطلوبان." });
            }

            var startDate = request.Start_Date.Date;
            var endDate = request.End_Date.Date;
            if (endDate < startDate)
                return BadRequest(new { message = "تاريخ نهاية الفترة لا يمكن أن يسبق تاريخ البداية." });

            if (request.Is_Closed && string.IsNullOrWhiteSpace(request.Close_Reason))
                return BadRequest(new { message = "سبب الإقفال مطلوب عند إقفال الفترة." });

            FiscalPeriod period;
            if (request.Fiscal_Period_ID > 0)
            {
                var existing = await _context.Fiscal_Periods.FirstOrDefaultAsync(x =>
                    x.Fiscal_Period_ID == request.Fiscal_Period_ID &&
                    x.Company_ID == session.Company_ID &&
                    x.Branch_ID == session.Branch_ID &&
                    x.Fiscal_Year_ID == session.Year_ID);
                if (existing == null)
                    return NotFound(new { message = "الفترة المالية غير موجودة ضمن نطاق الجلسة." });

                period = existing;
            }
            else
            {
                period = new FiscalPeriod
                {
                    Company_ID = session.Company_ID,
                    Branch_ID = session.Branch_ID,
                    Fiscal_Year_ID = session.Year_ID,
                    Created_At = DateTime.Now
                };
                _context.Fiscal_Periods.Add(period);
            }

            var code = request.Period_Code.Trim();
            var duplicateCode = await _context.Fiscal_Periods.AnyAsync(x =>
                x.Company_ID == session.Company_ID &&
                x.Branch_ID == session.Branch_ID &&
                x.Fiscal_Year_ID == session.Year_ID &&
                x.Period_Code == code &&
                x.Fiscal_Period_ID != period.Fiscal_Period_ID);
            if (duplicateCode)
                return BadRequest(new { message = "كود الفترة مستخدم مسبقاً في السنة الحالية." });

            // لا يسمح بتداخل فترتين فعالتين داخل الفرع والسنة نفسيهما.
            var overlap = await _context.Fiscal_Periods.AnyAsync(x =>
                x.Company_ID == session.Company_ID &&
                x.Branch_ID == session.Branch_ID &&
                x.Fiscal_Year_ID == session.Year_ID &&
                x.Fiscal_Period_ID != period.Fiscal_Period_ID &&
                x.Is_Active &&
                startDate <= x.End_Date &&
                endDate >= x.Start_Date);
            if (overlap)
                return BadRequest(new { message = "الفترة تتداخل مع فترة مالية فعالة موجودة." });

            period.Period_Code = code;
            period.Period_Name = request.Period_Name.Trim();
            period.Start_Date = startDate;
            period.End_Date = endDate;
            period.Is_Closed = request.Is_Closed;
            period.Close_Date = request.Is_Closed
                ? (request.Close_Date ?? DateTime.Today).Date
                : null;
            period.Close_Reason = request.Is_Closed ? request.Close_Reason?.Trim() : null;
            period.Is_Active = request.Is_Active;
            period.Updated_At = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = request.Fiscal_Period_ID > 0 ? "تم تعديل الفترة المالية." : "تمت إضافة الفترة المالية.",
                period.Fiscal_Period_ID
            });
        }
    }

    public sealed class SaveFiscalPeriodRequest
    {
        public int Fiscal_Period_ID { get; set; }
        public string Period_Code { get; set; } = string.Empty;
        public string Period_Name { get; set; } = string.Empty;
        public DateTime Start_Date { get; set; }
        public DateTime End_Date { get; set; }
        public bool Is_Closed { get; set; }
        public DateTime? Close_Date { get; set; }
        public string? Close_Reason { get; set; }
        public bool Is_Active { get; set; } = true;
    }
}
