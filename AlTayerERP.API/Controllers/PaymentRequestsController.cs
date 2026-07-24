using AlTayerERP.API.Services;
using AlTayerERP.API.Services.Accounting;
using AlTayerERP.Core.Entities;
using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers;

[ApiController]
[Route("api/payment-requests")]
public sealed class PaymentRequestsController : ControllerBase
{
    private readonly AppDbContext _db; private readonly ScreenAuthorizationService _auth; private readonly AuditTrailService _audit; private readonly FinancialVoucherService _vouchers;
    public PaymentRequestsController(AppDbContext db,ScreenAuthorizationService auth,AuditTrailService audit,FinancialVoucherService vouchers){_db=db;_auth=auth;_audit=audit;_vouchers=vouchers;}
    private ServerSession Session()=>HttpContext.Items["ServerSession"] as ServerSession??throw new InvalidOperationException("جلسة الخادم غير متاحة.");
    private async Task<IActionResult?> Allow(ScreenOperation op)=>await _auth.IsExplicitlyAllowedAsync(Session(),"PaymentRequest",op)?null:Forbid();
    private IQueryable<PaymentRequest> Scoped()=>_db.Payment_Requests.Include(x=>x.Details).Where(x=>x.Company_ID==Session().Company_ID&&x.Branch_ID==Session().Branch_ID&&x.Fiscal_Year_ID==Session().Year_ID);

    [HttpGet] public async Task<IActionResult> List([FromQuery]string? status){var d=await Allow(ScreenOperation.View);if(d!=null)return d;var q=Scoped().AsNoTracking();if(!string.IsNullOrWhiteSpace(status))q=q.Where(x=>x.Status==status.Trim().ToUpperInvariant());return Ok(await q.OrderByDescending(x=>x.Created_At).Take(500).ToListAsync());}
    [HttpGet("{id:long}")] public async Task<IActionResult> Get(long id){var d=await Allow(ScreenOperation.View);if(d!=null)return d;var r=await Scoped().AsNoTracking().SingleOrDefaultAsync(x=>x.Payment_Request_ID==id);return r==null?NotFound():Ok(r);}

