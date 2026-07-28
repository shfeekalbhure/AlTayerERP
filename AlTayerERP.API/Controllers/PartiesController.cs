using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>السجل الموحد للأطراف المالية؛ العميل والمورد والموظف والسائق/المندوب أنواع للطرف نفسه.</summary>
    [Authorize, ApiController, Route("api/[controller]")]
    public sealed class PartiesController : ControllerBase
    {
        private readonly AppDbContext _context; private readonly ScreenAuthorizationService _authorization; private readonly AuditTrailService _audit;
        public PartiesController(AppDbContext context, ScreenAuthorizationService authorization, AuditTrailService audit) { _context=context;_authorization=authorization;_audit=audit; }
        private ServerSession? Session=>HttpContext.Items["ServerSession"] as ServerSession;
        private async Task<IActionResult?> RequireAsync(ScreenOperation op)
        {
            if(Session==null)return Unauthorized(new {message="انتهت الجلسة أو أنها غير صالحة."});
            return await _authorization.IsAllowedAsync(Session,"Parties",op)?null:Forbid();
        }
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]string? type=null,[FromQuery]bool includeInactive=false)
        {
            var e=await RequireAsync(ScreenOperation.View);if(e!=null||Session==null)return e!;
            // يرجع السجل ضمن الشركة الحالية فقط، وتبقى الأطراف الموقوفة مخفية افتراضياً.
            var q=_context.Parties.AsNoTracking().Where(x=>x.Company_ID==Session.Company_ID);
            if(!includeInactive)q=q.Where(x=>x.Is_Active);
            if(!string.IsNullOrWhiteSpace(type))q=q.Where(x=>x.Party_Type==type.Trim().ToUpperInvariant());

            var parties = await q.OrderBy(x=>x.Party_Code).ToListAsync();

            // بيانات التدقيق تُستخرج من سجل الخادم، ولا تُقبل من جهاز المستخدم.
            var partyIds = parties.Select(x=>x.Party_ID).ToList();
            var auditLogs = partyIds.Count == 0
                ? new List<AuditLog>()
                : await _context.Audit_Logs.AsNoTracking()
                    .Where(x=>x.Table_Name=="parties" && partyIds.Contains(x.Record_ID))
                    .OrderBy(x=>x.Action_At)
                    .ToListAsync();

            // تحويل رقم المستخدم المخزن في التدقيق إلى اسمه الظاهر للمستخدم.
            var auditUserIds = auditLogs
                .Select(x=>x.User_ID)
                .Where(x=>int.TryParse(x, out _))
                .Select(x=>int.Parse(x!))
                .Distinct()
                .ToList();
            var userNames = auditUserIds.Count == 0
                ? new Dictionary<int, string>()
                : await _context.Users.AsNoTracking()
                    .Where(x=>x.Company_ID==Session.Company_ID && auditUserIds.Contains(x.User_ID))
                    .ToDictionaryAsync(x=>x.User_ID, x=>x.Full_Name);

            string UserName(string? userId) =>
                int.TryParse(userId, out var id) && userNames.TryGetValue(id, out var name)
                    ? name
                    : string.IsNullOrWhiteSpace(userId) ? "—" : userId;

            return Ok(parties.Select(party =>
            {
                var logs = auditLogs.Where(x=>x.Record_ID==party.Party_ID).ToList();
                var created = logs.FirstOrDefault(x=>x.Action_Type=="CREATE");
                var updated = logs.LastOrDefault(x=>x.Action_Type=="UPDATE" || x.Action_Type=="DEACTIVATE");

                return new
                {
                    party.Party_ID,
                    party.Party_Code,
                    party.Party_Name_AR,
                    party.Party_Name_EN,
                    party.Party_Type,
                    party.Mobile_No,
                    party.Phone_No,
                    party.Identity_No,
                    party.Tax_No,
                    party.Address,
                    party.Account_ID,
                    party.Credit_Limit,
                    party.Notes,
                    party.Is_Active,
                    party.Created_At,
                    party.Updated_At,
                    Created_By = UserName(created?.User_ID ?? party.Created_By),
                    Updated_By = UserName(updated?.User_ID ?? party.Updated_By),
                    Edit_Count = logs.Count(x=>x.Action_Type=="UPDATE"),
                    // لا توجد عملية طباعة للأطراف حالياً؛ يظهر العداد صفراً بوضوح.
                    Print_Count = 0
                };
            }));
        }
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SavePartyRequest? r)
        {
            if (r == null)
                return BadRequest(new { message = "بيانات الطرف المالي مطلوبة." });

            var op=string.IsNullOrWhiteSpace(r.Party_ID)?ScreenOperation.Add:ScreenOperation.Edit;
            var e=await RequireAsync(op);if(e!=null||Session==null)return e!;
            var type=r.Party_Type?.Trim().ToUpperInvariant();
            if(string.IsNullOrWhiteSpace(r.Party_Code)||string.IsNullOrWhiteSpace(r.Party_Name_AR)||!new[]{"CUSTOMER","VENDOR","EMPLOYEE","DRIVER","REPRESENTATIVE","AGENT","OTHER"}.Contains(type))
                return BadRequest(new {message="الكود والاسم العربي ونوع طرف صالح حقول مطلوبة."});
            var code=r.Party_Code.Trim().ToUpperInvariant();
            var party=string.IsNullOrWhiteSpace(r.Party_ID)?null:await _context.Parties.FirstOrDefaultAsync(x=>x.Party_ID==r.Party_ID&&x.Company_ID==Session.Company_ID);
            if(!string.IsNullOrWhiteSpace(r.Party_ID)&&party==null)return NotFound(new {message="الطرف غير موجود ضمن الشركة الحالية."});
            if(await _context.Parties.AnyAsync(x=>x.Company_ID==Session.Company_ID&&x.Party_Code==code&&x.Party_ID!=(party==null?"":party.Party_ID)))return Conflict(new {message="كود الطرف مستخدم مسبقاً."});
            var id=Text(r.Identity_No);
            if(id!=null&&await _context.Parties.AnyAsync(x=>x.Company_ID==Session.Company_ID&&x.Identity_No==id&&x.Party_ID!=(party==null?"":party.Party_ID)))return Conflict(new {message="رقم الهوية/السجل مستخدم مسبقاً."});
            if(!string.IsNullOrWhiteSpace(r.Account_ID)&&!await _context.Chart_Of_Accounts.AnyAsync(x=>x.Company_ID==Session.Company_ID&&x.Account_ID==r.Account_ID&&x.Is_Active&&x.Is_Postable))return BadRequest(new {message="الحساب الرقابي المختار غير فعّال أو غير قابل للترحيل."});
            var old=party==null?null:new {party.Party_Code,party.Party_Name_AR,party.Party_Type,party.Account_ID,party.Is_Active};
            var isNew = party == null;
            if(isNew)
            {
                // الإنشاء يسجل في حقول الإنشاء فقط؛ حقول التعديل لا تملأ قبل أول تعديل حقيقي.
                party=new Party
                {
                    Party_ID=Guid.NewGuid().ToString("N"),
                    Company_ID=Session.Company_ID,
                    Created_At=DateTime.UtcNow,
                    Created_By=Session.User_ID.ToString()
                };
                _context.Parties.Add(party);
            }
            party!.Party_Code=code;party.Party_Name_AR=r.Party_Name_AR.Trim();party.Party_Name_EN=Text(r.Party_Name_EN);party.Party_Type=type!;
            party.Mobile_No=Text(r.Mobile_No);party.Phone_No=Text(r.Phone_No);party.Identity_No=id;party.Tax_No=Text(r.Tax_No);party.Address=Text(r.Address);party.Account_ID=Text(r.Account_ID);party.Credit_Limit=Math.Max(0,r.Credit_Limit);party.Notes=Text(r.Notes);party.Is_Active=r.Is_Active;
            if (!isNew)
            {
                // التعديل فقط هو الذي يحدّث بيانات آخر تعديل.
                party.Updated_At=DateTime.UtcNow;
                party.Updated_By=Session.User_ID.ToString();
            }
            _audit.Add(Session,HttpContext,"parties",party.Party_ID,old==null?"CREATE":"UPDATE",old,new {party.Party_Code,party.Party_Name_AR,party.Party_Type,party.Account_ID,party.Is_Active});
            await _context.SaveChangesAsync();return Ok(new {message=old==null?"تمت إضافة الطرف المالي.":"تم تعديل الطرف المالي.",party.Party_ID});
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deactivate(string id,[FromQuery]string? reason)
        {
            var e=await RequireAsync(ScreenOperation.Delete);if(e!=null||Session==null)return e!;
            var p=await _context.Parties.FirstOrDefaultAsync(x=>x.Party_ID==id&&x.Company_ID==Session.Company_ID);if(p==null)return NotFound(new {message="الطرف غير موجود."});
            if(await _context.Financial_Voucher_Headers.AnyAsync(x=>x.Party_ID==id&&x.Is_Active&&!x.Is_Posted))return Conflict(new {message="لا يمكن إيقاف طرف لديه مستندات معلقة."});
            p.Is_Active=false;p.Updated_At=DateTime.UtcNow;p.Updated_By=Session.User_ID.ToString();_audit.Add(Session,HttpContext,"parties",id,"DEACTIVATE",new{Is_Active=true},new{Is_Active=false},reason);await _context.SaveChangesAsync();return Ok(new{message="تم إيقاف الطرف دون حذف تاريخه."});
        }
        private static string? Text(string? x)=>string.IsNullOrWhiteSpace(x)?null:x.Trim();
    }
    public sealed class SavePartyRequest {public string? Party_ID{get;set;}public string Party_Code{get;set;}=string.Empty;public string Party_Name_AR{get;set;}=string.Empty;public string? Party_Name_EN{get;set;}public string Party_Type{get;set;}=string.Empty;public string? Mobile_No{get;set;}public string? Phone_No{get;set;}public string? Identity_No{get;set;}public string? Tax_No{get;set;}public string? Address{get;set;}public string? Account_ID{get;set;}public decimal Credit_Limit{get;set;}public string? Notes{get;set;}public bool Is_Active{get;set;}=true;}
}