using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers;
[ApiController][Route("api/payment-requests/{requestId:long}/attachments")]
public sealed class PaymentRequestAttachmentsController:ControllerBase
{
 private readonly AppDbContext _db;private readonly ScreenAuthorizationService _auth;private readonly AuditTrailService _audit;private readonly IWebHostEnvironment _env;
 public PaymentRequestAttachmentsController(AppDbContext db,ScreenAuthorizationService auth,AuditTrailService audit,IWebHostEnvironment env){_db=db;_auth=auth;_audit=audit;_env=env;}
 private ServerSession S()=>HttpContext.Items["ServerSession"] as ServerSession??throw new InvalidOperationException("جلسة الخادم غير متاحة.");
 private async Task<bool> Allowed(ScreenOperation op)=>await _auth.IsExplicitlyAllowedAsync(S(),"PaymentRequestAttachments",op);
 private Task<bool> Exists(long id)=>_db.Payment_Requests.AnyAsync(x=>x.Payment_Request_ID==id&&x.Company_ID==S().Company_ID&&x.Branch_ID==S().Branch_ID&&x.Fiscal_Year_ID==S().Year_ID);
 [HttpGet] public async Task<IActionResult> List(long requestId){if(!await Allowed(ScreenOperation.View))return Forbid();if(!await Exists(requestId))return NotFound();return Ok(await _db.Payment_Request_Attachments.Where(x=>x.Payment_Request_ID==requestId&&x.Company_ID==S().Company_ID&&x.Is_Active).AsNoTracking().ToListAsync());}
 [HttpPost] public async Task<IActionResult> Upload(long requestId,IFormFile file){if(!await Allowed(ScreenOperation.Add))return Forbid();if(!await Exists(requestId))return NotFound();if(file==null||file.Length==0||file.Length>20*1024*1024)return BadRequest(new{message="ملف المرفق غير صالح أو يتجاوز 20MB."});var s=S();var safe=Path.GetFileName(file.FileName);var key=Path.Combine("payment-request-attachments",s.Company_ID,s.Branch_ID.ToString(),s.Year_ID.ToString(),requestId.ToString(),Guid.NewGuid().ToString("N")+Path.GetExtension(safe));var full=Path.Combine(_env.ContentRootPath,key);Directory.CreateDirectory(Path.GetDirectoryName(full)!);await using(var stream=System.IO.File.Create(full))await file.CopyToAsync(stream);var row=new PaymentRequestAttachment{Payment_Request_ID=requestId,Company_ID=s.Company_ID,Branch_ID=s.Branch_ID,Fiscal_Year_ID=s.Year_ID,Original_File_Name=safe,Storage_Key=key,Content_Type=file.ContentType??"application/octet-stream",File_Size=file.Length,Created_By=s.User_ID.ToString()};_db.Payment_Request_Attachments.Add(row);_audit.Add(s,HttpContext,"payment_request_attachments","new","UPLOAD",null,new{requestId,safe,file.Length});await _db.SaveChangesAsync();return Ok(row);}
 [HttpDelete("{attachmentId:long}")] public async Task<IActionResult> Delete(long requestId,long attachmentId){if(!await Allowed(ScreenOperation.Delete))return Forbid();var row=await _db.Payment_Request_Attachments.SingleOrDefaultAsync(x=>x.Payment_Request_Attachment_ID==attachmentId&&x.Payment_Request_ID==requestId&&x.Company_ID==S().Company_ID&&x.Branch_ID==S().Branch_ID&&x.Fiscal_Year_ID==S().Year_ID&&x.Is_Active);if(row==null)return NotFound();row.Is_Active=false;_audit.Add(S(),HttpContext,"payment_request_attachments",attachmentId.ToString(),"DELETE",new{row.Original_File_Name},null);await _db.SaveChangesAsync();return NoContent();}
}