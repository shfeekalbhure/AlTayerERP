namespace AlTayerERP.API.DTOs
{
    public class CreateTenantGroupDto
    {
        public string Group_Name_AR { get; set; } = string.Empty;
        public string Group_Name_EN { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
