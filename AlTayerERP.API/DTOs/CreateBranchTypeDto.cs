namespace AlTayerERP.API.DTOs;

public class CreateBranchTypeDto
{
    public string Branch_Type_Code { get; set; } = string.Empty;
    public string Branch_Type_Name_AR { get; set; } = string.Empty;
    public string? Branch_Type_Name_EN { get; set; }
    public bool Allows_Financial_Operations { get; set; } = true;
    public bool Is_Active { get; set; } = true;
    public int Sort_Order { get; set; }
}