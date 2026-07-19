namespace AlTayerERP.API.DTOs
{
    public class CreateRoleDto
    {
        public string Role_Code { get; set; } = string.Empty;

        public string Role_Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool Is_Active { get; set; } = true;
    }
}