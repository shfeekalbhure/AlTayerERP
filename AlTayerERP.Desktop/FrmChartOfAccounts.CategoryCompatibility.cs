namespace AlTayerERP.Desktop
{
    public partial class FrmChartOfAccounts
    {
        /// <summary>
        /// دالة توافق للاستدعاءات القديمة الموجودة في ملفات التخطيط الجزئية للشاشة.
        /// تعرض الاسم العربي من التصنيفات المحملة ديناميكياً، مع قيم احتياطية
        /// فقط قبل اكتمال تحميل التصنيفات من واجهة API.
        /// </summary>
        private string ConvertDbToCategory(string? categoryCode)
        {
            if (string.IsNullOrWhiteSpace(categoryCode))
                return string.Empty;

            string dynamicName = CategoryName(categoryCode);
            if (!string.Equals(dynamicName, categoryCode, System.StringComparison.OrdinalIgnoreCase))
                return dynamicName;

            return categoryCode switch
            {
                "Cash" => "نقدية وصناديق",
                "Bank" => "بنوك",
                "Receivable" => "ذمم مدينة",
                "Customer" => "عملاء",
                "Inventory" => "مخزون",
                "FixedAsset" => "أصول ثابتة",
                "OtherAsset" => "أصول أخرى",
                "Payable" => "ذمم دائنة",
                "Vendor" => "موردون",
                "Loan" => "قروض",
                "TaxPayable" => "ضرائب مستحقة",
                "OtherLiability" => "خصوم أخرى",
                "Capital" => "رأس المال",
                "Reserve" => "احتياطيات",
                "RetainedEarnings" => "أرباح محتجزة",
                "OtherEquity" => "حقوق ملكية أخرى",
                "TransportRevenue" => "إيرادات نقل",
                "ShippingRevenue" => "إيرادات شحن",
                "TicketRevenue" => "إيرادات تذاكر",
                "ServiceRevenue" => "إيرادات خدمات",
                "OtherRevenue" => "إيرادات أخرى",
                "FuelExpense" => "مصروف وقود",
                "SalaryExpense" => "مصروف رواتب",
                "MaintenanceExpense" => "مصروف صيانة",
                "RentExpense" => "مصروف إيجارات",
                "OperatingExpense" => "مصروفات تشغيلية",
                "AdministrativeExpense" => "مصروفات إدارية",
                "FinanceCost" => "تكاليف تمويل",
                "DepreciationExpense" => "مصروف إهلاك",
                "OtherExpense" => "مصروفات أخرى",
                _ => categoryCode
            };
        }
    }
}
