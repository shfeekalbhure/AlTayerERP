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
            var catalog = SystemScreenCatalogDefinition.Items;

            var existingScreens = await _context.SystemScreens.ToListAsync();

            foreach (var item in catalog)
            {
                var existing = existingScreens.FirstOrDefault(x =>
                    string.Equals(x.Screen_Code, item.Code, StringComparison.OrdinalIgnoreCase));

                if (existing != null && !string.Equals(existing.Screen_Code, item.Code, StringComparison.Ordinal))
                    existing.Screen_Code = item.Code;
            }

            var missingScreens = catalog
                .Where(item => !existingScreens.Any(x =>
                    string.Equals(x.Screen_Code, item.Code, StringComparison.OrdinalIgnoreCase)))
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

            // هذان العنصران عمليتان سياقيتان داخل شاشات أخرى وليسا شاشتين مستقلتين.
            // إيقافهما يمنع ظهورهما في شجرة النظام دون حذف سجلات أو صلاحيات تاريخية.
            var contextualOnlyCodes = new[] { "PaymentRequestAttachments", "JournalEntryView" };
            foreach (var contextualScreen in existingScreens.Where(screen =>
                         contextualOnlyCodes.Contains(screen.Screen_Code, StringComparer.OrdinalIgnoreCase)))
                contextualScreen.Is_Active = false;

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
    }
}
