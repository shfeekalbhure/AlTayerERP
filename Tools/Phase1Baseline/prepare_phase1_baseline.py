#!/usr/bin/env python3
"""تهيئة نموذج EF Core للـBaseline دون الاتصال بأي قاعدة بيانات."""
from __future__ import annotations

import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def write(path: str, content: str) -> None:
    target = ROOT / path
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_text(content.strip() + "\n", encoding="utf-8")


def replace_in(path: str, replacements: list[tuple[str, str]]) -> None:
    target = ROOT / path
    text = target.read_text(encoding="utf-8-sig")
    original = text
    for old, new in replacements:
        text = text.replace(old, new)
    if text != original:
        target.write_text(text, encoding="utf-8")


# توحيد Branch_ID في الكيانات المالية والتدقيق.
replace_in("AlTayerERP.Core/Entities/Accounting/FinancialVoucherHeader.cs", [
    ("public string Branch_ID { get; set; } = string.Empty;", "public int Branch_ID { get; set; }")
])
replace_in("AlTayerERP.Core/Entities/Accounting/JournalEntryHeader.cs", [
    ("public string Branch_ID { get; set; } = string.Empty;", "public int Branch_ID { get; set; }")
])
replace_in("AlTayerERP.Core/Entities/Accounting/AuditLog.cs", [
    ("public string? Branch_ID { get; set; }", "public int? Branch_ID { get; set; }")
])

# إزالة التحويلات النصية الشائعة بعد اعتماد Branch_ID الرقمي.
for target in list((ROOT / "AlTayerERP.API").rglob("*.cs")) + list((ROOT / "AlTayerERP.Desktop").rglob("*.cs")) + list((ROOT / "AlTayerERP.Mobile.Office").rglob("*.cs")):
    text = target.read_text(encoding="utf-8-sig")
    changed = text
    changed = re.sub(r"(Branch_ID\s*=\s*(?:Session|session)\.Branch_ID)\.ToString\(\)", r"\1", changed)
    changed = re.sub(r"(\.Branch_ID\s*==\s*(?:Session|session)\.Branch_ID)\.ToString\(\)", r"\1", changed)
    changed = re.sub(r"((?:Session|session)\.Branch_ID)\.ToString\(\)\s*==\s*([A-Za-z0-9_\.]+\.Branch_ID)", r"\1 == \2", changed)
    if changed != text:
        target.write_text(changed, encoding="utf-8")

write("AlTayerERP.Core/Entities/Phase1ReferenceEntities.cs", r'''
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities;

/// <summary>الدولة المرجعية.</summary>
[Table("countries")]
public sealed class Country
{
    [Key] public int Country_ID { get; set; }
    [Required, MaxLength(10)] public string Country_Code { get; set; } = string.Empty;
    [Required, MaxLength(150)] public string Country_Name_AR { get; set; } = string.Empty;
    [MaxLength(150)] public string? Country_Name_EN { get; set; }
    [MaxLength(2)] public string? ISO2 { get; set; }
    [MaxLength(3)] public string? ISO3 { get; set; }
    [MaxLength(10)] public string? Phone_Code { get; set; }
    [MaxLength(10)] public string? Currency_Code { get; set; }
    [MaxLength(150)] public string? Nationality_Name_AR { get; set; }
    public int Sort_Order { get; set; }
    public bool Is_Active { get; set; } = true;
    [MaxLength(500)] public string? Notes { get; set; }
    public DateTime Created_At { get; set; } = DateTime.UtcNow;
    public DateTime? Updated_At { get; set; }
}

/// <summary>المحافظة التابعة لدولة.</summary>
[Table("governorates")]
public sealed class Governorate
{
    [Key] public int Governorate_ID { get; set; }
    public int Country_ID { get; set; }
    [Required, MaxLength(20)] public string Governorate_Code { get; set; } = string.Empty;
    [Required, MaxLength(150)] public string Governorate_Name_AR { get; set; } = string.Empty;
    [MaxLength(150)] public string? Governorate_Name_EN { get; set; }
    public int Sort_Order { get; set; }
    public bool Is_Active { get; set; } = true;
    [MaxLength(500)] public string? Notes { get; set; }
    public DateTime Created_At { get; set; } = DateTime.UtcNow;
    public DateTime? Updated_At { get; set; }
}

/// <summary>المدينة التابعة لمحافظة ودولة.</summary>
[Table("cities")]
public sealed class City
{
    [Key] public int City_ID { get; set; }
    public int Country_ID { get; set; }
    public int Governorate_ID { get; set; }
    [Required, MaxLength(20)] public string City_Code { get; set; } = string.Empty;
    [Required, MaxLength(150)] public string City_Name_AR { get; set; } = string.Empty;
    [MaxLength(150)] public string? City_Name_EN { get; set; }
    [MaxLength(20)] public string? Postal_Code { get; set; }
    public int Sort_Order { get; set; }
    public bool Is_Active { get; set; } = true;
    [MaxLength(500)] public string? Notes { get; set; }
    public DateTime Created_At { get; set; } = DateTime.UtcNow;
    public DateTime? Updated_At { get; set; }
}

/// <summary>نوع الفرع المرجعي.</summary>
[Table("branch_types")]
public sealed class BranchType
{
    [Key] public int Branch_Type_ID { get; set; }
    [Required, MaxLength(30)] public string Branch_Type_Code { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Branch_Type_Name_AR { get; set; } = string.Empty;
    [MaxLength(100)] public string? Branch_Type_Name_EN { get; set; }
    public int Sort_Order { get; set; }
    public bool Is_Active { get; set; } = true;
    public DateTime Created_At { get; set; } = DateTime.UtcNow;
    public DateTime? Updated_At { get; set; }
}

/// <summary>حالة اعتماد مرجعية.</summary>
[Table("approval_statuses")]
public sealed class ApprovalStatusReference
{
    [Key] public int Approval_Status_ID { get; set; }
    [Required, MaxLength(30)] public string Approval_Status_Code { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Approval_Status_Name_AR { get; set; } = string.Empty;
    [MaxLength(100)] public string? Approval_Status_Name_EN { get; set; }
    public int Sort_Order { get; set; }
    public bool Is_Active { get; set; } = true;
}

/// <summary>نوع مستند يستخدمه نظام الترقيم.</summary>
[Table("numbering_document_types")]
public sealed class NumberingDocumentType
{
    [Key] public int Numbering_Document_Type_ID { get; set; }
    [Required, MaxLength(50)] public string Document_Type_Code { get; set; } = string.Empty;
    [Required, MaxLength(150)] public string Document_Type_Name_AR { get; set; } = string.Empty;
    [MaxLength(150)] public string? Document_Type_Name_EN { get; set; }
    public bool Is_Active { get; set; } = true;
    public int Sort_Order { get; set; }
}
''')

