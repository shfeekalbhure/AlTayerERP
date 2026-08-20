using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Services
{
    /// <summary>
    /// يحل الإعداد الفعّال وفق الأولوية: السنة المالية ثم الفرع ثم الشركة ثم النظام.
    /// لا يعتمد على قيمة من العميل؛ يستعمل نطاق ServerSession.
    /// </summary>
    public sealed class SettingsResolverService
    {
        private readonly AppDbContext _context;

        public SettingsResolverService(AppDbContext context) => _context = context;

        public async Task<ResolvedSetting?> ResolveAsync(
            string settingKey,
            ServerSession session,
            DateTime? effectiveAt = null,
            CancellationToken cancellationToken = default)
        {
            var key = settingKey?.Trim();
            if (string.IsNullOrWhiteSpace(key)) return null;
            var at = (effectiveAt ?? DateTime.UtcNow).Date;

            var rows = await _context.System_Settings.AsNoTracking()
                .Where(x => x.Setting_Key == key && x.Is_Active &&
                    (!x.Effective_Date.HasValue || x.Effective_Date.Value <= at) &&
                    (x.Scope == "SYSTEM" ||
                     (x.Scope == "COMPANY" && x.Company_ID == session.Company_ID && x.Branch_ID == 0 && x.Fiscal_Year_ID == 0) ||
                     (x.Scope == "BRANCH" && x.Company_ID == session.Company_ID && x.Branch_ID == session.Branch_ID && x.Fiscal_Year_ID == 0) ||
                     (x.Scope == "FISCAL_YEAR" && x.Company_ID == session.Company_ID && x.Branch_ID == session.Branch_ID && x.Fiscal_Year_ID == session.Year_ID)))
                .ToListAsync(cancellationToken);

            var selected = rows
                .OrderByDescending(x => ScopePriority(x.Scope))
                .ThenByDescending(x => x.Effective_Date ?? DateTime.MinValue)
                .ThenByDescending(x => x.Setting_ID)
                .FirstOrDefault();

            return selected == null ? null : new ResolvedSetting(
                selected.Setting_Key, selected.Setting_Value, selected.Scope,
                selected.Setting_ID, selected.Effective_Date);
        }

        private static int ScopePriority(string scope) => scope.ToUpperInvariant() switch
        {
            "FISCAL_YEAR" => 4,
            "BRANCH" => 3,
            "COMPANY" => 2,
            "SYSTEM" => 1,
            _ => 0
        };
    }

    public sealed record ResolvedSetting(
        string Setting_Key,
        string Setting_Value,
        string Scope,
        int Setting_ID,
        DateTime? Effective_Date);
}
