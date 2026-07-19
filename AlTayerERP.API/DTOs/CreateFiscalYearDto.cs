namespace AlTayerERP.API.DTOs
{
    // ======================================================
    // استقبال بيانات السنة المالية من شاشة السنوات المالية
    // ======================================================
    public class CreateFiscalYearDto
    {
        // الشركة التابعة
        public string Company_ID { get; set; } = string.Empty;

        // اسم السنة المالية
        public string Year_Name { get; set; } = string.Empty;

        // تاريخ البداية
        public DateTime Start_Date { get; set; }

        // تاريخ النهاية
        public DateTime End_Date { get; set; }

        // السنة الافتراضية
        public bool Is_Default { get; set; }

        // السنة مقفلة
        public bool Is_Closed { get; set; }

        // الحالة
        public bool Is_Active { get; set; } = true;
    }
}