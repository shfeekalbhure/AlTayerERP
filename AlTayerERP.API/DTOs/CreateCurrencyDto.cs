namespace AlTayerERP.API.DTOs
{
    /// <summary>
    /// نموذج إنشاء وتعديل العملة.
    /// </summary>
    public class CreateCurrencyDto
    {
        public string Company_ID { get; set; } = string.Empty;

        public string Currency_Code { get; set; } = string.Empty;

        public string Currency_Name_AR { get; set; } = string.Empty;

        public string? Currency_Name_EN { get; set; }

        public string? Currency_Symbol { get; set; }

        public int Decimal_Places { get; set; } = 2;

        public decimal Exchange_Rate { get; set; } = 1;

        public decimal Min_Exchange_Rate { get; set; } = 1;

        public decimal Max_Exchange_Rate { get; set; } = 1;

        public bool Is_Local_Currency { get; set; }

        /// <summary>
        /// العملة الافتراضية.
        /// تحدد العملة التي تظهر تلقائيًا
        /// عند فتح السندات والشاشات المالية.
        /// </summary>
        public bool Is_Default { get; set; }

        public bool Is_Active { get; set; } = true;

        public string? Notes { get; set; }

        public string? Created_By { get; set; }

        public string? Updated_By { get; set; }
    }
}