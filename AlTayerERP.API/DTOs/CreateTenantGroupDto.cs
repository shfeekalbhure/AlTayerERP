namespace AlTayerERP.API.DTOs
{
    public class CreateTenantGroupDto
    {
        public string Group_Code { get; set; } = string.Empty;
        public string Group_Name_AR { get; set; } = string.Empty;
        public string Group_Name_EN { get; set; } = string.Empty;
        public string Short_Name { get; set; } = string.Empty;
        public string Group_Type { get; set; } = string.Empty;
        public string? Parent_Group_ID { get; set; }
        public string? Main_Company_ID { get; set; }
        public string? Default_Currency_Code { get; set; }
        public string? Country_Name { get; set; }
        public string? City_Name { get; set; }
        public string? Short_Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Manager_Name { get; set; }
        public bool Show_In_Login { get; set; }
        public int Sort_Order { get; set; }
        public string? Notes { get; set; }
        public bool Is_Active { get; set; } = true;
    }
}