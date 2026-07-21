using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// إدارة السياسات والسقوف المالية للشركة الحالية.
    /// نطاق الشركة يؤخذ من الجلسة ولا يقبل من الواجهة.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public sealed class FinancialPoliciesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public FinancialPoliciesController(AppDbContext context) => _context = context;

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

            return Ok(await _context.Financial_Policies.AsNoTracking()
                .Where(x => x.Company_ID == session.Company_ID)
                .OrderBy(x => x.Entity_Type).ThenBy(x => x.Limit_Type).ThenBy(x => x.Limit_ID)
                .ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SaveFinancialPolicyRequest request)
        {
            var error = RequireSystemAdmin(out var session);
            if (error != null || session == null) return error!;

            if (request == null || string.IsNullOrWhiteSpace(request.Entity_Type) ||
                string.IsNullOrWhiteSpace(request.Entity_ID) ||
                string.IsNullOrWhiteSpace(request.Limit_Type) ||
                string.IsNullOrWhiteSpace(request.Currency_Code))
                return BadRequest(new { message = "الجهة ورقمها ونوع السياسة والعملة حقول مطلوبة." });

            if (request.Limit_Amount < 0)
                return BadRequest(new { message = "قيمة السقف لا يمكن أن تكون سالبة." });

            var row = request.Limit_ID > 0
                ? await _context.Financial_Policies.FirstOrDefaultAsync(x =>
                    x.Limit_ID == request.Limit_ID && x.Company_ID == session.Company_ID)
                : null;

            if (request.Limit_ID > 0 && row == null)
                return NotFound(new { message = "السياسة غير موجودة ضمن الشركة الحالية." });

            if (row == null)
            {
                row = new FinancialPolicy { Company_ID = session.Company_ID, Created_At = DateTime.Now };
                _context.Financial_Policies.Add(row);
            }

            row.Entity_Type = request.Entity_Type.Trim();
            row.Entity_ID = request.Entity_ID.Trim();
            row.Limit_Type = request.Limit_Type.Trim();
            row.Currency_Code = request.Currency_Code.Trim().ToUpperInvariant();
            row.Limit_Amount = request.Limit_Amount;
            row.Period_Type = string.IsNullOrWhiteSpace(request.Period_Type) ? "Monthly" : request.Period_Type.Trim();
            row.Requires_Approval = request.Requires_Approval;
            row.Is_Active = request.Is_Active;
            row.Updated_At = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(new { message = "تم حفظ السياسة المالية.", row.Limit_ID });
        }
    }

    public sealed class SaveFinancialPolicyRequest
    {
        public int Limit_ID { get; set; }
        public string Entity_Type { get; set; } = string.Empty;
        public string Entity_ID { get; set; } = string.Empty;
        public string Limit_Type { get; set; } = string.Empty;
        public string Currency_Code { get; set; } = string.Empty;
        public decimal Limit_Amount { get; set; }
        public string? Period_Type { get; set; }
        public bool Requires_Approval { get; set; } = true;
        public bool Is_Active { get; set; } = true;
    }
}
