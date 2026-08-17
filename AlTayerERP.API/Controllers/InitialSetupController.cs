using AlTayerERP.API.Security;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers;

/// <summary>تهيئة أول تشغيل: بيانات المنشأة فقط، بينما القيم النظامية تُنشأ تلقائياً.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class InitialSetupController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly NumberGeneratorService _numbers;
    private readonly SystemBootstrapSeeder _bootstrap;

    public InitialSetupController(AppDbContext context, NumberGeneratorService numbers, SystemBootstrapSeeder bootstrap)
    { _context = context; _numbers = numbers; _bootstrap = bootstrap; }

    [HttpGet("Status")]
    public async Task<IActionResult> Status() => Ok(new { Is_Required = !await _context.Companies.AnyAsync() || !await _context.Users.AnyAsync() });

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] InitialSetupRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Group_Name_AR) ||
            string.IsNullOrWhiteSpace(request.Company_Name_AR) || string.IsNullOrWhiteSpace(request.Branch_Name) ||
            string.IsNullOrWhiteSpace(request.Admin_Login_Name) || string.IsNullOrWhiteSpace(request.Admin_Password))
            return BadRequest(new { message = "اسم المجموعة والشركة والفرع وبيانات مدير النظام مطلوبة." });
        if (request.Admin_Password.Length < 8)
            return BadRequest(new { message = "كلمة مرور مدير النظام يجب ألا تقل عن 8 أحرف." });
        if (await _context.Companies.AnyAsync() || await _context.Users.AnyAsync())
            return Conflict(new { message = "اكتملت التهيئة الأولية. استخدم شاشات الإدارة لإضافة بيانات جديدة." });

        await _bootstrap.EnsureSeededAsync();
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var group = new TenantGroup { Group_ID = Guid.NewGuid().ToString(), Group_Name_AR = request.Group_Name_AR.Trim(), Group_Name_EN = request.Group_Name_EN?.Trim() ?? string.Empty, Is_Active = true, Created_At = DateTime.UtcNow };
            _context.Tenant_Groups.Add(group);
            var companyId = await _numbers.GenerateNextNumberAsync("COMPANY");
            var company = new Company { Company_ID = companyId, Group_ID = group.Group_ID, Company_Name_AR = request.Company_Name_AR.Trim(), Company_Name_EN = request.Company_Name_EN?.Trim() ?? string.Empty, Company_Prefix = request.Company_Prefix?.Trim().ToUpperInvariant(), Is_Active = true, Created_At = DateTime.UtcNow };
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            var currency = new Currency { Company_ID = companyId, Currency_Code = "YER", Currency_Name_AR = "ريال يمني", Currency_Name_EN = "Yemeni Rial", Currency_Symbol = "ر.ي", Decimal_Places = 2, Exchange_Rate = 1, Min_Exchange_Rate = 1, Max_Exchange_Rate = 1, Is_Local_Currency = true, Is_Default = true, Is_Active = true, Created_At = DateTime.UtcNow };
            _context.Currencies.Add(currency);
            await _context.SaveChangesAsync();

            var branchCode = await _numbers.GenerateNextNumberAsync("BRANCH", companyId);
            var branch = new TenantBranch { Company_ID = companyId, Branch_Code = branchCode, Branch_Name = request.Branch_Name.Trim(), Branch_Name_EN = request.Branch_Name_EN?.Trim(), Branch_Type = "MAIN", Currency_ID = currency.Currency_ID, Is_Active = true, Created_Date = DateTime.Now };
            _context.Tenant_Branches.Add(branch);
            var yearStart = new DateTime(request.Fiscal_Year <= 0 ? DateTime.Today.Year : request.Fiscal_Year, 1, 1);
            var year = new FiscalYear { Company_ID = companyId, Year_Name = yearStart.Year.ToString(), Start_Date = yearStart, End_Date = yearStart.AddYears(1).AddDays(-1), Is_Default = true, Is_Active = true, Created_At = DateTime.Now };
            _context.Fiscal_Years.Add(year);
            await _context.SaveChangesAsync();

            var role = new Role { Role_Code = "SYSTEM_ADMIN", Role_Name = "مدير النظام", Description = "إدارة كاملة للنظام", Is_System_Admin = true, Is_Active = true, Created_At = DateTime.Now };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            _context.Users.Add(new User { Company_ID = companyId, Branch_ID = branch.Branch_ID, Role_ID = role.Role_ID, User_Code = "ADMIN", Full_Name = request.Admin_Full_Name?.Trim() ?? "مدير النظام", Login_Name = request.Admin_Login_Name.Trim(), Password_Hash = PasswordProtector.Hash(request.Admin_Password), Must_Change_Password = true, Is_Active = true, Created_At = DateTime.Now });
            _context.System_Settings.AddRange(
                new SystemSetting { Setting_Key = "DEFAULT_CURRENCY", Setting_Name = "العملة الافتراضية", Setting_Value = "YER", Scope = "COMPANY", Company_ID = companyId, Is_Active = true },
                new SystemSetting { Setting_Key = "REQUIRE_APPROVAL", Setting_Name = "الاعتماد مطلوب", Setting_Value = "true", Scope = "COMPANY", Company_ID = companyId, Is_Active = true },
                new SystemSetting { Setting_Key = "ALLOW_CREATOR_APPROVAL", Setting_Name = "السماح للمنشئ بالاعتماد", Setting_Value = "false", Scope = "SYSTEM", Is_Active = true },
                new SystemSetting { Setting_Key = "OFFLINE_SAVE_MODE", Setting_Name = "الحفظ دون اتصال", Setting_Value = "DRAFT_ONLY", Scope = "SYSTEM", Is_Active = true });
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return Ok(new { message = "اكتملت التهيئة الأولية بنجاح.", company.Company_ID, branch.Branch_ID, year.Fiscal_Year_ID, Admin_Login_Name = request.Admin_Login_Name.Trim() });
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "تعذر إكمال التهيئة الأولية، ولم تُحفظ بيانات جزئية." });
        }
    }
}

public sealed class InitialSetupRequest
{
    public string Group_Name_AR { get; set; } = string.Empty;
    public string? Group_Name_EN { get; set; }
    public string Company_Name_AR { get; set; } = string.Empty;
    public string? Company_Name_EN { get; set; }
    public string? Company_Prefix { get; set; }
    public string Branch_Name { get; set; } = string.Empty;
    public string? Branch_Name_EN { get; set; }
    public int Fiscal_Year { get; set; }
    public string? Admin_Full_Name { get; set; }
    public string Admin_Login_Name { get; set; } = string.Empty;
    public string Admin_Password { get; set; } = string.Empty;
}
