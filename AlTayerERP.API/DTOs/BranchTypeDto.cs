namespace AlTayerERP.API.DTOs
{
    public class BranchTypeDto
    {
        public string Branch_Type_Code { get; set; } = string.Empty;
        public string Branch_Type_Name_AR { get; set; } = string.Empty;
        public string? Branch_Type_Name_EN { get; set; }
        public int Sort_Order { get; set; }
        public bool Is_Active { get; set; } = true;
        public string? Notes { get; set; }
    }
}
