namespace AlTayerERP.API.DTOs;

public class CreateWorkCenterDto
{
    public string Company_ID { get; set; } = string.Empty;
    public int? Branch_ID { get; set; }
    public string Work_Center_Code { get; set; } = string.Empty;
    public string Work_Center_Name_AR { get; set; } = string.Empty;
    public string? Work_Center_Name_EN { get; set; }
    public string Work_Center_Type { get; set; } = string.Empty;
    public long? Parent_Work_Center_ID { get; set; }
    public bool Is_Active { get; set; } = true;
    public int Sort_Order { get; set; }
}