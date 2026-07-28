using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>الفترات المالية؛ الحفظ لا ينفذ الإقفال، بل تستخدم إجراءات Close/Reopen المدققة.</summary>
    [Authorize, ApiController, Route("api/[controller]")]
    public sealed class FiscalPeriodsController : ControllerBase
    {
        private readonly AppDbContext _context; private readonly ScreenAuthorizationService _authorization; private readonly AuditTrailService _audit;
        public FiscalPeriodsController(AppDbContext context, ScreenAuthorizationService authorization, AuditTrailService audit) { _context=context; _authorization=authorization; _audit=audit; }
        private ServerSession? Session => HttpContext.Items["ServerSession"] as ServerSession;
        private async Task<IActionResult?> RequireAsync(ScreenOperation op)
        {
            if (Session == null) return Unauthorized(new { message="انتهت الجلسة أو أنها غير صالحة." });
            return await _authorization.IsAllowedAsync(Session,"FiscalPeriods",op) ? null : Forbid();
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var e=await RequireAsync(ScreenOperation.View); if(e!=null||Session==null)return e!;
            return Ok(await _context.Fiscal_Periods.AsNoTracking().Where(x=>x.Company_ID==Session.Company_ID&&x.Branch_ID==Session.Branch_ID&&x.Fiscal_Year_ID==Session.Year_ID).OrderBy(x=>x.Start_Date).ToListAsync());
        }
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SaveFiscalPeriodRequest? r)
        {
            if (r == null)
                return BadRequest(new { message = "بيانات الفترة المالية مطلوبة." });

            var e = await RequireAsync(r.Fiscal_Period_ID > 0 ? ScreenOperation.Edit : ScreenOperation.Add);
            if (e != null || Session == null) return e!;

            if (string.IsNullOrWhiteSpace(r.Period_Code) ||
                string.IsNullOrWhiteSpace(r.Period_Name) ||
                r.End_Date.Date < r.Start_Date.Date)
            {
                return BadRequest(new { message = "كود واسم وتواريخ الفترة الصحيحة مطلوبة." });
            }
            var year=await _context.Fiscal_Years.AsNoTracking().FirstOrDefaultAsync(x=>x.Fiscal_Year_ID==Session.Year_ID&&x.Company_ID==Session.Company_ID&&x.Is_Active&&!x.Is_Closed);
            if(year==null)return Conflict(new {message="السنة الحالية غير فعالة أو مقفلة."});
            if(r.Start_Date.Date<year.Start_Date.Date||r.End_Date.Date>year.End_Date.Date)return BadRequest(new {message="تواريخ الفترة يجب أن تقع داخل السنة المالية."});
            var row=r.Fiscal_Period_ID>0?await _context.Fiscal_Periods.FirstOrDefaultAsync(x=>x.Fiscal_Period_ID==r.Fiscal_Period_ID&&x.Company_ID==Session.Company_ID&&x.Branch_ID==Session.Branch_ID&&x.Fiscal_Year_ID==Session.Year_ID):null;
            if(r.Fiscal_Period_ID>0&&row==null)return NotFound(new {message="الفترة غير موجودة ضمن نطاق الجلسة."});
            if(row?.Is_Closed==true)return Conflict(new {message="لا تعدّل فترة مقفلة؛ استخدم إعادة الفتح المدققة أولاً."});
            var code=r.Period_Code.Trim();
            if(await _context.Fiscal_Periods.AnyAsync(x=>x.Company_ID==Session.Company_ID&&x.Branch_ID==Session.Branch_ID&&x.Fiscal_Year_ID==Session.Year_ID&&x.Period_Code==code&&x.Fiscal_Period_ID!=r.Fiscal_Period_ID))return Conflict(new {message="كود الفترة مستخدم مسبقاً."});
            if(await _context.Fiscal_Periods.AnyAsync(x=>x.Company_ID==Session.Company_ID&&x.Branch_ID==Session.Branch_ID&&x.Fiscal_Year_ID==Session.Year_ID&&x.Fiscal_Period_ID!=r.Fiscal_Period_ID&&x.Is_Active&&r.Start_Date.Date<=x.End_Date&&r.End_Date.Date>=x.Start_Date))return Conflict(new {message="الفترة تتداخل مع فترة فعالة."});
            var old=row==null?null:new {row.Period_Code,row.Period_Name,row.Start_Date,row.End_Date,row.Is_Active};
            if(row==null){row=new FiscalPeriod {Company_ID=Session.Company_ID,Branch_ID=Session.Branch_ID,Fiscal_Year_ID=Session.Year_ID,Created_At=DateTime.UtcNow};_context.Fiscal_Periods.Add(row);}
            row.Period_Code=code;row.Period_Name=r.Period_Name.Trim();row.Start_Date=r.Start_Date.Date;row.End_Date=r.End_Date.Date;row.Is_Active=r.Is_Active;row.Updated_At=DateTime.UtcNow;
            _audit.Add(Session,HttpContext,"fiscal_periods",r.Fiscal_Period_ID>0?r.Fiscal_Period_ID.ToString():code,r.Fiscal_Period_ID>0?"UPDATE":"CREATE",old,new {row.Period_Code,row.Period_Name,row.Start_Date,row.End_Date,row.Is_Active});
            await _context.SaveChangesAsync();return Ok(new {message="تم حفظ الفترة المالية.",row.Fiscal_Period_ID});
        }
        [HttpPost("{id:int}/Close")]
        public async Task<IActionResult> Close(int id,[FromBody] FiscalPeriodLifecycleRequest r)
        {
            var e=await RequireAsync(ScreenOperation.Approve);if(e!=null||Session==null)return e!;
            if(string.IsNullOrWhiteSpace(r?.Reason))return BadRequest(new {message="سبب الإقفال مطلوب."});
            var row=await FindAsync(id);if(row==null)return NotFound(new {message="الفترة غير موجودة."});
            if(row.Is_Closed)return Conflict(new {message="الفترة مقفلة بالفعل."});
            if(await _context.Financial_Voucher_Headers.AnyAsync(x=>x.Fiscal_Year_ID==Session.Year_ID&&x.Branch_ID==Session.Branch_ID.ToString()&&x.Is_Active&&!x.Is_Posted&&x.Voucher_Date>=row.Start_Date&&x.Voucher_Date<=row.End_Date))return Conflict(new {message="يوجد مستندات معلقة غير مرحلة ضمن الفترة."});
            row.Is_Closed=true;row.Close_Date=DateTime.UtcNow.Date;row.Close_Reason=r.Reason.Trim();row.Updated_At=DateTime.UtcNow;
            _audit.Add(Session,HttpContext,"fiscal_periods",id.ToString(),"CLOSE",new {Is_Closed=false},new {Is_Closed=true},r.Reason);await _context.SaveChangesAsync();return Ok(new {message="تم إقفال الفترة."});
        }
        [HttpPost("{id:int}/Reopen")]
        public async Task<IActionResult> Reopen(int id,[FromBody] FiscalPeriodLifecycleRequest r)
        {
            var e=await RequireAsync(ScreenOperation.Unapprove);if(e!=null||Session==null)return e!;
            if(string.IsNullOrWhiteSpace(r?.Reason))return BadRequest(new {message="سبب إعادة الفتح مطلوب."});
            var row=await FindAsync(id);if(row==null)return NotFound(new {message="الفترة غير موجودة."});
            if(!row.Is_Closed)return Conflict(new {message="الفترة مفتوحة بالفعل."});
            row.Is_Closed=false;row.Close_Date=null;row.Close_Reason=null;row.Updated_At=DateTime.UtcNow;
            _audit.Add(Session,HttpContext,"fiscal_periods",id.ToString(),"REOPEN",new {Is_Closed=true},new {Is_Closed=false},r.Reason);await _context.SaveChangesAsync();return Ok(new {message="تمت إعادة فتح الفترة."});
        }
        private Task<FiscalPeriod?> FindAsync(int id)=>_context.Fiscal_Periods.FirstOrDefaultAsync(x=>Session!=null&&x.Fiscal_Period_ID==id&&x.Company_ID==Session.Company_ID&&x.Branch_ID==Session.Branch_ID&&x.Fiscal_Year_ID==Session.Year_ID);
    }
    public sealed class SaveFiscalPeriodRequest { public int Fiscal_Period_ID{get;set;} public string Period_Code{get;set;}=string.Empty;public string Period_Name{get;set;}=string.Empty;public DateTime Start_Date{get;set;}public DateTime End_Date{get;set;}public bool Is_Active{get;set;}=true;}
    public sealed class FiscalPeriodLifecycleRequest {public string? Reason{get;set;}}
}