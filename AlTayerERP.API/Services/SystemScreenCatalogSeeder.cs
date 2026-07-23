using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Services;

/// <summary>
/// يضمن وجود كتالوج الشاشات والصلاحيات الأساسية للمرحلة الأولى.
/// الإضافة فقط (Additive)؛ لا يحذف سجلات أو يبدل إعدادات المدير.
/// </summary>
public sealed class SystemScreenCatalogSeeder
{
    private readonly AppDbContext _context;

    public SystemScreenCatalogSeeder(AppDbContext context) => _context = context;

    /// <summary>تهيئة كتالوج الشاشات ثم كتالوج أكواد الصلاحيات الدقيقة.</summary>
    public async Task EnsureSeededAsync()
    {
        var screens = new[]
        {
            new ScreenSeed("Companies", "الشركات", "الإدارة العامة", 10),
            new ScreenSeed("Branches", "الفروع", "الإدارة العامة", 20),
            new ScreenSeed("FiscalYears", "السنوات المالية", "الإدارة العامة", 30),
            new ScreenSeed("Users", "المستخدمون", "الإدارة العامة", 40),
            new ScreenSeed("Roles", "الأدوار", "الإدارة العامة", 50),
            new ScreenSeed("RolePermissions", "صلاحيات الأدوار", "الإدارة العامة", 60),
            new ScreenSeed("GeneralSettings", "الإعدادات العامة والمالية", "التهيئة والإعدادات", 70),
            new ScreenSeed("NumberingSettings", "إعدادات الترقيم", "التهيئة والإعدادات", 90),
            new ScreenSeed("FiscalPeriods", "الفترات المالية", "التهيئة والإعدادات", 100),
            new ScreenSeed("ExchangeRates", "أسعار الصرف", "التهيئة والإعدادات", 110),
            new ScreenSeed("ChartOfAccounts", "الدليل المحاسبي", "الحسابات", 160),
            new ScreenSeed("Currencies", "العملات", "الحسابات", 170),
            new ScreenSeed("CostCenters", "مراكز التكلفة", "الحسابات", 180),
            new ScreenSeed("CashBoxes", "الصناديق", "الحسابات", 190),
            new ScreenSeed("Banks", "البنوك والحسابات البنكية", "الحسابات", 200),
            new ScreenSeed("Parties", "الأطراف المالية", "الحسابات", 210),
            new ScreenSeed("JournalEntry", "قيد اليومية", "الحسابات", 220),
            new ScreenSeed("ReceiptVoucher", "سند القبض", "الحسابات", 230),
            new ScreenSeed("PaymentVoucher", "سند الصرف", "الحسابات", 240),
            new ScreenSeed("AuditLogs", "سجل التدقيق", "الرقابة", 250)
        };

        var permissions = new[]
        {
            new PermissionSeed("Users.View", "عرض المستخدمين", "Screen", "Administration", 10),
            new PermissionSeed("Users.Create", "إنشاء مستخدم", "Action", "Administration", 11),
            new PermissionSeed("Users.Edit", "تعديل مستخدم", "Action", "Administration", 12),
            new PermissionSeed("Users.Deactivate", "إيقاف مستخدم", "Action", "Administration", 13),
            new PermissionSeed("Roles.View", "عرض الأدوار", "Screen", "Administration", 20),
            new PermissionSeed("Roles.Create", "إنشاء دور", "Action", "Administration", 21),
            new PermissionSeed("Roles.Edit", "تعديل دور", "Action", "Administration", 22),
            new PermissionSeed("Roles.Deactivate", "إيقاف دور", "Action", "Administration", 23),
            new PermissionSeed("RolePermissions.View", "عرض صلاحيات الأدوار", "Screen", "Administration", 30),
            new PermissionSeed("RolePermissions.Edit", "إدارة صلاحيات الأدوار", "Action", "Administration", 31),
            new PermissionSeed("JournalEntry.View", "عرض قيود اليومية", "Screen", "Accounting", 40),
            new PermissionSeed("JournalEntry.Create", "إنشاء قيد يومية", "Action", "Accounting", 41),
            new PermissionSeed("JournalEntry.Approve", "اعتماد قيد يومية", "Action", "Accounting", 42),
            new PermissionSeed("ReceiptVoucher.View", "عرض سندات القبض", "Screen", "Accounting", 50),
            new PermissionSeed("ReceiptVoucher.Create", "إنشاء سند قبض", "Action", "Accounting", 51),
            new PermissionSeed("ReceiptVoucher.EditPending", "تعديل سند قبض معلق", "Action", "Accounting", 52),
            new PermissionSeed("ReceiptVoucher.Approve", "اعتماد وترحيل سند قبض", "Action", "Accounting", 53),
            new PermissionSeed("ReceiptVoucher.Unpost", "فك ترحيل سند قبض", "Action", "Accounting", 54),
            new PermissionSeed("ReceiptVoucher.Cancel", "إلغاء سند قبض", "Action", "Accounting", 55),
            new PermissionSeed("ReceiptVoucher.PrintDraft", "طباعة مسودة سند قبض", "Action", "Accounting", 56),
            new PermissionSeed("ReceiptVoucher.PrintPosted", "طباعة سند قبض مرحل", "Action", "Accounting", 57),
            new PermissionSeed("ReceiptVoucher.Reprint", "إعادة طباعة سند قبض", "Action", "Accounting", 58),
            new PermissionSeed("ReceiptVoucher.ChangeDate", "تغيير تاريخ سند قبض", "Field", "Accounting", 59),
            new PermissionSeed("ReceiptVoucher.ChangeExchangeRate", "تغيير سعر صرف سند قبض", "Field", "Accounting", 60),
            new PermissionSeed("ReceiptVoucher.ViewJournalEntry", "عرض قيد سند القبض", "Action", "Accounting", 61),
            new PermissionSeed("ReceiptVoucher.ViewOtherBranches", "عرض سندات فروع أخرى", "Data", "Accounting", 62),
            new PermissionSeed("ReceiptVoucher.ApproveAboveLimit", "اعتماد سند قبض فوق الحد", "Action", "Accounting", 63),
            new PermissionSeed("PaymentVoucher.View", "عرض سندات الصرف", "Screen", "Accounting", 70),
            new PermissionSeed("PaymentVoucher.Create", "إنشاء سند صرف", "Action", "Accounting", 71),
            new PermissionSeed("PaymentVoucher.Approve", "اعتماد وترحيل سند صرف", "Action", "Accounting", 72),
            new PermissionSeed("PaymentVoucher.Cancel", "إلغاء سند صرف", "Action", "Accounting", 73),
            new PermissionSeed("AuditLogs.View", "عرض سجل التدقيق", "Screen", "Audit", 80)
        };

        var knownScreenCodes = await _context.SystemScreens.AsNoTracking()
            .Select(x => x.Screen_Code).ToListAsync();
        var missingScreens = screens
            .Where(x => !knownScreenCodes.Contains(x.Code, StringComparer.OrdinalIgnoreCase))
            .Select(x => new SystemScreen
            {
                Screen_Code = x.Code, Screen_Name = x.Name, Module_Name = x.Module,
                Sort_Order = x.SortOrder, Is_Active = true, Created_At = DateTime.UtcNow
            }).ToList();

        var knownPermissionCodes = await _context.System_Permissions.AsNoTracking()
            .Select(x => x.Permission_Code).ToListAsync();
        var missingPermissions = permissions
            .Where(x => !knownPermissionCodes.Contains(x.Code, StringComparer.OrdinalIgnoreCase))
            .Select(x => new SystemPermission
            {
                Permission_Code = x.Code, Permission_Name = x.Name, Permission_Type = x.Type,
                Module_Name = x.Module, Sort_Order = x.SortOrder, Is_Active = true,
                Created_At = DateTime.UtcNow
            }).ToList();

        if (missingScreens.Count == 0 && missingPermissions.Count == 0) return;
        await _context.SystemScreens.AddRangeAsync(missingScreens);
        await _context.System_Permissions.AddRangeAsync(missingPermissions);
        await _context.SaveChangesAsync();
    }

    /// <summary>صف تهيئة شاشة بالاسم والكود والقسم والترتيب.</summary>
    private sealed record ScreenSeed(string Code, string Name, string Module, int SortOrder);

    /// <summary>صف تهيئة صلاحية دقيقة بالاسم والكود والنوع والوحدة.</summary>
    private sealed record PermissionSeed(string Code, string Name, string Type, string Module, int SortOrder);
}