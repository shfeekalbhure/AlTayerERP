using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers;

[ApiController]
[Route("api/financial-governance")]
public sealed class FinancialGovernanceController : ControllerBase
{
    private readonly AppDbContext _db; private readonly ScreenAuthorizationService _auth; private readonly AuditTrailService _audit;
    public FinancialGovernanceController(AppDbContext db, ScreenAuthorizationService auth, AuditTrailService audit){_db=db;_auth=auth;_audit=audit;}
    private ServerSession Session()=>HttpContext.Items["ServerSession"] as ServerSession??throw new InvalidOperationException("جلسة الخادم غير متاحة.");
    private async Task<IActionResult?> Allow(string screen, ScreenOperation op){return await _auth.IsExplicitlyAllowedAsync(Session(),screen,op)?null:Forbid();}

    [HttpGet("approvals")]
    public async Task<IActionResult> Approvals([FromQuery]string? status=null)
    {
        var denial=await Allow("ApprovalRequests",ScreenOperation.View);if(denial!=null)return denial;var s=Session();
        var query=_db.Approval_Requests.AsNoTracking().Where(x=>x.Company_ID==s.Company_ID);
        if(!string.IsNullOrWhiteSpace(status))query=query.Where(x=>x.Status==status.Trim());
        return Ok(await query.OrderByDescending(x=>x.Requested_At).Take(500).ToListAsync());
    }
    [HttpPost("approvals/{id:int}/review")]
    public async Task<IActionResult> Review(int id,[FromBody]ReasonRequest request)=>await ChangeApproval(id,ApprovalStatus.UnderReview,ScreenOperation.Approve,request,"REVIEW");
    [HttpPost("approvals/{id:int}/approve")]
    public async Task<IActionResult> Approve(int id,[FromBody]ReasonRequest request)=>await ChangeApproval(id,ApprovalStatus.Approved,ScreenOperation.Approve,request,"APPROVE");
    [HttpPost("approvals/{id:int}/reject")]
    public async Task<IActionResult> Reject(int id,[FromBody]ReasonRequest request)=>await ChangeApproval(id,ApprovalStatus.Rejected,ScreenOperation.Unapprove,request,"REJECT");
    [HttpPost("approvals/{id:int}/return")]
    public async Task<IActionResult> Return(int id,[FromBody]ReasonRequest request)=>await ChangeApproval(id,ApprovalStatus.Returned,ScreenOperation.Unapprove,request,"RETURN");

    private async Task<IActionResult> ChangeApproval(int id,ApprovalStatus target,ScreenOperation operation,ReasonRequest request,string action)
    {
        var denial=await Allow("ApprovalRequests",operation);if(denial!=null)return denial;
        if(request==null||string.IsNullOrWhiteSpace(request.Reason))return BadRequest(new{message="السبب إلزامي لهذه العملية."});
        var row=await _db.Approval_Requests.FirstOrDefaultAsync(x=>x.Approval_ID==id&&x.Company_ID==Session().Company_ID);if(row==null)return NotFound();
        if(row.Status is nameof(ApprovalStatus.Approved) or nameof(ApprovalStatus.Rejected) or nameof(ApprovalStatus.Canceled))return Conflict(new{message="الطلب مغلق ولا يمكن تغيير حالته."});
        var before=new{row.Status,row.Approved_By,row.Approved_At,row.Approval_Notes};row.Status=target.ToString();row.Approval_Notes=request.Reason.Trim();
        if(target==ApprovalStatus.Approved){row.Approved_By=Session().User_ID.ToString();row.Approved_At=DateTime.UtcNow;}
        _audit.Add(Session(),HttpContext,"approval_requests",id.ToString(),action,before,new{row.Status,row.Approved_By,row.Approved_At,row.Approval_Notes},request.Reason);
        await _db.SaveChangesAsync();return Ok(row);
    }

    [HttpGet("limits")]
    public async Task<IActionResult> Limits()
    {
        var denial=await Allow("FinancialLimits",ScreenOperation.View);if(denial!=null)return denial;var s=Session();
        return Ok(await _db.Financial_Policies.AsNoTracking().Where(x=>x.Company_ID==s.Company_ID).OrderBy(x=>x.Entity_Type).ThenBy(x=>x.Entity_ID).ToListAsync());
    }
    [HttpPost("limits")]
    public async Task<IActionResult> SaveLimit([FromBody]FinancialLimitRequest request)
    {
        var denial=await Allow("FinancialLimits",request.Limit_ID==0?ScreenOperation.Add:ScreenOperation.Edit);if(denial!=null)return denial;
        if(request==null||string.IsNullOrWhiteSpace(request.Entity_Type)||string.IsNullOrWhiteSpace(request.Entity_ID)||string.IsNullOrWhiteSpace(request.Limit_Type)||string.IsNullOrWhiteSpace(request.Currency_Code)||request.Limit_Amount<0)return BadRequest(new{message="بيانات السقف المالي غير مكتملة."});
        var s=Session();var row=request.Limit_ID==0?null:await _db.Financial_Policies.FirstOrDefaultAsync(x=>x.Limit_ID==request.Limit_ID&&x.Company_ID==s.Company_ID);
        var before=row==null?null:new{row.Limit_Amount,row.Used_Amount,row.Is_Active,row.Requires_Approval};
        if(row==null){row=new FinancialPolicy{Company_ID=s.Company_ID,Created_At=DateTime.UtcNow};_db.Financial_Policies.Add(row);}
        row.Entity_Type=request.Entity_Type.Trim();row.Entity_ID=request.Entity_ID.Trim();row.Limit_Type=request.Limit_Type.Trim().ToUpperInvariant();row.Currency_Code=request.Currency_Code.Trim().ToUpperInvariant();row.Limit_Amount=request.Limit_Amount;row.Period_Type=request.Period_Type?.Trim()??"Monthly";row.Requires_Approval=request.Requires_Approval;row.Is_Active=request.Is_Active;row.Updated_At=DateTime.UtcNow;
        _audit.Add(s,HttpContext,"financial_limits",row.Limit_ID==0?"new":row.Limit_ID.ToString(),before==null?"CREATE":"UPDATE",before,new{row.Entity_Type,row.Entity_ID,row.Limit_Type,row.Currency_Code,row.Limit_Amount,row.Requires_Approval,row.Is_Active});
        await _db.SaveChangesAsync();return Ok(row);
    }
    [HttpGet("limits/{id:int}/movements")]
    public async Task<IActionResult> Movements(int id)
    {
        var denial=await Allow("FinancialLimits",ScreenOperation.View);if(denial!=null)return denial;var s=Session();var exists=await _db.Financial_Policies.AnyAsync(x=>x.Limit_ID==id&&x.Company_ID==s.Company_ID);if(!exists)return NotFound();
        return Ok(await _db.Financial_Policy_Movements.AsNoTracking().Where(x=>x.Limit_ID==id&&x.Company_ID==s.Company_ID).OrderByDescending(x=>x.Movement_Date).ToListAsync());
    }
    public sealed class ReasonRequest{public string Reason{get;set;}=string.Empty;}
    public sealed class FinancialLimitRequest{public int Limit_ID{get;set;}public string Entity_Type{get;set;}=string.Empty;public string Entity_ID{get;set;}=string.Empty;public string Limit_Type{get;set;}=string.Empty;public string Currency_Code{get;set;}=string.Empty;public decimal Limit_Amount{get;set;}public string? Period_Type{get;set;}public bool Requires_Approval{get;set;}=true;public bool Is_Active{get;set;}=true;}
}