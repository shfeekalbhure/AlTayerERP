using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Services
{
    /// <summary>
    /// يضمن وجود كتالوج الشاشات الأساسية للمرحلة الأولى.
    /// لا يحذف سجلات موجودة ولا يستبدل أسماء مخصصة أدخلها مدير النظام.
    /// </summary>
    public sealed class SystemScreenCatalogSeeder
    {
        private readonly AppDbContext _context;

        public SystemScreenCatalogSeeder(AppDbContext context) => _context = context;

        public async Task EnsureSeededAsync()
        {
            var catalog = new[]
            {
                new ScreenSeed("TenantGroups", "المجموعات التجارية", "الإدارة العامة", 5),
                new ScreenSeed("Companies", "الشركات", "الإدارة العامة", 10),
                new ScreenSeed("Branches", "الفروع", "الإدارة العامة", 20),
                new ScreenSeed("Countries", "الدول", "الإدارة العامة", 25),
                new ScreenSeed("Governorates", "المحافظات", "الإدارة العامة", 26),
                new ScreenSeed("Cities", "المدن", "الإدارة العامة", 27),
                new ScreenSeed("FiscalYears", "السنوات المالية", "الإدارة العامة", 30),
                new ScreenSeed("Users", "المستخدمون", "الإدارة العامة", 40),
                new ScreenSeed("Roles", "الأدوار", "الإدارة العامة", 50),
                new ScreenSeed("RolePermissions", "صلاحيات الأدوار", "الإدارة العامة", 60),
                new ScreenSeed("AuditLogs", "سجل التدقيق والرقابة", "الإدارة العامة", 70),
                new ScreenSeed("Sessions", "الجلسات النشطة", "الإدارة العامة", 80),
                new ScreenSeed("GeneralSettings", "الإعدادات العامة والمالية", "التهيئة والإعدادات", 70),
                new ScreenSeed("SystemScreens", "كتالوج شاشات النظام", "التهيئة والإعدادات", 80),
                new ScreenSeed("NumberingSettings", "إعدادات الترقيم", "التهيئة والإعدادات", 90),
                new ScreenSeed("FiscalPeriods", "الفترات المالية", "التهيئة والإعدادات", 100),
                new ScreenSeed("ExchangeRates", "أسعار الصرف", "التهيئة والإعدادات", 110),
                new ScreenSeed("PaymentMethods", "طرق السداد", "التهيئة والإعدادات", 120),
                new ScreenSeed("VoucherTypes", "أنواع السندات", "التهيئة والإعدادات", 130),
                new ScreenSeed("VoucherStatuses", "حالات السندات", "التهيئة والإعدادات", 140),
                new ScreenSeed("ApprovalPolicies", "سياسات الاعتماد والسقوف", "التهيئة والإعدادات", 150),
                new ScreenSeed("ChartOfAccounts", "الدليل المحاسبي", "الحسابات", 160),
                new ScreenSeed("Currencies", "العملات", "الحسابات", 170),
                new ScreenSeed("CostCenters", "مراكز التكلفة", "الحسابات", 180),
                new ScreenSeed("CashBoxes", "الصناديق", "الحسابات", 190),
                new ScreenSeed("Banks", "البنوك والحسابات البنكية", "الحسابات", 200),
                new ScreenSeed("Parties", "الأطراف المالية", "الحسابات", 210),
                new ScreenSeed("ReceiptVoucher", "سند القبض", "الحسابات", 220),
                new ScreenSeed("PaymentVoucher", "سند الصرف", "الحسابات", 230),
                new ScreenSeed("PaymentRequest", "طلب الصرف", "الحسابات", 235),
                new ScreenSeed("JournalVoucher", "القيد اليومي", "الحسابات", 240),
                new ScreenSeed("DocumentSearch", "البحث عن المستندات", "الحسابات", 250),
                new ScreenSeed("ApprovalRequests", "طلبات الاعتماد", "الحسابات", 260),
                new ScreenSeed("FinancialLimits", "السقوف المالية وحركات الاستخدام", "الحسابات", 270),
                new ScreenSeed("TrialBalance", "ميزان المراجعة", "التقارير المالية", 280),
                new ScreenSeed("GeneralLedger", "الأستاذ العام", "التقارير المالية", 290),
                new ScreenSeed("JournalEntryView", "عرض القيد المحاسبي", "التقارير المالية", 300)
            };

            var knownCodes = await _context.SystemScreens
                .AsNoTracking()
                .Select(x => x.Screen_Code)
                .ToListAsync();

            var missingScreens = catalog
                .Where(item => !knownCodes.Contains(item.Code, StringComparer.OrdinalIgnoreCase))
                .Select(item => new SystemScreen
                {
                    Screen_Code = item.Code,
                    Screen_Name = item.Name,
                    Module_Name = item.Module,
                    Sort_Order = item.SortOrder,
                    Is_Active = true,
                    Created_At = DateTime.Now
                })
                .ToList();

            if (missingScreens.Count > 0)
                await _context.SystemScreens.AddRangeAsync(missingScreens);

            await EnsureScreenActionCatalogAsync();

            if (missingScreens.Count > 0 || _context.ChangeTracker.HasChanges())
                await _context.SaveChangesAsync();
        }

        /// <summary>
        /// تعريف إجراءات الشاشة الثابتة. هذه ليست أرقام مستندات ولا تتبع محرك الترقيم،
        /// بل أكواد صلاحيات تستخدمها الواجهة والـ API والتدقيق.
        /// </summary>
        private async Task EnsureScreenActionCatalogAsync()
        {
            var actions = new[]
            {
                new ActionSeed("SCREEN.VIEW", "عرض الشاشة (View)", 10),
                new ActionSeed("SCREEN.ADD", "إضافة (Create)", 20),
                new ActionSeed("SCREEN.EDIT", "تعديل (Edit)", 30),
                new ActionSeed("SCREEN.DELETE", "إيقاف أو حذف منطقي (Delete)", 40),
                new ActionSeed("SCREEN.PRINT", "طباعة (Print)", 50),
                new ActionSeed("SCREEN.EXPORT", "تصدير (Export)", 60),
                new ActionSeed("SCREEN.IMPORT", "استيراد (Import)", 70),
                new ActionSeed("SCREEN.APPROVE", "اعتماد (Approve)", 80),
                new ActionSeed("SCREEN.UNAPPROVE", "فك الاعتماد أو الترحيل (Unapprove)", 90)
            };

            var known = await _context.System_Permissions.AsNoTracking()
                .Select(x => x.Permission_Code).ToListAsync();

            var missing = actions.Where(x => !known.Contains(x.Code, StringComparer.OrdinalIgnoreCase))
                .Select(x => new SystemPermission
                {
                    Permission_Code = x.Code,
                    Permission_Name = x.Name,
                    Permission_Type = "ACTION",
                    Module_Name = "Security",
                    Sort_Order = x.SortOrder,
                    Is_Active = true,
                    Created_At = DateTime.UtcNow
                }).ToList();

            if (missing.Count > 0)
                await _context.System_Permissions.AddRangeAsync(missing);
        }

        private sealed record ActionSeed(string Code, string Name, int SortOrder);
        private sealed record ScreenSeed(string Code, string Name, string Module, int SortOrder);
    }
}