using System;

namespace AlTayerERP.Core.Entities
{
    /// <summary>
    /// المجموعة التجارية (TenantGroup) وهي المستوى الأعلى قبل الشركات.
    /// حقول التدقيق مصدرها Backend ولا تقبل من واجهة المستخدم.
    /// </summary>
    public class TenantGroup
    {
        public string Group_ID { get; set; } = string.Empty;
        public string Group_Code { get; set; } = string.Empty;
        public string Group_Name_AR { get; set; } = string.Empty;
        /// <summary>اسم المجموعة بالإنجليزية اختياري وقد يكون NULL في البيانات القديمة.</summary>
        public string? Group_Name_EN { get; set; }
        public string Short_Name { get; set; } = string.Empty;
        public bool Is_Default { get; set; }
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
        public bool Show_In_Tree { get; set; }
        public int Sort_Order { get; set; }
        public string? Notes { get; set; }
        public DateTime Created_At { get; set; }
        public DateTime? Updated_At { get; set; }
        public int? Created_By { get; set; }
        public int? Updated_By { get; set; }
        public int Edit_Count { get; set; }
        public bool Is_Active { get; set; }
        public int? Stopped_By { get; set; }
        public DateTime? Stopped_At { get; set; }
        public string? Stopped_Reason { get; set; }
        public int? Reactivated_By { get; set; }
        public DateTime? Reactivated_At { get; set; }
        public string? Reactivate_Reason { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public int Companies_Count { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? Main_Company_Name { get; set; }
    }
}
