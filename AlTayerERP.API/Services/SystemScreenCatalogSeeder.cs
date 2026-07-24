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
                new ScreenSeed("FiscalYears", "السنوات المالية", "الإدارة العامة", 30),
                new ScreenSeed("Users", "المستخدمون", "الإدارة العامة", 40),
                new ScreenSeed("Roles", "الأدوار", "الإدارة العامة", 50),
                new ScreenSeed("RolePermissions", "صلاحيات الأدوار", "الإدارة العامة", 60),
                new ScreenSeed("AuditLogs", "سجل التدقيق والرقابة", "الإدارة العامة", 70),
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
                new ScreenSeed("ReceiptVoucher", "سند القبض", "الحسابات", 220)
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

            if (missingScreens.Count == 0)
                return;

            await _context.SystemScreens.AddRangeAsync(missingScreens);
            await _context.SaveChangesAsync();
        }

        private sealed record ScreenSeed(string Code, string Name, string Module, int SortOrder);
    }
}