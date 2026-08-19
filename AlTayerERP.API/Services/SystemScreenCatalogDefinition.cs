namespace AlTayerERP.API.Services
{
    /// <summary>
    /// التعريف الرسمي لشاشات المرحلة الأولى التي تملك نموذج سطح مكتب فعلياً.
    /// يستخدمه Seeder وواجهة API معاً حتى لا يسجل الكتالوج كوداً لا تستطيع
    /// شجرة النظام فتحه.
    /// </summary>
    public static class SystemScreenCatalogDefinition
    {
        public static readonly IReadOnlyList<SystemScreenDefinition> Items = new SystemScreenDefinition[]
        {
            new("TenantGroups", "المجموعات التجارية", "الإدارة العامة", 5),
            new("Companies", "الشركات", "الإدارة العامة", 10),
            new("Branches", "الفروع", "الإدارة العامة", 20),
            new("Countries", "الدول", "الإدارة العامة", 25),
            new("Governorates", "المحافظات", "الإدارة العامة", 26),
            new("Cities", "المدن", "الإدارة العامة", 27),
            new("FiscalYears", "السنوات المالية", "الإدارة العامة", 30),
            new("Users", "المستخدمون", "الإدارة العامة", 40),
            new("Roles", "الأدوار", "الإدارة العامة", 50),
            new("RolePermissions", "صلاحيات الأدوار", "الإدارة العامة", 60),
            new("AuditLogs", "سجل التدقيق والرقابة", "الإدارة العامة", 70),
            new("Sessions", "الجلسات النشطة", "الإدارة العامة", 80),
            new("GeneralSettings", "الإعدادات العامة والمالية", "التهيئة والإعدادات", 70),
            new("SystemScreens", "كتالوج شاشات النظام", "التهيئة والإعدادات", 80),
            new("NumberingSettings", "إعدادات الترقيم", "التهيئة والإعدادات", 90),
            new("FiscalPeriods", "الفترات المالية", "التهيئة والإعدادات", 100),
            new("ExchangeRates", "أسعار الصرف", "التهيئة والإعدادات", 110),
            new("PaymentMethods", "طرق السداد", "التهيئة والإعدادات", 120),
            new("VoucherTypes", "أنواع السندات", "التهيئة والإعدادات", 130),
            new("VoucherStatuses", "حالات السندات", "التهيئة والإعدادات", 140),
            new("ApprovalPolicies", "سياسات الاعتماد والسقوف", "التهيئة والإعدادات", 150),
            new("ChartOfAccounts", "الدليل المحاسبي", "الحسابات", 160),
            new("Currencies", "العملات", "الحسابات", 170),
            new("CostCenters", "مراكز التكلفة", "الحسابات", 180),
            new("CashBoxes", "الصناديق", "الحسابات", 190),
            new("Banks", "البنوك والحسابات البنكية", "الحسابات", 200),
            new("Parties", "الأطراف المالية", "الحسابات", 210),
            new("ReceiptVoucher", "سند القبض", "الحسابات", 220),
            new("PaymentVoucher", "سند الصرف", "الحسابات", 230),
            new("PaymentRequest", "طلب الصرف", "الحسابات", 235),
            new("JournalVoucher", "القيد اليومي", "الحسابات", 240),
            new("DocumentSearch", "البحث عن المستندات", "الحسابات", 250),
            new("ApprovalRequests", "طلبات الاعتماد", "الحسابات", 260),
            new("FinancialLimits", "السقوف المالية وحركات الاستخدام", "الحسابات", 270),
            new("TrialBalance", "ميزان المراجعة", "التقارير المالية", 280),
            new("GeneralLedger", "الأستاذ العام", "التقارير المالية", 290)
        };

        /// <summary>يتحقق من الكود حرفياً مع مراعاة حالة الأحرف.</summary>
        public static bool IsSupported(string? code) =>
            !string.IsNullOrWhiteSpace(code) &&
            Items.Any(item => string.Equals(item.Code, code, StringComparison.Ordinal));
    }

    public sealed record SystemScreenDefinition(string Code, string Name, string Module, int SortOrder);
}