    [HttpPost] public async Task<IActionResult> Create([FromBody]PaymentRequestDto dto)
    {
        var denial=await Allow(ScreenOperation.Add);if(denial!=null)return denial;var s=Session();var validation=await Validate(dto);if(validation!=null)return BadRequest(new{message=validation});
        var row=new PaymentRequest{Company_ID=s.Company_ID,Branch_ID=s.Branch_ID,Fiscal_Year_ID=s.Year_ID,Request_No=$"PR-{DateTime.UtcNow:yyyyMMddHHmmssfff}",Request_Date=dto.Request_Date.Date,Status="DRAFT",Beneficiary_Name=dto.Beneficiary_Name.Trim(),Party_ID=Text(dto.Party_ID),Payment_Method_ID=dto.Payment_Method_ID,Header_Reference_No=Text(dto.Header_Reference_No),Description=Text(dto.Description),Created_By=s.User_ID.ToString(),Created_At=DateTime.UtcNow,Details=dto.Lines.Select((x,i)=>Line(x,i+1)).ToList()};
        _db.Payment_Requests.Add(row);_audit.Add(s,HttpContext,"payment_requests","new","CREATE",null,new{row.Request_No,row.Status,row.Beneficiary_Name,Lines=row.Details.Count});await _db.SaveChangesAsync();return Ok(row);
    }
    [HttpPut("{id:long}")] public async Task<IActionResult> Update(long id,[FromBody]PaymentRequestDto dto)
    {
        var denial=await Allow(ScreenOperation.Edit);if(denial!=null)return denial;var row=await Scoped().SingleOrDefaultAsync(x=>x.Payment_Request_ID==id);if(row==null)return NotFound();if(row.Status is not ("DRAFT" or "RETURNED"))return Conflict(new{message="لا يعدل إلا طلب مسودة أو معاد."});var validation=await Validate(dto);if(validation!=null)return BadRequest(new{message=validation});
        var before=new{row.Beneficiary_Name,row.Status,row.Approved_Local_Total};row.Beneficiary_Name=dto.Beneficiary_Name.Trim();row.Party_ID=Text(dto.Party_ID);row.Payment_Method_ID=dto.Payment_Method_ID;row.Header_Reference_No=Text(dto.Header_Reference_No);row.Description=Text(dto.Description);row.Request_Date=dto.Request_Date.Date;_db.Payment_Request_Lines.RemoveRange(row.Details);row.Details=dto.Lines.Select((x,i)=>Line(x,i+1)).ToList();row.Status="DRAFT";row.Updated_By=Session().User_ID.ToString();row.Updated_At=DateTime.UtcNow;_audit.Add(Session(),HttpContext,"payment_requests",id.ToString(),"UPDATE",before,new{row.Status,row.Beneficiary_Name,Lines=row.Details.Count});await _db.SaveChangesAsync();return Ok(row);
    }
    [HttpPost("{id:long}/submit")] public Task<IActionResult> Submit(long id)=>Transition(id,"DRAFT","PENDING_REVIEW",ScreenOperation.Edit,null,"SUBMIT");
    [HttpPost("{id:long}/review")] public Task<IActionResult> Review(long id,[FromBody]ReasonDto dto)=>Transition(id,"PENDING_REVIEW","PENDING_APPROVAL",ScreenOperation.Approve,dto.Reason,"REVIEW");
    [HttpPost("{id:long}/approve")] public async Task<IActionResult> Approve(long id,[FromBody]ReasonDto dto)
    {
        var d=await Allow(ScreenOperation.Approve);if(d!=null)return d;if(string.IsNullOrWhiteSpace(dto?.Reason))return BadRequest(new{message="سبب الاعتماد إلزامي."});var row=await Scoped().SingleOrDefaultAsync(x=>x.Payment_Request_ID==id);if(row==null)return NotFound();if(row.Status!="PENDING_APPROVAL")return Conflict(new{message="الحالة الحالية لا تسمح بالاعتماد."});
        var amount=row.Details.Sum(x=>x.Local_Amount);var limit=await _db.Financial_Policies.Where(x=>x.Company_ID==Session().Company_ID&&x.Is_Active&&x.Limit_Type=="PAYMENT"&&(x.Entity_ID==row.Party_ID||x.Entity_ID==Session().Branch_ID.ToString())).OrderBy(x=>x.Limit_Amount).FirstOrDefaultAsync();if(limit!=null&&amount>limit.Limit_Amount-limit.Used_Amount)return Conflict(new{message="المبلغ يتجاوز السقف المالي المتاح."});
        row.Status="APPROVED";row.Approved_Local_Total=amount;row.Approval_Reason=dto.Reason.Trim();row.Updated_By=Session().User_ID.ToString();row.Updated_At=DateTime.UtcNow;_audit.Add(Session(),HttpContext,"payment_requests",id.ToString(),"APPROVE",null,new{row.Status,row.Approved_Local_Total},dto.Reason);await _db.SaveChangesAsync();return Ok(row);
    }
    [HttpPost("{id:long}/reject")] public Task<IActionResult> Reject(long id,[FromBody]ReasonDto dto)=>Close(id,"REJECTED",dto,"REJECT");
    [HttpPost("{id:long}/return")] public Task<IActionResult> Return(long id,[FromBody]ReasonDto dto)=>Close(id,"RETURNED",dto,"RETURN");
    [HttpPost("{id:long}/create-payment-voucher")] public async Task<IActionResult> CreatePaymentVoucher(long id,[FromBody]CreateVoucherDto dto)
    {
        var denial=await Allow(ScreenOperation.Add);if(denial!=null)return denial;var row=await Scoped().SingleOrDefaultAsync(x=>x.Payment_Request_ID==id);if(row==null)return NotFound();if(row.Status!="APPROVED"||row.Payment_Voucher_ID.HasValue)return Conflict(new{message="لا ينشأ سند الصرف إلا مرة واحدة من طلب معتمد."});if(string.IsNullOrWhiteSpace(dto.Cash_Account_ID))return BadRequest(new{message="الصندوق/البنك الدائن مطلوب."});
        var limit=await _db.Financial_Policies.Where(x=>x.Company_ID==Session().Company_ID&&x.Is_Active&&x.Limit_Type=="PAYMENT"&&(x.Entity_ID==row.Party_ID||x.Entity_ID==Session().Branch_ID.ToString())).OrderBy(x=>x.Limit_Amount).FirstOrDefaultAsync();if(limit!=null&&row.Details.Sum(x=>x.Local_Amount)>limit.Limit_Amount-limit.Used_Amount)return Conflict(new{message="السقف المالي لم يعد متاحاً لإنشاء سند الصرف."});
        var type=await _db.Voucher_Types.Where(x=>x.Is_Active&&x.Voucher_Type_Code=="PAYMENT").Select(x=>x.Voucher_Type_ID).SingleAsync();var draft=await _db.Voucher_Statuses.Where(x=>x.Is_Active&&x.Voucher_Status_Code=="DRAFT").Select(x=>x.Voucher_Status_ID).SingleAsync();var local=row.Details.Sum(x=>x.Local_Amount);if(local>row.Approved_Local_Total)return Conflict(new{message="تجاوز المبلغ المعتمد ممنوع."});
        var headerCurrency=row.Details.First().Currency_ID;var now=DateTime.UtcNow;var request=new AlTayerERP.API.DTOs.Accounting.CreateFinancialVoucherDto{Voucher_Type_ID=type,Voucher_Status_ID=draft,Branch_ID=Session().Branch_ID.ToString(),Fiscal_Year_ID=Session().Year_ID,Voucher_Date=now,Transaction_Date=now,Cash_Account_ID=dto.Cash_Account_ID,Party_ID=row.Party_ID,Received_From_Name=row.Beneficiary_Name,Payment_Method_ID=row.Payment_Method_ID,Currency_ID=headerCurrency,Exchange_Rate=1m,Amount=local,Foreign_Total=0m,Local_Total=local,Reference_No=row.Request_No,Against_Text=row.Description,Description=row.Description,Source_Document_No=row.Request_No,Requires_Approval=true,Created_By=Session().User_ID.ToString(),Updated_By=Session().User_ID.ToString(),Details=row.Details.Select((x,i)=>new AlTayerERP.API.DTOs.Accounting.CreateFinancialVoucherDetailDto{Line_No=i+2,Account_ID=x.Account_ID,Cost_Center_ID=x.Cost_Center_ID,Currency_ID=x.Currency_ID,Exchange_Rate=x.Exchange_Rate,Foreign_Amount=x.Foreign_Amount,Local_Amount=x.Local_Amount,Debit_Amount=x.Local_Amount,Credit_Amount=0m,Reference_No=x.Reference_No,Description=x.Description,Line_Type=2}).Prepend(new AlTayerERP.API.DTOs.Accounting.CreateFinancialVoucherDetailDto{Line_No=1,Account_ID=dto.Cash_Account_ID,Currency_ID=headerCurrency,Exchange_Rate=1m,Foreign_Amount=0m,Local_Amount=local,Debit_Amount=0m,Credit_Amount=local,Description=row.Description,Line_Type=1}).ToList()};
        var result=await _vouchers.CreateAsync(request);if(!result.Success)return BadRequest(new{message=result.Message});row.Payment_Voucher_ID=result.VoucherId;if(limit!=null){limit.Used_Amount+=local;limit.Updated_At=DateTime.UtcNow;}_audit.Add(Session(),HttpContext,"payment_requests",id.ToString(),"CREATE_PAYMENT_VOUCHER",null,new{result.VoucherId,result.VoucherNo});await _db.SaveChangesAsync();return Ok(new{result.VoucherId,result.VoucherNo});
    }
    private async Task<IActionResult> Transition(long id,string from,string to,ScreenOperation op,string? reason,string action){var d=await Allow(op);if(d!=null)return d;var row=await Scoped().SingleOrDefaultAsync(x=>x.Payment_Request_ID==id);if(row==null)return NotFound();if(row.Status!=from)return Conflict(new{message="الحالة الحالية لا تسمح بهذه العملية."});row.Status=to;row.Review_Reason=Text(reason);row.Updated_By=Session().User_ID.ToString();row.Updated_At=DateTime.UtcNow;_audit.Add(Session(),HttpContext,"payment_requests",id.ToString(),action,null,new{row.Status},reason);await _db.SaveChangesAsync();return Ok(row);}
    private Task<IActionResult> Close(long id,string to,ReasonDto dto,string action)=>string.IsNullOrWhiteSpace(dto?.Reason)?Task.FromResult<IActionResult>(BadRequest(new{message="السبب إلزامي."})):Transition(id,"PENDING_APPROVAL",to,ScreenOperation.Unapprove,dto.Reason,action);
    private async Task<string?> Validate(PaymentRequestDto dto)
    {
        if(dto==null||string.IsNullOrWhiteSpace(dto.Beneficiary_Name)||dto.Lines.Count==0)return "المستفيد والتفاصيل مطلوبان.";
        var s=Session();
        var periodOpen=await _db.Fiscal_Periods.AsNoTracking().AnyAsync(p=>p.Branch_ID==s.Branch_ID&&p.Fiscal_Year_ID==s.Year_ID&&p.Is_Active&&!p.Is_Closed&&p.Start_Date.Date<=dto.Request_Date.Date&&p.End_Date.Date>=dto.Request_Date.Date);
        if(!periodOpen)return "لا توجد فترة مالية مفتوحة لتاريخ طلب الصرف.";
        foreach(var x in dto.Lines)
        {
            if(string.IsNullOrWhiteSpace(x.Account_ID)||x.Currency_ID<=0||x.Exchange_Rate<=0||x.Local_Amount<=0)return "الحساب والعملة وسعر الصرف والمبلغ المحلي مطلوبة لكل سطر.";
            var currency=await _db.Currencies.AsNoTracking().SingleOrDefaultAsync(c=>c.Currency_ID==x.Currency_ID&&c.Company_ID==s.Company_ID&&c.Is_Active);
            if(currency==null)return "توجد عملة موقوفة أو خارج نطاق الشركة.";
            if(currency.Is_Local_Currency&&(x.Foreign_Amount!=0m||x.Exchange_Rate!=1m))return "في العملة المحلية يجب أن يكون الأجنبي صفراً وسعر الصرف 1.";
            if(!currency.Is_Local_Currency&&decimal.Round(x.Foreign_Amount*x.Exchange_Rate,4)!=decimal.Round(x.Local_Amount,4))return "المبلغ المحلي للسطر الأجنبي يجب أن يساوي الأجنبي × سعر الصرف.";
            var account=await _db.Chart_Of_Accounts.AsNoTracking().AnyAsync(a=>a.Account_ID==x.Account_ID.Trim()&&a.Company_ID==s.Company_ID&&a.Is_Active&&a.Is_Postable);
            if(!account)return "يوجد حساب غير نشط أو غير قابل للترحيل.";
            if(!string.IsNullOrWhiteSpace(x.Cost_Center_ID)&&!await _db.Cost_Centers.AsNoTracking().AnyAsync(cc=>cc.Cost_Center_ID==x.Cost_Center_ID.Trim()&&cc.Company_ID==s.Company_ID&&cc.Is_Active&&cc.Is_Postable))return "يوجد مركز تكلفة غير نشط أو غير قابل للترحيل.";
        }
        return null;
    }
    private static PaymentRequestLine Line(PaymentRequestLineDto x,int n)=>new(){Line_No=n,Account_ID=x.Account_ID.Trim(),Cost_Center_ID=Text(x.Cost_Center_ID),Currency_ID=x.Currency_ID,Exchange_Rate=x.Exchange_Rate,Foreign_Amount=x.Foreign_Amount,Local_Amount=x.Local_Amount,Reference_No=Text(x.Reference_No),Description=Text(x.Description)};
    private static string? Text(string? x)=>string.IsNullOrWhiteSpace(x)?null:x.Trim();
    public sealed class PaymentRequestDto{public DateTime Request_Date{get;set;}=DateTime.UtcNow;public string Beneficiary_Name{get;set;}=string.Empty;public string? Party_ID{get;set;}public int? Payment_Method_ID{get;set;}public string? Header_Reference_No{get;set;}public string? Description{get;set;}public List<PaymentRequestLineDto> Lines{get;set;}=new();}
    public sealed class PaymentRequestLineDto{public string Account_ID{get;set;}=string.Empty;public string? Cost_Center_ID{get;set;}public int Currency_ID{get;set;}public decimal Exchange_Rate{get;set;}=1m;public decimal Foreign_Amount{get;set;}public decimal Local_Amount{get;set;}public string? Reference_No{get;set;}public string? Description{get;set;}}
    public sealed class ReasonDto{public string Reason{get;set;}=string.Empty;} public sealed class CreateVoucherDto{public string Cash_Account_ID{get;set;}=string.Empty;}
}