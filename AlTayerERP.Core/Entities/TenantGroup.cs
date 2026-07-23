using System;

namespace AlTayerERP.Core.Entities
{
    /// <summary>
    /// المجموعة التجارية (TenantGroup) وهي المستوى الأعلى قبل الشركات.
    /// حقول التدقيق مصدرها Backend ولا تقبل من واجهة المستخدم.
    /// </summary>
    public class TenantGroup
    {
        /// <summary>المعرف الداخلي للمجموعة Group_ID.</summary>
        public string Group_ID { get; set; } = string.Empty;
        /// <summary>الكود الفريد للمجموعة Group_Code.</summary>
        public string Group_Code { get; set; } = string.Empty;
        /// <summary>اسم المجموعة بالعربية Group_Name_AR.</summary>
        public string Group_Name_AR { get; set; } = string.Empty;
        /// <summary>اسم المجموعة بالإنجليزية Group_Name_EN.</summary>
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

        /// <summary>وقت الإنشاء Created_At من الخادم.</summary>
        public DateTime Created_At { get; set; }
        /// <summary>آخر وقت تعديل Updated_At من الخادم.</summary>
        public DateTime? Updated_At { get; set; }
        /// <summary>المستخدم المنشئ Created_By من الجلسة الموثوقة.</summary>
        public int? Created_By { get; set; }
        /// <summary>آخر مستخدم معدّل Updated_By من الجلسة الموثوقة.</summary>
        public int? Updated_By { get; set; }
        /// <summary>عداد التعديلات الناجحة Edit_Count ولا يعدل من العميل.</summary>
        public int Edit_Count { get; set; }
        /// <summary>حالة السجل Is_Active؛ يستخدم الإيقاف بدلاً من الحذف عند الارتباط.</summary>
        public bool Is_Active { get; set; }
        /// <summary>من أوقف السجل Stopped_By.</summary>
        public int? Stopped_By { get; set; }
        /// <summary>وقت الإيقاف Stopped_At.</summary>
        public DateTime? Stopped_At { get; set; }
        /// <summary>سبب الإيقاف Stopped_Reason.</summary>
        public string? Stopped_Reason { get; set; }
        /// <summary>من أعاد التفعيل Reactivated_By.</summary>
        public int? Reactivated_By { get; set; }
        /// <summary>وقت إعادة التفعيل Reactivated_At.</summary>
        public DateTime? Reactivated_At { get; set; }
        /// <summary>سبب إعادة التفعيل Reactivate_Reason.</summary>
        public string? Reactivate_Reason { get; set; }
    }
}