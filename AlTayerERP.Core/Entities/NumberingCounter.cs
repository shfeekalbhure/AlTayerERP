using System;

namespace AlTayerERP.Core.Entities
{
    // ======================================================
    // هذا الكلاس يمثل جدول عدادات الترقيم الفعلية
    // كل سجل هنا يحفظ آخر رقم مستخدم حسب:
    // نوع المستند + الشركة + الفرع + السنة
    // ======================================================
    public class NumberingCounter
    {

        
        
        // رقم العداد الأساسي
        public int Counter_ID { get; set; }

        // نوع المستند
        // مثال: BRANCH / SHIPMENT / TICKET
        public string Document_Type { get; set; } = string.Empty;

        // رقم الشركة
        // يستخدم إذا كان الترقيم حسب الشركة
        public string? Company_ID { get; set; }

        // رقم الفرع
        // يستخدم إذا كان الترقيم حسب الفرع
        public int? Branch_ID { get; set; }

        // السنة
        // تستخدم إذا كان الترقيم حسب السنة
        public int? Year_Value { get; set; }

        // آخر رقم مستخدم لهذا العداد
        public int Last_Number { get; set; }

        // تاريخ إنشاء العداد
        public DateTime Created_At { get; set; }

        // تاريخ آخر تعديل
        public DateTime? Updated_At { get; set; }
    }
}