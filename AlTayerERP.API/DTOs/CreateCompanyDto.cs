namespace AlTayerERP.API.DTOs
{
    /// <summary>
    /// نموذج بيانات الشركة المقبول من الواجهة.
    /// حقول الحالة والتدقيق لا تُقبل من العميل؛ الإيقاف وإعادة التفعيل لهما مساران مستقلان.
    /// </summary>
    public sealed class CreateCompanyDto
    {
        /// <summary>معرّف المجموعة التجارية النشطة التابعة لها الشركة.</summary>
        public string Group_ID { get; set; } = string.Empty;

        /// <summary>اسم الشركة باللغة العربية.</summary>
        public string Company_Name_AR { get; set; } = string.Empty;

        /// <summary>اسم الشركة باللغة الإنجليزية.</summary>
        public string Company_Name_EN { get; set; } = string.Empty;

        /// <summary>بادئة مختصرة تستخدم في ترقيم مستندات الشركة، وليست كود الشركة الداخلي.</summary>
        public string Company_Prefix { get; set; } = string.Empty;

        /// <summary>نوع النشاط.</summary>
        public string? Activity_Type { get; set; }

        /// <summary>الرقم الضريبي.</summary>
        public string? Tax_Number { get; set; }

        /// <summary>رقم الهاتف.</summary>
        public string? Phone { get; set; }

        /// <summary>رقم الجوال.</summary>
        public string? Mobile { get; set; }

        /// <summary>البريد الإلكتروني.</summary>
        public string? Email { get; set; }

        /// <summary>عنوان الشركة.</summary>
        public string? Address { get; set; }

        /// <summary>شعار الشركة.</summary>
        public byte[]? Company_Logo { get; set; }
    }
}
