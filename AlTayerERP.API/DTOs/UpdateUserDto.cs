namespace AlTayerERP.API.DTOs
{
    public class UpdateUserDto
    {
        public int User_ID { get; set; }

        public string Company_ID { get; set; } = string.Empty;
        public int Branch_ID { get; set; }
        public int Role_ID { get; set; }

        public string User_Code { get; set; } = string.Empty;
        public string Full_Name { get; set; } = string.Empty;
        public string Login_Name { get; set; } = string.Empty;

        // إذا تركته فارغاً لا يتم تغيير كلمة المرور
        public string? Password { get; set; }

        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Notes { get; set; }

        public bool Must_Change_Password { get; set; }
        public bool Is_Active { get; set; }
    }
}