write("AlTayerERP.Infrastructure/Data/AppDbContext.cs", r'''
using AlTayerERP.Core.Entities;
using AlTayerERP.Core.Entities.Accounting;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.Infrastructure.Data;

/// <summary>
/// سياق قاعدة بيانات المرحلة الأولى. المخطط يُدار حصريًا بواسطة EF Core Migrations.
/// </summary>
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TenantGroup> Tenant_Groups => Set<TenantGroup>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<TenantBranch> Tenant_Branches => Set<TenantBranch>();
    public DbSet<FiscalYear> Fiscal_Years => Set<FiscalYear>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Governorate> Governorates => Set<Governorate>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<BranchType> Branch_Types => Set<BranchType>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserPermission> User_Permissions => Set<UserPermission>();
    public DbSet<SystemPermission> System_Permissions => Set<SystemPermission>();
    public DbSet<SystemScreen> SystemScreens => Set<SystemScreen>();
    public DbSet<LoginAttempt> Login_Attempts => Set<LoginAttempt>();
    public DbSet<RefreshToken> Refresh_Tokens => Set<RefreshToken>();
    public DbSet<SystemSetting> System_Settings => Set<SystemSetting>();
    public DbSet<FiscalPeriod> Fiscal_Periods => Set<FiscalPeriod>();
    public DbSet<ExchangeRate> Exchange_Rates => Set<ExchangeRate>();
    public DbSet<NumberingSetting> Numbering_Settings => Set<NumberingSetting>();
    public DbSet<NumberingCounter> Numbering_Counters => Set<NumberingCounter>();
    public DbSet<NumberingDocumentType> Numbering_Document_Types => Set<NumberingDocumentType>();
    public DbSet<FinancialPolicy> Financial_Policies => Set<FinancialPolicy>();
    public DbSet<FinancialPolicyMovement> Financial_Policy_Movements => Set<FinancialPolicyMovement>();
    public DbSet<ApprovalRequest> Approval_Requests => Set<ApprovalRequest>();
    public DbSet<ApprovalStatusReference> Approval_Statuses => Set<ApprovalStatusReference>();
    public DbSet<AccountCodeSetting> Account_Code_Settings => Set<AccountCodeSetting>();
    public DbSet<AccountCategory> Account_Categories => Set<AccountCategory>();
    public DbSet<ChartOfAccount> Chart_Of_Accounts => Set<ChartOfAccount>();
    public DbSet<CostCenter> Cost_Centers => Set<CostCenter>();
    public DbSet<CashBox> Cash_Boxes => Set<CashBox>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<BankAccount> Bank_Accounts => Set<BankAccount>();
    public DbSet<FinancialVoucherHeader> Financial_Voucher_Headers => Set<FinancialVoucherHeader>();
    public DbSet<FinancialVoucherDetail> Financial_Voucher_Details => Set<FinancialVoucherDetail>();
    public DbSet<JournalEntryHeader> Journal_Entry_Headers => Set<JournalEntryHeader>();
    public DbSet<JournalEntryDetail> Journal_Entry_Details => Set<JournalEntryDetail>();
    public DbSet<DocumentAllocation> Document_Allocations => Set<DocumentAllocation>();
    public DbSet<DocumentLink> Document_Links => Set<DocumentLink>();
    public DbSet<AuditLog> Audit_Logs => Set<AuditLog>();
    public DbSet<VoucherActionLog> Voucher_Action_Logs => Set<VoucherActionLog>();
    public DbSet<VoucherType> Voucher_Types => Set<VoucherType>();
    public DbSet<VoucherStatus> Voucher_Statuses => Set<VoucherStatus>();
    public DbSet<PaymentMethod> Payment_Methods => Set<PaymentMethod>();
    public DbSet<Party> Parties => Set<Party>();
    public DbSet<PaymentRequest> Payment_Requests => Set<PaymentRequest>();
    public DbSet<PaymentRequestLine> Payment_Request_Lines => Set<PaymentRequestLine>();
    public DbSet<PaymentRequestAttachment> Payment_Request_Attachments => Set<PaymentRequestAttachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        Phase1ModelConfiguration.Configure(modelBuilder);
    }
}
''')

