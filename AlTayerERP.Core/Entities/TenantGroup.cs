using System;

namespace AlTayerERP.Core.Entities
{
    /// <summary>
    /// كلاس الكيان الخاص بالمجموعات التجارية الأم (الهيكل الأعلى للنظام)
    /// </summary>
    public class TenantGroup
    {
        // [1] المفتاح الأساسي للمجموعة (UUID / Guid) مشفر ومحمي
        public string Group_ID { get; set; } = string.Empty;

        // [2] اسم المجموعة التجارية باللغة العربية (إجباري)
        public string Group_Name_AR { get; set; } = string.Empty;

        // [3] اسم المجموعة التجارية باللغة الإنجليزية (اختياري)
        public string Group_Name_EN { get; set; } = string.Empty;

        // [4] تاريخ ووقت إنشاء المجموعة في النظام
        public DateTime Created_At { get; set; }

        // [5] حالة المجموعة (نشطة = True / موقفة = False) لضبط الصلاحيات ديناميكياً
        public bool Is_Active { get; set; }
    }
}