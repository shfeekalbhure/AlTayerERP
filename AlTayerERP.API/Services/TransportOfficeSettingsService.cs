using AlTayerERP.API.DTOs;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Services;

public sealed class TransportOfficeSettingsService(AppDbContext context)
{
    public const string CurrentAccountSettingKey = "BRANCH_CURRENT_ACCOUNT_ID";

    public async Task<string?> ValidateAsync(CreateBranchDto request, TenantBranch? existing)
    {
        var office = request.OfficeSettings;
        if (office is null)
            return null;

        if (office.Credit_Limit is { } limit &&
            (!request.Allow_Credit || !IsValidAmount(limit)))
            return "أدخل سقفًا غير سالب عند السماح بالآجل.";

        var commissions = new[] { office.Passenger_Commission, office.Outbound_Commission, office.Inbound_Commission };
        if (commissions.Any(x => x != null && (!office.Allow_Commission || !IsValidCommission(x))))
            return "حدد لكل عمولة مفعّلة نسبة بين صفر و100 أو مبلغًا ثابتًا غير سالب.";

        if (office.Allow_Commission && commissions.All(x => x == null))
            return "حدد عمولة واحدة على الأقل من الركاب أو الصادر أو الوارد.";

        var accountId = CleanText(office.Analytical_Account_ID);
        if (accountId == null || (existing != null && accountId == existing.Analytical_Account_ID && request.Company_ID == existing.Company_ID))
            return null;

        if (!await EligibleAccounts(request.Company_ID).AnyAsync(x => x.Account_ID == accountId))
            return "اختر حسابًا تحليليًا نشطًا تحت حساب جاري الفروع المحدد للشركة.";

        if (await context.Tenant_Branches.AnyAsync(x => x.Analytical_Account_ID == accountId))
            return "الحساب التحليلي مرتبط بفرع آخر.";

        return null;
    }

    public IQueryable<ChartOfAccount> EligibleAccounts(string companyId)
    {
        var configuredParents = context.System_Settings
            .Where(x => x.Company_ID == companyId && x.Scope == "COMPANY" &&
                x.Branch_ID == 0 && x.Fiscal_Year_ID == 0 && x.Is_Active &&
                x.Setting_Key == CurrentAccountSettingKey &&
                (x.Effective_Date == null || x.Effective_Date <= DateTime.Today))
            .Select(x => x.Setting_Value.Trim());

        var parents = context.Chart_Of_Accounts
            .Where(x => x.Company_ID == companyId && x.Is_Active && !x.Is_Postable &&
                configuredParents.Contains(x.Account_ID))
            .Select(x => x.Account_ID);

        return context.Chart_Of_Accounts.AsNoTracking()
            .Where(x => x.Company_ID == companyId && x.Is_Active && x.Is_Postable &&
                !x.Is_Summary_Account && x.Parent_Account_ID != null &&
                parents.Contains(x.Parent_Account_ID));
    }

    public static void Apply(TenantBranch branch, TransportOfficeSettingsDto? office)
    {
        if (office == null)
            return;

        branch.Country = CleanText(office.Country);
        branch.Governorate = CleanText(office.Governorate);
        branch.District = CleanText(office.District);
        branch.Locality = CleanText(office.Locality);
        branch.Credit_Limit = office.Credit_Limit;
        branch.Allow_Commission = office.Allow_Commission;
        branch.Passenger_Commission_Method = office.Passenger_Commission?.Method;
        branch.Passenger_Commission_Value = office.Passenger_Commission?.Value;
        branch.Outbound_Commission_Method = office.Outbound_Commission?.Method;
        branch.Outbound_Commission_Value = office.Outbound_Commission?.Value;
        branch.Inbound_Commission_Method = office.Inbound_Commission?.Method;
        branch.Inbound_Commission_Value = office.Inbound_Commission?.Value;
        branch.Analytical_Account_ID = CleanText(office.Analytical_Account_ID);
    }

    private static string? CleanText(string? text) =>
        string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    private static bool IsValidAmount(decimal value) =>
        value >= 0 && value <= 999_999_999_999.999999m && decimal.Round(value, 6) == value;

    private static bool IsValidCommission(OfficeCommissionDto commission) =>
        IsValidAmount(commission.Value) && (commission.Method switch
        {
            "Percentage" => commission.Value <= 100,
            "FixedAmount" => true,
            _ => false
        });
}
