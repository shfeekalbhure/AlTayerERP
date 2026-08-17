using AlTayerERP.API.Services;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers;

[ApiController]
[Route("api/accounting-reports")]
public sealed class AccountingReportsController : ControllerBase
{
    private readonly AppDbContext _db; private readonly ScreenAuthorizationService _authorization;
    public AccountingReportsController(AppDbContext db, ScreenAuthorizationService authorization){_db=db;_authorization=authorization;}
    private ServerSession Session()=>HttpContext.Items["ServerSession"] as ServerSession??throw new InvalidOperationException("جلسة الخادم غير متاحة.");
    private async Task<IActionResult?> Allow(string screen){return await _authorization.IsAllowedAsync(Session(),screen,ScreenOperation.View)?null:Forbid();}

    [HttpGet("trial-balance")]
    public async Task<IActionResult> TrialBalance([FromQuery]DateTime? toDate=null, [FromQuery]int? currencyId=null, [FromQuery]string? costCenterId=null)
    {
        var denial=await Allow("TrialBalance");if(denial!=null)return denial;var s=Session();var date=(toDate??DateTime.UtcNow.Date).Date;
        var rows=await (from d in _db.Financial_Voucher_Details.AsNoTracking()
                        join h in _db.Financial_Voucher_Headers.AsNoTracking() on d.Voucher_ID equals h.Voucher_ID
                        join a in _db.Chart_Of_Accounts.AsNoTracking() on d.Account_ID equals a.Account_ID
                        where h.Is_Active&&h.Is_Posted&&h.Branch_ID==s.Branch_ID&&h.Fiscal_Year_ID==s.Year_ID&&h.Voucher_Date<=date&&a.Company_ID==s.Company_ID&&(currencyId==null||d.Currency_ID==currencyId.Value)&&(string.IsNullOrWhiteSpace(costCenterId)||d.Cost_Center_ID==costCenterId.Trim())
                        group d by new{a.Account_ID,a.Account_Code,a.Account_Name_AR} into g
                        select new{g.Key.Account_ID,g.Key.Account_Code,g.Key.Account_Name_AR,Debit=g.Sum(x=>x.Debit_Amount),Credit=g.Sum(x=>x.Credit_Amount)}).OrderBy(x=>x.Account_Code).ToListAsync();
        return Ok(new{asOfDate=date,rows,totalDebit=rows.Sum(x=>x.Debit),totalCredit=rows.Sum(x=>x.Credit)});
    }

    [HttpGet("general-ledger")]
    public async Task<IActionResult> GeneralLedger([FromQuery]string accountId,[FromQuery]DateTime? fromDate=null,[FromQuery]DateTime? toDate=null,[FromQuery]int? currencyId=null,[FromQuery]string? costCenterId=null)
    {
        var denial=await Allow("GeneralLedger");if(denial!=null)return denial;if(string.IsNullOrWhiteSpace(accountId))return BadRequest(new{message="الحساب مطلوب."});
        var s=Session();var startDate=(fromDate??new DateTime(DateTime.UtcNow.Year,1,1)).Date;var to=(toDate??DateTime.UtcNow).Date;
        var account=await _db.Chart_Of_Accounts.AsNoTracking().Where(x=>x.Account_ID==accountId.Trim()&&x.Company_ID==s.Company_ID).Select(x=>new{x.Account_ID,x.Account_Code,x.Account_Name_AR}).SingleOrDefaultAsync();if(account==null)return NotFound();
        var opening=await ((from d in _db.Financial_Voucher_Details.AsNoTracking() join h in _db.Financial_Voucher_Headers.AsNoTracking() on d.Voucher_ID equals h.Voucher_ID
                           where h.Is_Active&&h.Is_Posted&&h.Branch_ID==s.Branch_ID&&h.Fiscal_Year_ID==s.Year_ID&&d.Account_ID==accountId.Trim()&&h.Voucher_Date<startDate&&(currencyId==null||d.Currency_ID==currencyId.Value)&&(string.IsNullOrWhiteSpace(costCenterId)||d.Cost_Center_ID==costCenterId.Trim())
                           select d.Debit_Amount-d.Credit_Amount).DefaultIfEmpty().SumAsync());
        var entries=await ((from d in _db.Financial_Voucher_Details.AsNoTracking() join h in _db.Financial_Voucher_Headers.AsNoTracking() on d.Voucher_ID equals h.Voucher_ID
                           where h.Is_Active&&h.Is_Posted&&h.Branch_ID==s.Branch_ID&&h.Fiscal_Year_ID==s.Year_ID&&d.Account_ID==accountId.Trim()&&h.Voucher_Date>=startDate&&h.Voucher_Date<=to&&(currencyId==null||d.Currency_ID==currencyId.Value)&&(string.IsNullOrWhiteSpace(costCenterId)||d.Cost_Center_ID==costCenterId.Trim())
                           orderby h.Voucher_Date,h.Voucher_No,d.Line_No select new{h.Voucher_Date,h.Voucher_No,d.Line_No,d.Description,d.Reference_No,d.Debit_Amount,d.Credit_Amount}).ToListAsync());
        decimal running=opening;var rows=entries.Select(x=>new{x.Voucher_Date,x.Voucher_No,x.Line_No,x.Description,x.Reference_No,x.Debit_Amount,x.Credit_Amount,Balance=running+=x.Debit_Amount-x.Credit_Amount}).ToList();
        return Ok(new{account,fromDate=startDate,toDate=to,openingBalance=opening,rows,closingBalance=running});
    }
}
