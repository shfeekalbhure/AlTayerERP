using AlTayerERP.API.Services;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// منسدلات شاشة الصناديق من نطاق الجلسة نفسها.
    /// لا تتطلب صلاحيات مستقلة لشاشات العملات أو دليل الحسابات.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/CashBoxes/Lookups")]
    public sealed class CashBoxLookupsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ScreenAuthorizationService _authorization;

        public CashBoxLookupsController(
            AppDbContext context,
            ScreenAuthorizationService authorization)
        {
            _context = context;
            _authorization = authorization;
        }

        private ServerSession? Session =>
            HttpContext.Items["ServerSession"] as ServerSession;

        [HttpGet]
        public async Task<IActionResult> GetLookups()
        {
            if (Session == null)
                return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة." });

            if (!await _authorization.IsAllowedAsync(
                    Session,
                    "CashBoxes",
                    ScreenOperation.View))
            {
                return Forbid();
            }

            var branch = await _context.Tenant_Branches.AsNoTracking()
                .Where(x =>
                    x.Company_ID == Session.Company_ID &&
                    x.Branch_ID == Session.Branch_ID &&
                    x.Is_Active)
                .Select(x => new
                {
                    x.Branch_ID,
                    x.Branch_Code,
                    x.Branch_Name
                })
                .FirstOrDefaultAsync();

            if (branch == null)
                return BadRequest(new { message = "فرع الجلسة الحالي غير موجود أو موقوف." });

            var currencies = await _context.Currencies.AsNoTracking()
                .Where(x =>
                    x.Company_ID == Session.Company_ID &&
                    x.Is_Active)
                .OrderByDescending(x => x.Is_Default)
                .ThenByDescending(x => x.Is_Local_Currency)
                .ThenBy(x => x.Currency_Code)
                .Select(x => new
                {
                    x.Currency_Code,
                    x.Currency_Name_AR,
                    x.Currency_Name_EN,
                    x.Is_Default,
                    x.Is_Local_Currency
                })
                .ToListAsync();

            var accounts = await _context.Chart_Of_Accounts.AsNoTracking()
                .Where(x =>
                    x.Company_ID == Session.Company_ID &&
                    x.Is_Active &&
                    !x.Is_Postable &&
                    x.Is_Summary_Account &&
                    (x.Account_Category == "Cash" ||
                     x.Account_Name_AR.Contains("صندوق") ||
                     x.Account_Name_AR.Contains("نقد")))
                .OrderBy(x => x.Account_Code)
                .Select(x => new
                {
                    x.Account_ID,
                    x.Account_Code,
                    x.Account_Name_AR,
                    x.Account_Name_EN,
                    Display_Name = $"{x.Account_Code} - {x.Account_Name_AR}"
                })
                .ToListAsync();

            return Ok(new
            {
                Branches = new[] { branch },
                Currencies = currencies,
                Accounts = accounts
            });
        }
    }
}
