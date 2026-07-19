namespace AlTayerERP.API.DTOs
{
    // ======================================================
    // نموذج استقبال بيانات الشركة القادمة من شاشة الشركة
    // هذا الكلاس لا يحفظ في قاعدة البيانات مباشرة
    // بل يستقبل البيانات من الواجهة ثم يرسلها للـ Controller
    // ======================================================
    public class CreateCompanyDto
    {
        // رقم المجموعة التجارية التي تتبع لها الشركة
        public string Group_ID { get; set; } = string.Empty;

        // اسم الشركة باللغة العربية
        public string Company_Name_AR { get; set; } = string.Empty;

        // اسم الشركة باللغة الإنجليزية
        public string Company_Name_EN { get; set; } = string.Empty;

        // رمز الشركة المختصر
        public string Company_Prefix { get; set; } = string.Empty;

        // نوع النشاط
        public string Activity_Type { get; set; } = string.Empty;

        // الرقم الضريبي
        public string Tax_Number { get; set; } = string.Empty;

        // رقم الهاتف
        public string Phone { get; set; } = string.Empty;

        // رقم الجوال
        public string Mobile { get; set; } = string.Empty;

        // البريد الإلكتروني
        public string Email { get; set; } = string.Empty;

        // عنوان الشركة
        public string Address { get; set; } = string.Empty;

        // شعار الشركة
        public byte[]? Company_Logo { get; set; }

        // حالة الشركة
        public bool Is_Active { get; set; } = true;
    }
}