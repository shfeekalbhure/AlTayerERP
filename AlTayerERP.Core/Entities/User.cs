using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("User_ID")]
        public int User_ID { get; set; }

        [Column("Company_ID")]
        public string Company_ID { get; set; } = string.Empty;

        [Column("Branch_ID")]
        public int Branch_ID { get; set; }

        [Column("Role_ID")]
        public int Role_ID { get; set; }

        [Column("User_Code")]
        public string User_Code { get; set; } = string.Empty;

        [Column("Full_Name")]
        public string Full_Name { get; set; } = string.Empty;

        [Column("Login_Name")]
        public string Login_Name { get; set; } = string.Empty;

        [Column("Password_Hash")]
        public string Password_Hash { get; set; } = string.Empty;

        [Column("Phone")]
        public string? Phone { get; set; }

        [Column("Email")]
        public string? Email { get; set; }

        [Column("Notes")]
        public string? Notes { get; set; }

        [Column("Must_Change_Password")]
        public bool Must_Change_Password { get; set; } = true;

        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;

        [Column("Created_At")]
        public DateTime Created_At { get; set; } = DateTime.Now;

        [Column("Updated_At")]
        public DateTime? Updated_At { get; set; }

        // حقول قفل الدخول يديرها الـ API فقط، ولا يقبلها من تطبيق سطح المكتب.
        [Column("Failed_Login_Count")]
        public int Failed_Login_Count { get; set; }

        [Column("Last_Failed_Login_At")]
        public DateTime? Last_Failed_Login_At { get; set; }

        [Column("Locked_Until")]
        public DateTime? Locked_Until { get; set; }

        [Column("Last_Login_At")]
        public DateTime? Last_Login_At { get; set; }

        [Column("Last_Login_IP")]
        public string? Last_Login_IP { get; set; }
    }
}