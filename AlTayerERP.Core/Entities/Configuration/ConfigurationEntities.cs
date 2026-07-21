using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities.Configuration
{
    [Table("system_settings")]
    public class SystemSetting
    {
        [Key]
        [Column("Setting_ID")]
        public long Setting_ID { get; set; }

        [Required, MaxLength(150)]
        [Column("Setting_Code")]
        public string Setting_Code { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("Setting_Name")]
        public string Setting_Name { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("Module_Name")]
        public string? Module_Name { get; set; }

        [Required, MaxLength(30)]
        [Column("Data_Type")]
        public string Data_Type { get; set; } = "STRING";

        [Column("Default_Value")]
        public string? Default_Value { get; set; }

        [Column("Is_Sensitive")]
        public bool Is_Sensitive { get; set; }

        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;

        [Column("Created_At")]
        public DateTime Created_At { get; set; } = DateTime.UtcNow;
    }

    [Table("setting_scope_values")]
    public class SettingScopeValue
    {
        [Key]
        [Column("Setting_Scope_Value_ID")]
        public long Setting_Scope_Value_ID { get; set; }

        [Column("Setting_ID")]
        public long Setting_ID { get; set; }

        [Required, MaxLength(30)]
        [Column("Scope_Type")]
        public string Scope_Type { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        [Column("Scope_ID")]
        public string Scope_ID { get; set; } = string.Empty;

        [Column("Value")]
        public string? Value { get; set; }

        [Column("Effective_From")]
        public DateTime? Effective_From { get; set; }

        [Column("Effective_To")]
        public DateTime? Effective_To { get; set; }

        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;

        [Required, MaxLength(100)]
        [Column("Created_By")]
        public string Created_By { get; set; } = string.Empty;

        [Column("Created_At")]
        public DateTime Created_At { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        [Column("Change_Reason")]
        public string? Change_Reason { get; set; }
    }

    [Table("system_screen_fields")]
    public class SystemScreenField
    {
        [Key]
        [Column("Screen_Field_ID")]
        public long Screen_Field_ID { get; set; }

        [Column("Screen_ID")]
        public int Screen_ID { get; set; }

        [Required, MaxLength(150)]
        [Column("Field_Code")]
        public string Field_Code { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("Field_Name")]
        public string Field_Name { get; set; } = string.Empty;

        [Column("Is_Sensitive")]
        public bool Is_Sensitive { get; set; }

        [Column("Default_Required")]
        public bool Default_Required { get; set; }

        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;

        [Column("Sort_Order")]
        public int Sort_Order { get; set; }
    }

    [Table("system_screen_actions")]
    public class SystemScreenAction
    {
        [Key]
        [Column("Screen_Action_ID")]
        public long Screen_Action_ID { get; set; }

        [Column("Screen_ID")]
        public int Screen_ID { get; set; }

        [Required, MaxLength(100)]
        [Column("Action_Code")]
        public string Action_Code { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("Action_Name")]
        public string Action_Name { get; set; } = string.Empty;

        [Column("Is_Sensitive")]
        public bool Is_Sensitive { get; set; }

        [Column("Requires_Reason")]
        public bool Requires_Reason { get; set; }

        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;

        [Column("Sort_Order")]
        public int Sort_Order { get; set; }
    }

    [Table("role_resource_permissions")]
    public class RoleResourcePermission
    {
        [Key]
        [Column("Role_Resource_Permission_ID")]
        public long Role_Resource_Permission_ID { get; set; }

        [Column("Role_ID")]
        public int Role_ID { get; set; }

        [Required, MaxLength(30)]
        [Column("Resource_Type")]
        public string Resource_Type { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        [Column("Resource_Code")]
        public string Resource_Code { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        [Column("Permission_Code")]
        public string Permission_Code { get; set; } = string.Empty;

        [Column("Effect")]
        public bool Effect { get; set; }

        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;
    }

    [Table("user_resource_permissions")]
    public class UserResourcePermission
    {
        [Key]
        [Column("User_Resource_Permission_ID")]
        public long User_Resource_Permission_ID { get; set; }

        [Column("User_ID")]
        public int User_ID { get; set; }

        [Required, MaxLength(30)]
        [Column("Resource_Type")]
        public string Resource_Type { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        [Column("Resource_Code")]
        public string Resource_Code { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        [Column("Permission_Code")]
        public string Permission_Code { get; set; } = string.Empty;

        [Column("Effect")]
        public bool Effect { get; set; }

        [Column("Effective_To")]
        public DateTime? Effective_To { get; set; }

        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;
    }
}