write("AlTayerERP.Infrastructure/Data/Phase1ModelConfiguration.cs", r'''
using System.Text;
using AlTayerERP.Core.Entities;
using AlTayerERP.Core.Entities.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AlTayerERP.Infrastructure.Data;

/// <summary>الإعداد المركزي المعتمد لمخطط المرحلة الأولى.</summary>
internal static class Phase1ModelConfiguration
{
    internal static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.HasCharSet("utf8mb4");
        modelBuilder.UseCollation("utf8mb4_unicode_ci");

        ConfigureInstitutional(modelBuilder);
        ConfigureSecurity(modelBuilder);
        ConfigureSettings(modelBuilder);
        ConfigureAccountingMasterData(modelBuilder);
        ConfigureFinancialEngine(modelBuilder);
        ConfigureReferenceSeeds(modelBuilder);
        ApplySafeDefaultsAndNaming(modelBuilder);
    }

    private static void ConfigureInstitutional(ModelBuilder m)
    {
        m.Entity<TenantGroup>(e =>
        {
            e.ToTable("tenant_groups"); e.HasKey(x => x.Group_ID);
            e.Property(x => x.Group_ID).HasMaxLength(50);
            e.Property(x => x.Group_Code).HasMaxLength(30).IsRequired();
            e.Property(x => x.Group_Name_AR).HasMaxLength(200).IsRequired();
            e.Property(x => x.Group_Name_EN).HasMaxLength(200);
            e.HasIndex(x => x.Group_Code).IsUnique();
        });
        m.Entity<Company>(e =>
        {
            e.ToTable("companies"); e.HasKey(x => x.Company_ID);
            e.Property(x => x.Company_ID).HasMaxLength(50);
            e.Property(x => x.Group_ID).HasMaxLength(50).IsRequired();
            e.HasIndex(x => new { x.Group_ID, x.Is_Active });
            e.HasOne<TenantGroup>().WithMany().HasForeignKey(x => x.Group_ID).OnDelete(DeleteBehavior.Restrict);
        });
        m.Entity<TenantBranch>(e =>
        {
            e.ToTable("tenant_branches"); e.HasKey(x => x.Branch_ID);
            e.Property(x => x.Company_ID).HasMaxLength(50).IsRequired();
            e.Property(x => x.Branch_Code).HasMaxLength(30).IsRequired();
            e.HasIndex(x => new { x.Company_ID, x.Branch_Code }).IsUnique();
            e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ParentBranch).WithMany().HasForeignKey(x => x.Parent_Branch_ID).OnDelete(DeleteBehavior.Restrict);
        });
        m.Entity<FiscalYear>(e =>
        {
            e.ToTable("fiscal_years"); e.HasKey(x => x.Fiscal_Year_ID);
            e.HasIndex(x => new { x.Company_ID, x.Year_Name }).IsUnique();
            e.HasCheckConstraint("ck_fiscal_year_dates", "start_date <= end_date");
            e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
        });
        m.Entity<Country>(e =>
        {
            e.ToTable("countries"); e.HasKey(x => x.Country_ID);
            e.HasIndex(x => x.Country_Code).IsUnique();
            e.HasIndex(x => x.ISO2).IsUnique();
            e.HasIndex(x => x.ISO3).IsUnique();
            e.HasCheckConstraint("ck_country_iso2", "iso2 IS NULL OR CHAR_LENGTH(iso2) = 2");
            e.HasCheckConstraint("ck_country_iso3", "iso3 IS NULL OR CHAR_LENGTH(iso3) = 3");
        });
        m.Entity<Governorate>(e =>
        {
            e.ToTable("governorates"); e.HasKey(x => x.Governorate_ID);
            e.HasIndex(x => new { x.Country_ID, x.Governorate_Code }).IsUnique();
            e.HasOne<Country>().WithMany().HasForeignKey(x => x.Country_ID).OnDelete(DeleteBehavior.Restrict);
        });
        m.Entity<City>(e =>
        {
            e.ToTable("cities"); e.HasKey(x => x.City_ID);
            e.HasIndex(x => new { x.Governorate_ID, x.City_Code }).IsUnique();
            e.HasOne<Country>().WithMany().HasForeignKey(x => x.Country_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Governorate>().WithMany().HasForeignKey(x => x.Governorate_ID).OnDelete(DeleteBehavior.Restrict);
        });
        m.Entity<BranchType>(e => { e.ToTable("branch_types"); e.HasKey(x => x.Branch_Type_ID); e.HasIndex(x => x.Branch_Type_Code).IsUnique(); });
    }

    private static void ConfigureSecurity(ModelBuilder m)
    {
        m.Entity<Role>(e => { e.ToTable("roles"); e.HasKey(x => x.Role_ID); e.HasIndex(x => x.Role_Code).IsUnique(); });
        m.Entity<User>(e =>
        {
            e.ToTable("users"); e.HasKey(x => x.User_ID);
            e.HasIndex(x => x.Login_Name).IsUnique();
            e.HasIndex(x => x.User_Code).IsUnique();
            e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Role>().WithMany().HasForeignKey(x => x.Role_ID).OnDelete(DeleteBehavior.Restrict);
        });
        m.Entity<RolePermission>(e =>
        {
            e.ToTable("role_permissions"); e.HasKey(x => x.Permission_ID);
            e.HasIndex(x => new { x.Role_ID, x.Screen_ID }).IsUnique();
            e.HasOne<Role>().WithMany().HasForeignKey(x => x.Role_ID).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<SystemScreen>().WithMany().HasForeignKey(x => x.Screen_ID).OnDelete(DeleteBehavior.Restrict);
        });
        m.Entity<UserPermission>(e => { e.ToTable("user_permissions"); e.HasKey(x => x.Permission_ID); e.HasIndex(x => new { x.User_ID, x.Permission_Category, x.Permission_Name }).IsUnique(); e.HasOne<User>().WithMany().HasForeignKey(x => x.User_ID).OnDelete(DeleteBehavior.Cascade); });
        m.Entity<SystemPermission>(e => { e.ToTable("system_permissions"); e.HasKey(x => x.Permission_ID); e.HasIndex(x => x.Permission_Code).IsUnique(); });
        m.Entity<SystemScreen>(e => { e.ToTable("system_screens"); e.HasKey(x => x.Screen_ID); e.HasIndex(x => x.Screen_Code).IsUnique(); });
        m.Entity<LoginAttempt>(e => { e.ToTable("login_attempts"); e.HasKey(x => x.Login_Attempt_ID); e.HasIndex(x => new { x.Login_Name, x.Attempted_At }); e.HasIndex(x => new { x.User_ID, x.Attempted_At }); e.HasOne<User>().WithMany().HasForeignKey(x => x.User_ID).OnDelete(DeleteBehavior.SetNull); });
        m.Entity<RefreshToken>(e => { e.ToTable("refresh_tokens"); e.HasKey(x => x.Refresh_Token_ID); e.HasIndex(x => x.Token_Hash).IsUnique(); e.HasIndex(x => new { x.User_ID, x.Expires_At }); e.HasOne<User>().WithMany().HasForeignKey(x => x.User_ID).OnDelete(DeleteBehavior.Cascade); });
    }

    private static void ConfigureSettings(ModelBuilder m)
    {
        m.Entity<SystemSetting>(e => { e.ToTable("system_settings"); e.HasKey(x => x.Setting_ID); e.HasIndex(x => new { x.Setting_Key, x.Scope, x.Company_ID, x.Branch_ID, x.Fiscal_Year_ID }).IsUnique(); e.Property(x => x.Setting_Value).HasColumnType("text"); e.Property(x => x.Description).HasColumnType("text"); });
        m.Entity<FiscalPeriod>(e => { e.ToTable("fiscal_periods"); e.HasKey(x => x.Fiscal_Period_ID); e.HasIndex(x => new { x.Company_ID, x.Branch_ID, x.Fiscal_Year_ID, x.Period_Code }).IsUnique(); e.HasCheckConstraint("ck_fiscal_period_dates", "start_date <= end_date"); });
        m.Entity<ExchangeRate>(e => { e.ToTable("exchange_rates"); e.HasKey(x => x.Exchange_Rate_ID); e.Property(x => x.Exchange_Rate_Value).HasPrecision(18, 6); e.Property(x => x.Min_Rate).HasPrecision(18, 6); e.Property(x => x.Max_Rate).HasPrecision(18, 6); e.HasIndex(x => new { x.Company_ID, x.Currency_Code, x.Rate_Date }).IsUnique(); e.HasCheckConstraint("ck_exchange_rate_positive", "exchange_rate > 0"); });
        m.Entity<NumberingSetting>(e => { e.ToTable("numbering_settings"); e.HasKey(x => x.Numbering_ID); e.HasIndex(x => x.Document_Type).IsUnique(); });
        m.Entity<NumberingCounter>(e => { e.ToTable("numbering_counters"); e.HasKey(x => x.Counter_ID); e.HasIndex(x => new { x.Document_Type, x.Company_ID, x.Branch_ID, x.Year_Value }).IsUnique(); });
        m.Entity<NumberingDocumentType>(e => { e.ToTable("numbering_document_types"); e.HasKey(x => x.Numbering_Document_Type_ID); e.HasIndex(x => x.Document_Type_Code).IsUnique(); });
        m.Entity<FinancialPolicy>(e => { e.ToTable("financial_limits"); e.HasKey(x => x.Limit_ID); e.Property(x => x.Limit_Amount).HasPrecision(19, 4); e.Property(x => x.Used_Amount).HasPrecision(19, 4); });
        m.Entity<FinancialPolicyMovement>(e => { e.ToTable("financial_limit_movements"); e.HasKey(x => x.Movement_ID); e.Property(x => x.Amount).HasPrecision(19, 4); e.Property(x => x.Balance_After).HasPrecision(19, 4); e.HasOne<FinancialPolicy>().WithMany().HasForeignKey(x => x.Limit_ID).OnDelete(DeleteBehavior.Restrict); });
        m.Entity<ApprovalRequest>(e => { e.ToTable("approval_requests"); e.HasKey(x => x.Approval_ID); e.Property(x => x.Amount).HasPrecision(19, 4); e.HasCheckConstraint("ck_approval_request_status", "status IN ('Pending','UnderReview','Approved','Rejected','Returned','Canceled')"); });
        m.Entity<ApprovalStatusReference>(e => { e.ToTable("approval_statuses"); e.HasKey(x => x.Approval_Status_ID); e.HasIndex(x => x.Approval_Status_Code).IsUnique(); });
    }

    private static void ConfigureAccountingMasterData(ModelBuilder m)
    {
        m.Entity<AccountCodeSetting>(e => { e.ToTable("account_code_settings"); e.HasKey(x => x.Setting_ID); e.HasIndex(x => new { x.Company_ID, x.Level_No }).IsUnique(); });
        m.Entity<AccountCategory>(e => { e.ToTable("account_categories"); e.HasKey(x => x.Category_ID); e.HasIndex(x => new { x.Company_ID, x.Category_Code }).IsUnique(); });
        m.Entity<ChartOfAccount>(e => { e.ToTable("chart_of_accounts"); e.HasKey(x => x.Account_ID); e.Property(x => x.Account_ID).HasMaxLength(50); e.HasIndex(x => new { x.Company_ID, x.Account_Code }).IsUnique(); e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict); e.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Parent_Account_ID).OnDelete(DeleteBehavior.Restrict); });
        m.Entity<CostCenter>(e => { e.ToTable("cost_centers"); e.HasKey(x => x.Cost_Center_ID); e.Property(x => x.Cost_Center_ID).HasMaxLength(50); e.HasIndex(x => new { x.Company_ID, x.Center_Code }).IsUnique(); e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict); e.HasOne<CostCenter>().WithMany().HasForeignKey(x => x.Parent_Cost_Center_ID).OnDelete(DeleteBehavior.Restrict); });
        m.Entity<Currency>(e => { e.ToTable("currencies"); e.HasKey(x => x.Currency_ID); e.Property(x => x.Exchange_Rate).HasPrecision(18, 6); e.Property(x => x.Min_Exchange_Rate).HasPrecision(18, 6); e.Property(x => x.Max_Exchange_Rate).HasPrecision(18, 6); e.HasIndex(x => new { x.Company_ID, x.Currency_Code }).IsUnique(); e.HasCheckConstraint("ck_currency_rate", "exchange_rate > 0"); });
        m.Entity<CashBox>(e => { e.ToTable("cash_boxes"); e.HasKey(x => x.Cash_Box_ID); e.Property(x => x.Cash_Box_ID).HasMaxLength(50); e.Property(x => x.Opening_Balance).HasPrecision(19, 4); e.Property(x => x.Min_Limit).HasPrecision(19, 4); e.Property(x => x.Max_Limit).HasPrecision(19, 4); e.HasIndex(x => new { x.Company_ID, x.Branch_ID, x.CashBox_Code }).IsUnique(); e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict); e.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict); e.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Account_ID).OnDelete(DeleteBehavior.Restrict); });
        m.Entity<BankAccount>(e => { e.ToTable("bank_accounts"); e.HasKey(x => x.Bank_Account_ID); e.HasIndex(x => new { x.Company_ID, x.Account_No }).IsUnique(); e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict); });
        m.Entity<Party>(e => { e.ToTable("parties"); e.HasKey(x => x.Party_ID); e.Property(x => x.Credit_Limit).HasPrecision(19, 4); e.HasIndex(x => new { x.Company_ID, x.Party_Code }).IsUnique(); e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict); e.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Account_ID).OnDelete(DeleteBehavior.SetNull); });
    }

    private static void ConfigureFinancialEngine(ModelBuilder m)
    {
        m.Entity<VoucherType>(e => { e.ToTable("voucher_types"); e.HasKey(x => x.Voucher_Type_ID); e.HasIndex(x => x.Voucher_Type_Code).IsUnique(); });
        m.Entity<VoucherStatus>(e => { e.ToTable("voucher_statuses"); e.HasKey(x => x.Voucher_Status_ID); e.HasIndex(x => x.Voucher_Status_Code).IsUnique(); });
        m.Entity<PaymentMethod>(e => { e.ToTable("payment_methods"); e.HasKey(x => x.Payment_Method_ID); e.HasIndex(x => x.Payment_Method_Code).IsUnique(); });
        m.Entity<FinancialVoucherHeader>(e =>
        {
            e.ToTable("financial_voucher_headers"); e.HasKey(x => x.Voucher_ID);
            e.HasIndex(x => new { x.Branch_ID, x.Fiscal_Year_ID, x.Voucher_Type_ID, x.Voucher_No }).IsUnique();
            e.Property(x => x.Exchange_Rate).HasPrecision(18, 6); e.Property(x => x.Amount).HasPrecision(19, 4); e.Property(x => x.Foreign_Total).HasPrecision(19, 4); e.Property(x => x.Local_Total).HasPrecision(19, 4);
            e.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<FiscalYear>().WithMany().HasForeignKey(x => x.Fiscal_Year_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<VoucherType>().WithMany().HasForeignKey(x => x.Voucher_Type_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<VoucherStatus>().WithMany().HasForeignKey(x => x.Voucher_Status_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Currency>().WithMany().HasForeignKey(x => x.Currency_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Party>().WithMany().HasForeignKey(x => x.Party_ID).OnDelete(DeleteBehavior.SetNull);
            e.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Cash_Account_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Details).WithOne(x => x.Voucher).HasForeignKey(x => x.Voucher_ID).OnDelete(DeleteBehavior.Cascade);
        });
        m.Entity<FinancialVoucherDetail>(e => { e.ToTable("financial_voucher_details"); e.HasKey(x => x.Voucher_Detail_ID); e.HasIndex(x => new { x.Voucher_ID, x.Line_No }).IsUnique(); e.Property(x => x.Exchange_Rate).HasPrecision(18, 6); e.Property(x => x.Foreign_Amount).HasPrecision(19, 4); e.Property(x => x.Local_Amount).HasPrecision(19, 4); e.Property(x => x.Debit_Amount).HasPrecision(19, 4); e.Property(x => x.Credit_Amount).HasPrecision(19, 4); e.HasCheckConstraint("ck_voucher_line_amounts", "debit_amount >= 0 AND credit_amount >= 0 AND NOT (debit_amount > 0 AND credit_amount > 0)"); e.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Account_ID).OnDelete(DeleteBehavior.Restrict); e.HasOne<CostCenter>().WithMany().HasForeignKey(x => x.Cost_Center_ID).OnDelete(DeleteBehavior.Restrict); e.HasOne<Currency>().WithMany().HasForeignKey(x => x.Currency_ID).OnDelete(DeleteBehavior.Restrict); });
        m.Entity<JournalEntryHeader>(e => { e.ToTable("journal_entry_headers"); e.HasKey(x => x.Journal_Entry_ID); e.HasIndex(x => x.Entry_No).IsUnique(); e.Property(x => x.Total_Debit).HasPrecision(19, 4); e.Property(x => x.Total_Credit).HasPrecision(19, 4); e.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict); e.HasOne<FiscalYear>().WithMany().HasForeignKey(x => x.Fiscal_Year_ID).OnDelete(DeleteBehavior.Restrict); e.HasOne<FinancialVoucherHeader>().WithMany().HasForeignKey(x => x.Source_Voucher_ID).OnDelete(DeleteBehavior.Restrict); e.HasMany(x => x.Details).WithOne(x => x.JournalEntry).HasForeignKey(x => x.Journal_Entry_ID).OnDelete(DeleteBehavior.Cascade); });
        m.Entity<JournalEntryDetail>(e => { e.ToTable("journal_entry_details"); e.HasKey(x => x.Journal_Entry_Detail_ID); e.HasIndex(x => new { x.Journal_Entry_ID, x.Line_No }).IsUnique(); e.Property(x => x.Exchange_Rate).HasPrecision(18, 6); e.Property(x => x.Foreign_Amount).HasPrecision(19, 4); e.Property(x => x.Local_Amount).HasPrecision(19, 4); e.Property(x => x.Debit_Amount).HasPrecision(19, 4); e.Property(x => x.Credit_Amount).HasPrecision(19, 4); e.HasCheckConstraint("ck_journal_line_amounts", "debit_amount >= 0 AND credit_amount >= 0 AND NOT (debit_amount > 0 AND credit_amount > 0)"); e.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Account_ID).OnDelete(DeleteBehavior.Restrict); e.HasOne<CostCenter>().WithMany().HasForeignKey(x => x.Cost_Center_ID).OnDelete(DeleteBehavior.Restrict); e.HasOne<Currency>().WithMany().HasForeignKey(x => x.Currency_ID).OnDelete(DeleteBehavior.Restrict); });
        m.Entity<DocumentAllocation>(e => { e.ToTable("document_allocations"); e.HasKey(x => x.Allocation_ID); e.Property(x => x.Exchange_Rate).HasPrecision(18, 6); e.Property(x => x.Document_Total).HasPrecision(19, 4); e.Property(x => x.Collected_Before).HasPrecision(19, 4); e.Property(x => x.Collected_Now).HasPrecision(19, 4); e.Property(x => x.Remaining_Balance).HasPrecision(19, 4); e.HasOne(x => x.Voucher).WithMany(x => x.DocumentAllocations).HasForeignKey(x => x.Voucher_ID).OnDelete(DeleteBehavior.Cascade); });
        m.Entity<DocumentLink>(e => { e.ToTable("document_links"); e.HasKey(x => x.Document_Link_ID); });
        m.Entity<AuditLog>(e => { e.ToTable("audit_logs"); e.HasKey(x => x.Audit_ID); e.HasIndex(x => new { x.Table_Name, x.Record_ID, x.Action_At }); e.HasCheckConstraint("ck_audit_action_upper", "action_type = UPPER(action_type)"); });
        m.Entity<VoucherActionLog>(e => { e.ToTable("voucher_action_logs"); e.HasKey(x => x.Voucher_Action_ID); e.HasOne(x => x.Voucher).WithMany(x => x.VoucherActionLogs).HasForeignKey(x => x.Voucher_ID).OnDelete(DeleteBehavior.Restrict); });
        m.Entity<PaymentRequest>(e => { e.ToTable("payment_requests"); e.HasKey(x => x.Payment_Request_ID); e.HasIndex(x => new { x.Company_ID, x.Branch_ID, x.Fiscal_Year_ID, x.Request_No }).IsUnique(); e.Property(x => x.Approved_Local_Total).HasPrecision(19, 4); e.HasCheckConstraint("ck_payment_request_status", "status IN ('DRAFT','UNDER_REVIEW','APPROVED','REJECTED','RETURNED','CANCELLED','VOUCHER_CREATED')"); e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict); e.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict); e.HasOne<FiscalYear>().WithMany().HasForeignKey(x => x.Fiscal_Year_ID).OnDelete(DeleteBehavior.Restrict); e.HasOne<Party>().WithMany().HasForeignKey(x => x.Party_ID).OnDelete(DeleteBehavior.SetNull); e.HasOne<PaymentMethod>().WithMany().HasForeignKey(x => x.Payment_Method_ID).OnDelete(DeleteBehavior.SetNull); e.HasOne<FinancialVoucherHeader>().WithMany().HasForeignKey(x => x.Payment_Voucher_ID).OnDelete(DeleteBehavior.Restrict); e.HasMany(x => x.Details).WithOne(x => x.PaymentRequest).HasForeignKey(x => x.Payment_Request_ID).OnDelete(DeleteBehavior.Cascade); });
        m.Entity<PaymentRequestLine>(e => { e.ToTable("payment_request_lines"); e.HasKey(x => x.Payment_Request_Line_ID); e.HasIndex(x => new { x.Payment_Request_ID, x.Line_No }).IsUnique(); e.Property(x => x.Exchange_Rate).HasPrecision(19, 8); e.Property(x => x.Foreign_Amount).HasPrecision(19, 4); e.Property(x => x.Local_Amount).HasPrecision(19, 4); e.HasCheckConstraint("ck_payment_request_line_rate", "exchange_rate > 0"); e.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Account_ID).OnDelete(DeleteBehavior.Restrict); e.HasOne<CostCenter>().WithMany().HasForeignKey(x => x.Cost_Center_ID).OnDelete(DeleteBehavior.Restrict); e.HasOne<Currency>().WithMany().HasForeignKey(x => x.Currency_ID).OnDelete(DeleteBehavior.Restrict); });
        m.Entity<PaymentRequestAttachment>(e => { e.ToTable("payment_request_attachments"); e.HasKey(x => x.Payment_Request_Attachment_ID); e.HasIndex(x => new { x.Payment_Request_ID, x.Is_Active }); e.HasOne<PaymentRequest>().WithMany().HasForeignKey(x => x.Payment_Request_ID).OnDelete(DeleteBehavior.Cascade); });
    }

    private static void ConfigureReferenceSeeds(ModelBuilder m)
    {
        m.Entity<BranchType>().HasData(
            new BranchType { Branch_Type_ID = 1, Branch_Type_Code = "MAIN", Branch_Type_Name_AR = "رئيسي", Branch_Type_Name_EN = "Main", Sort_Order = 10, Is_Active = true, Created_At = DateTime.UnixEpoch },
            new BranchType { Branch_Type_ID = 2, Branch_Type_Code = "OPERATING", Branch_Type_Name_AR = "تشغيلي", Branch_Type_Name_EN = "Operating", Sort_Order = 20, Is_Active = true, Created_At = DateTime.UnixEpoch },
            new BranchType { Branch_Type_ID = 3, Branch_Type_Code = "DISTRIBUTION", Branch_Type_Name_AR = "نقطة توزيع", Branch_Type_Name_EN = "Distribution Point", Sort_Order = 30, Is_Active = true, Created_At = DateTime.UnixEpoch },
            new BranchType { Branch_Type_ID = 4, Branch_Type_Code = "WAREHOUSE", Branch_Type_Name_AR = "مستودع", Branch_Type_Name_EN = "Warehouse", Sort_Order = 40, Is_Active = true, Created_At = DateTime.UnixEpoch });
        m.Entity<VoucherType>().HasData(
            new VoucherType { Voucher_Type_ID = 1, Voucher_Type_Code = "RECEIPT", Voucher_Type_Name_AR = "سند قبض", Voucher_Type_Name_EN = "Receipt Voucher", Is_Active = true, Sort_Order = 1 },
            new VoucherType { Voucher_Type_ID = 2, Voucher_Type_Code = "PAYMENT", Voucher_Type_Name_AR = "سند صرف", Voucher_Type_Name_EN = "Payment Voucher", Is_Active = true, Sort_Order = 2 },
            new VoucherType { Voucher_Type_ID = 3, Voucher_Type_Code = "JOURNAL", Voucher_Type_Name_AR = "قيد يومية", Voucher_Type_Name_EN = "Journal Voucher", Is_Active = true, Sort_Order = 3 },
            new VoucherType { Voucher_Type_ID = 4, Voucher_Type_Code = "ADJUSTMENT", Voucher_Type_Name_AR = "قيد تسوية", Voucher_Type_Name_EN = "Adjustment Voucher", Is_Active = true, Sort_Order = 4 },
            new VoucherType { Voucher_Type_ID = 5, Voucher_Type_Code = "OPENING", Voucher_Type_Name_AR = "قيد افتتاحي", Voucher_Type_Name_EN = "Opening Voucher", Is_Active = true, Sort_Order = 5 });
        m.Entity<VoucherStatus>().HasData(
            new VoucherStatus { Voucher_Status_ID = 1, Voucher_Status_Code = "DRAFT", Voucher_Status_Name_AR = "مسودة", Is_Active = true, Sort_Order = 1 },
            new VoucherStatus { Voucher_Status_ID = 2, Voucher_Status_Code = "PENDING", Voucher_Status_Name_AR = "معلق", Is_Active = true, Sort_Order = 2 },
            new VoucherStatus { Voucher_Status_ID = 3, Voucher_Status_Code = "REVIEWED", Voucher_Status_Name_AR = "تمت المراجعة", Is_Active = true, Sort_Order = 3 },
            new VoucherStatus { Voucher_Status_ID = 4, Voucher_Status_Code = "RETURNED", Voucher_Status_Name_AR = "معاد للتصحيح", Is_Active = true, Sort_Order = 4 },
            new VoucherStatus { Voucher_Status_ID = 5, Voucher_Status_Code = "APPROVED", Voucher_Status_Name_AR = "معتمد", Is_Active = true, Sort_Order = 5 },
            new VoucherStatus { Voucher_Status_ID = 6, Voucher_Status_Code = "POSTED", Voucher_Status_Name_AR = "مرحل", Is_Active = true, Sort_Order = 6 },
            new VoucherStatus { Voucher_Status_ID = 7, Voucher_Status_Code = "CANCELLED", Voucher_Status_Name_AR = "ملغي", Is_Active = true, Sort_Order = 7 },
            new VoucherStatus { Voucher_Status_ID = 8, Voucher_Status_Code = "REVERSED", Voucher_Status_Name_AR = "معكوس", Is_Active = true, Sort_Order = 8 });
        m.Entity<PaymentMethod>().HasData(
            new PaymentMethod { Payment_Method_ID = 1, Payment_Method_Code = "CASH", Payment_Method_Name_AR = "نقدي", Is_Cash = true, Is_Active = true, Sort_Order = 1 },
            new PaymentMethod { Payment_Method_ID = 2, Payment_Method_Code = "CHEQUE", Payment_Method_Name_AR = "شيك", Is_Bank = true, Requires_Reference = true, Requires_Reference_Date = true, Is_Active = true, Sort_Order = 2 },
            new PaymentMethod { Payment_Method_ID = 3, Payment_Method_Code = "BANK_TRANSFER", Payment_Method_Name_AR = "تحويل بنكي", Is_Bank = true, Requires_Reference = true, Requires_Reference_Date = true, Is_Active = true, Sort_Order = 3 });
        m.Entity<ApprovalStatusReference>().HasData(
            new ApprovalStatusReference { Approval_Status_ID = 1, Approval_Status_Code = "PENDING", Approval_Status_Name_AR = "بانتظار الاعتماد", Sort_Order = 1, Is_Active = true },
            new ApprovalStatusReference { Approval_Status_ID = 2, Approval_Status_Code = "UNDER_REVIEW", Approval_Status_Name_AR = "قيد المراجعة", Sort_Order = 2, Is_Active = true },
            new ApprovalStatusReference { Approval_Status_ID = 3, Approval_Status_Code = "APPROVED", Approval_Status_Name_AR = "معتمد", Sort_Order = 3, Is_Active = true },
            new ApprovalStatusReference { Approval_Status_ID = 4, Approval_Status_Code = "REJECTED", Approval_Status_Name_AR = "مرفوض", Sort_Order = 4, Is_Active = true },
            new ApprovalStatusReference { Approval_Status_ID = 5, Approval_Status_Code = "RETURNED", Approval_Status_Name_AR = "معاد للتعديل", Sort_Order = 5, Is_Active = true },
            new ApprovalStatusReference { Approval_Status_ID = 6, Approval_Status_Code = "CANCELLED", Approval_Status_Name_AR = "ملغي", Sort_Order = 6, Is_Active = true });
        m.Entity<NumberingDocumentType>().HasData(
            new NumberingDocumentType { Numbering_Document_Type_ID = 1, Document_Type_Code = "BRANCH", Document_Type_Name_AR = "الفروع", Is_Active = true, Sort_Order = 10 },
            new NumberingDocumentType { Numbering_Document_Type_ID = 2, Document_Type_Code = "RECEIPT_VOUCHER", Document_Type_Name_AR = "سند القبض", Is_Active = true, Sort_Order = 20 },
            new NumberingDocumentType { Numbering_Document_Type_ID = 3, Document_Type_Code = "PAYMENT_VOUCHER", Document_Type_Name_AR = "سند الصرف", Is_Active = true, Sort_Order = 30 },
            new NumberingDocumentType { Numbering_Document_Type_ID = 4, Document_Type_Code = "JOURNAL_ENTRY", Document_Type_Name_AR = "القيد المحاسبي", Is_Active = true, Sort_Order = 40 },
            new NumberingDocumentType { Numbering_Document_Type_ID = 5, Document_Type_Code = "PAYMENT_REQUEST", Document_Type_Name_AR = "طلب الصرف", Is_Active = true, Sort_Order = 50 });
        m.Entity<SystemPermission>().HasData(
            Permission(1, "VIEW", "عرض", "DATA", 10), Permission(2, "ADD", "إضافة", "DATA", 20), Permission(3, "EDIT", "تعديل", "DATA", 30), Permission(4, "DELETE", "إيقاف", "DATA", 40), Permission(5, "PRINT", "طباعة", "REPORT", 50), Permission(6, "EXPORT", "تصدير", "REPORT", 60), Permission(7, "IMPORT", "استيراد", "DATA", 70), Permission(8, "APPROVE", "اعتماد", "EXTRA", 80), Permission(9, "UNAPPROVE", "إلغاء اعتماد", "EXTRA", 90));

        var screens = new (string Code, string Name, string Module, int Sort)[]
        {
            ("TenantGroups","المجموعات التجارية","الإدارة العامة",5),("Companies","الشركات","الإدارة العامة",10),("Branches","الفروع","الإدارة العامة",20),("Countries","الدول","الإدارة العامة",25),("Governorates","المحافظات","الإدارة العامة",26),("Cities","المدن","الإدارة العامة",27),("FiscalYears","السنوات المالية","الإدارة العامة",30),("Users","المستخدمون","الإدارة العامة",40),("Roles","الأدوار","الإدارة العامة",50),("RolePermissions","صلاحيات الأدوار","الإدارة العامة",60),("AuditLogs","سجل التدقيق والرقابة","الإدارة العامة",70),("Sessions","الجلسات النشطة","الإدارة العامة",80),("GeneralSettings","الإعدادات العامة والمالية","التهيئة والإعدادات",70),("SystemScreens","كتالوج شاشات النظام","التهيئة والإعدادات",80),("NumberingSettings","إعدادات الترقيم","التهيئة والإعدادات",90),("FiscalPeriods","الفترات المالية","التهيئة والإعدادات",100),("ExchangeRates","أسعار الصرف","التهيئة والإعدادات",110),("PaymentMethods","طرق السداد","التهيئة والإعدادات",120),("VoucherTypes","أنواع السندات","التهيئة والإعدادات",130),("VoucherStatuses","حالات السندات","التهيئة والإعدادات",140),("ApprovalPolicies","سياسات الاعتماد والسقوف","التهيئة والإعدادات",150),("ChartOfAccounts","الدليل المحاسبي","الحسابات",160),("Currencies","العملات","الحسابات",170),("CostCenters","مراكز التكلفة","الحسابات",180),("CashBoxes","الصناديق","الحسابات",190),("Banks","البنوك والحسابات البنكية","الحسابات",200),("Parties","الأطراف المالية","الحسابات",210),("ReceiptVoucher","سند القبض","الحسابات",220),("PaymentVoucher","سند الصرف","الحسابات",230),("PaymentRequest","طلب الصرف","الحسابات",235),("JournalVoucher","القيد اليومي","الحسابات",240),("DocumentSearch","البحث عن المستندات","الحسابات",250),("ApprovalRequests","طلبات الاعتماد","الحسابات",260),("FinancialLimits","السقوف المالية وحركات الاستخدام","الحسابات",270),("TrialBalance","ميزان المراجعة","التقارير المالية",280),("GeneralLedger","الأستاذ العام","التقارير المالية",290)
        };
        m.Entity<SystemScreen>().HasData(screens.Select((s, i) => new SystemScreen { Screen_ID = i + 1, Screen_Code = s.Code, Screen_Name = s.Name, Module_Name = s.Module, Sort_Order = s.Sort, Is_Active = true, Created_At = DateTime.UnixEpoch }));
    }

    private static SystemPermission Permission(int id, string code, string name, string type, int sort) => new() { Permission_ID = id, Permission_Code = code, Permission_Name = name, Permission_Type = type, Is_Active = true, Sort_Order = sort, Created_At = DateTime.UnixEpoch };

    private static void ApplySafeDefaultsAndNaming(ModelBuilder m)
    {
        foreach (var entity in m.Model.GetEntityTypes())
        {
            entity.SetTableName(ToSnakeCase(entity.GetTableName() ?? entity.ClrType.Name));
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
                if (property.ClrType == typeof(string) && property.GetMaxLength() is null && property.GetColumnType() != "json" && property.GetColumnType() != "text")
                    property.SetMaxLength(InferLength(property.Name));
                if (property.ClrType == typeof(bool) && property.Name.StartsWith("Is_", StringComparison.Ordinal))
                    property.SetDefaultValue(true);
                if (property.ClrType == typeof(DateTime) && property.Name == "Created_At")
                    property.SetDefaultValueSql("CURRENT_TIMESTAMP(6)");
            }
            foreach (var index in entity.GetIndexes())
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName() ?? $"ix_{entity.GetTableName()}_{string.Join('_', index.Properties.Select(p => p.Name))}"));
            foreach (var key in entity.GetKeys())
                key.SetName(ToSnakeCase(key.GetName() ?? $"pk_{entity.GetTableName()}"));
            foreach (var fk in entity.GetForeignKeys())
                fk.SetConstraintName(ToSnakeCase(fk.GetConstraintName() ?? $"fk_{entity.GetTableName()}_{fk.PrincipalEntityType.GetTableName()}"));
        }
    }

    private static int InferLength(string name)
    {
        if (name.Contains("Password", StringComparison.OrdinalIgnoreCase)) return 255;
        if (name.Contains("Hash", StringComparison.OrdinalIgnoreCase)) return 128;
        if (name.Contains("User_Agent", StringComparison.OrdinalIgnoreCase)) return 512;
        if (name.EndsWith("_ID", StringComparison.OrdinalIgnoreCase) || name.Contains("Code", StringComparison.OrdinalIgnoreCase) || name.Contains("No", StringComparison.OrdinalIgnoreCase)) return 50;
        if (name.Contains("Name", StringComparison.OrdinalIgnoreCase) || name.Contains("Title", StringComparison.OrdinalIgnoreCase)) return 200;
        return 500;
    }

    private static string ToSnakeCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        var sb = new StringBuilder(value.Length + 8);
        for (var i = 0; i < value.Length; i++)
        {
            var ch = value[i];
            if (char.IsUpper(ch) && i > 0 && value[i - 1] != '_' && !char.IsUpper(value[i - 1])) sb.Append('_');
            sb.Append(char.ToLowerInvariant(ch));
        }
        return sb.ToString();
    }
}
''')

