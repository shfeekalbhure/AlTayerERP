using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Services;

/// <summary>
/// يهيئ الثوابت التقنية اللازمة للنظام تلقائياً. لا ينشئ شركة أو فرعاً أو مستخدماً؛
/// تلك بيانات منشأة تدخل مرة واحدة من معالج التهيئة الأولية.
/// </summary>
public sealed class SystemBootstrapSeeder
{
    private readonly AppDbContext _context;
    public SystemBootstrapSeeder(AppDbContext context) => _context = context;

    public async Task EnsureSeededAsync()
    {
        await AddNumberingIfMissingAsync("BUSINESS_GROUP", "GRP", 4, "NONE", false, false, false);
        await AddNumberingIfMissingAsync("COMPANY", "CMP", 4, "NONE", false, false, false);
        await AddNumberingIfMissingAsync("BRANCH", "BR", 4, "COMPANY", true, false, false);
        await AddNumberingIfMissingAsync("RECEIPT", "RCV", 6, "BRANCH_YEAR", true, true, true);
        await AddNumberingIfMissingAsync("PAYMENT", "PAY", 6, "BRANCH_YEAR", true, true, true);
        await AddNumberingIfMissingAsync("JOURNAL", "JV", 6, "BRANCH_YEAR", true, true, true);

        await AddBranchTypeIfMissingAsync("MAIN", "فرع رئيسي", "Main Branch", 10);
        await AddBranchTypeIfMissingAsync("OPERATING", "فرع تشغيلي", "Operating Branch", 20);
        await AddBranchTypeIfMissingAsync("OFFICE", "مكتب", "Office", 30);
        await AddBranchTypeIfMissingAsync("AGENCY", "وكالة", "Agency", 40);
        await AddBranchTypeIfMissingAsync("WAREHOUSE", "مستودع", "Warehouse", 50);
        await AddBranchTypeIfMissingAsync("SERVICE_POINT", "نقطة خدمة", "Service Point", 60);
        await _context.SaveChangesAsync();
    }

    private async Task AddNumberingIfMissingAsync(string type, string prefix, int digits, string reset, bool company, bool branch, bool year)
    {
        if (await _context.Numbering_Settings.AnyAsync(x => x.Document_Type == type)) return;
        _context.Numbering_Settings.Add(new NumberingSetting
        {
            Document_Type = type, Prefix = prefix, Digits_Count = digits, Reset_Type = reset,
            Last_Number = 0, Use_Company = company, Use_Branch = branch, Use_Year = year, Is_Active = true
        });
    }

    private async Task AddBranchTypeIfMissingAsync(string code, string ar, string en, int order)
    {
        if (await _context.Branch_Types.AnyAsync(x => x.Branch_Type_Code == code)) return;
        _context.Branch_Types.Add(new BranchType
        {
            Branch_Type_Code = code, Branch_Type_Name_AR = ar, Branch_Type_Name_EN = en,
            Sort_Order = order, Is_Active = true, Created_At = DateTime.Now
        });
    }
}
