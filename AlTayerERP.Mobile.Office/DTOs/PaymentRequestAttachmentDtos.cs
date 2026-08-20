namespace AlTayerERP.Mobile.Office.DTOs;

public sealed class PaymentRequestAttachmentDto
{
    public long Payment_Request_Attachment_ID { get; set; }
    public long Payment_Request_ID { get; set; }
    public string Original_File_Name { get; set; } = string.Empty;
    public string Content_Type { get; set; } = "application/octet-stream";
    public long File_Size { get; set; }
    public DateTime Created_At { get; set; }
    public string Created_By { get; set; } = string.Empty;

    public string FileSizeDisplay => File_Size switch
    {
        < 1024 => $"{File_Size} بايت",
        < 1024 * 1024 => $"{File_Size / 1024d:N1} كيلوبايت",
        _ => $"{File_Size / 1024d / 1024d:N1} ميجابايت"
    };
}