write("AlTayerERP.Infrastructure/Data/AppDbContextFactory.cs", r'''
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AlTayerERP.Infrastructure.Data;

/// <summary>مصنع تصميمي لتوليد Migrations دون الاتصال بخادم MySQL.</summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(
                "Server=127.0.0.1;Port=3306;Database=baseline_design_only;User=design;Password=not_used;",
                new MySqlServerVersion(new Version(8, 0, 36)))
            .Options;
        return new AppDbContext(options);
    }
}
''')

write("AlTayerERP.API/Services/PaymentRequestSchemaInitializer.cs", r'''
namespace AlTayerERP.API.Services;

/// <summary>
/// متوافق مع الاستدعاءات القديمة فقط. مخطط طلبات الصرف أصبح ضمن EF Core Migrations،
/// ولذلك لا ينفذ هذا الكلاس أي CREATE TABLE أو ALTER TABLE وقت تشغيل API.
/// </summary>
[Obsolete("مخطط قاعدة البيانات يُدار حصريًا بواسطة EF Core Migrations.")]
public sealed class PaymentRequestSchemaInitializer
{
    private readonly ILogger<PaymentRequestSchemaInitializer> _logger;
    public PaymentRequestSchemaInitializer(ILogger<PaymentRequestSchemaInitializer> logger) => _logger = logger;

    public Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("جداول طلبات الصرف مغطاة بواسطة EF Core Migrations؛ لم يُنفذ أي DDL وقت التشغيل.");
        return Task.CompletedTask;
    }
}
''')

# حفظ تاريخ Migration القديم كمرجع غير مترجم على فرع التنفيذ فقط.
legacy = ROOT / "Database/Legacy/Migrations"
legacy.mkdir(parents=True, exist_ok=True)
for name in ["20260716223222_AddJournalEntryTables.cs", "20260716223222_AddJournalEntryTables.Designer.cs", "AppDbContextModelSnapshot.cs"]:
    source = ROOT / "AlTayerERP.Infrastructure/Migrations" / name
    if source.exists():
        destination = legacy / f"{name}.legacy.txt"
        destination.write_text(source.read_text(encoding="utf-8-sig"), encoding="utf-8")
        source.unlink()

print("تم تجهيز نموذج المرحلة الأولى دون تنفيذ SQL أو إنشاء قاعدة.")
