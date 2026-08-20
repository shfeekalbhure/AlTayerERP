using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AlTayerERP.Core.Entities.Accounting;
[Table("payment_request_attachments")]
public sealed class PaymentRequestAttachment
{
 [Key] public long Payment_Request_Attachment_ID{get;set;}
 public long Payment_Request_ID{get;set;}
 [Required] public string Company_ID{get;set;}=string.Empty;
 public int Branch_ID{get;set;} public int Fiscal_Year_ID{get;set;}
 [Required] public string Original_File_Name{get;set;}=string.Empty;
 [Required] public string Storage_Key{get;set;}=string.Empty;
 public string Content_Type{get;set;}="application/octet-stream"; public long File_Size{get;set;}
 public bool Is_Active{get;set;}=true; public string Created_By{get;set;}=string.Empty; public DateTime Created_At{get;set;}=DateTime.UtcNow;
}