namespace AlTayerERP.API.DTOs
{
    public class CreateUserDto
    {
        public string Company_ID { get; set; } = string.Empty;
        public int Branch_ID { get; set; }
        public int Role_ID { get; set; }

        public string User_Code { get; set; } = string.Empty;
        public string Full_Name { get; set; } = string.Empty;
        public string Login_Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Notes { get; set; }

        public bool Must_Change_Password { get; set; } = true;
        public bool Is_Active { get; set; } = true;
    }
}