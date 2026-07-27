namespace AlTayerERP.API.DTOs
{
    /// <summary>حقول المجموعة التجارية المعتمدة للمرحلة الحالية.</summary>
    public sealed class CreateTenantGroupDto
    {
        public string Group_Code { get; set; } = string.Empty;
        public string Group_Name_AR { get; set; } = string.Empty;
        public string? Group_Name_EN { get; set; }
        public bool Is_Default { get; set; }
        public bool Show_In_Login { get; set; }
        public bool Show_In_Tree { get; set; }
        public string? Notes { get; set; }
    }
}
