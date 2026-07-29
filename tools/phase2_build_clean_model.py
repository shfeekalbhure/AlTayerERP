from __future__ import annotations

from pathlib import Path
import re
import shutil

ROOT = Path(__file__).resolve().parents[1]


def read(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8-sig")


def write(path: str, content: str) -> None:
    target = ROOT / path
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_text(content.rstrip() + "\n", encoding="utf-8", newline="\n")


def replace_regex(path: str, pattern: str, replacement: str, *, required: bool = False) -> None:
    content = read(path)
    updated, count = re.subn(pattern, replacement, content, flags=re.MULTILINE)
    if required and count == 0:
        raise RuntimeError(f"لم ينجح التعديل المطلوب في {path}: {pattern}")
    if count:
        write(path, updated)


# -----------------------------------------------------------------------------
# 1) توحيد Branch_ID إلى INT في عقود السندات وخدمات التحقق.
# -----------------------------------------------------------------------------
replace_regex(
    "AlTayerERP.API/DTOs/Accounting/CreateFinancialVoucherDto.cs",
    r'\s*\[Required\(ErrorMessage = "الفرع مطلوب\."\)\]\s*\[MaxLength\(50\)\]\s*public string Branch_ID \{ get; set; \} = string\.Empty;',
    '\n    [Required(ErrorMessage = "الفرع مطلوب.")]\n    [Range(1, int.MaxValue, ErrorMessage = "معرف الفرع غير صحيح.")]\n    public int Branch_ID { get; set; }',
)
replace_regex(
    "AlTayerERP.API/DTOs/Accounting/UpdateFinancialVoucherDto.cs",
    r'\s*\[Required\(ErrorMessage = "الفرع مطلوب\."\)\]\s*\[MaxLength\(50\)\]\s*public string Branch_ID \{ get; set; \} = string\.Empty;',
    '\n        [Required(ErrorMessage = "الفرع مطلوب.")]\n        [Range(1, int.MaxValue, ErrorMessage = "معرف الفرع غير صحيح.")]\n        public int Branch_ID { get; set; }',
)

validation_path = "AlTayerERP.API/Services/Accounting/VoucherValidationService.cs"
validation = read(validation_path)
validation = validation.replace(
    "string.IsNullOrWhiteSpace(voucher.Branch_ID) ||",
    "voucher.Branch_ID <= 0 ||",
)
validation = re.sub(
    r'\s*if \(!int\.TryParse\(voucher\.Branch_ID, out int branchId\)\)\s*return \(false, "معرف الفرع غير صالح\."\);',
    "\n        int branchId = voucher.Branch_ID;",
    validation,
    flags=re.MULTILINE,
)
write(validation_path, validation)

# إزالة التحويلات النصية الشائعة بعد اعتماد Branch_ID الرقمي.
for source in list((ROOT / "AlTayerERP.API").rglob("*.cs")) + list((ROOT / "AlTayerERP.Desktop").rglob("*.cs")) + list((ROOT / "AlTayerERP.Mobile.Office").rglob("*.cs")):
    text = source.read_text(encoding="utf-8-sig")
    original = text
    replacements = {
        "Branch_ID = session.Branch_ID.ToString()": "Branch_ID = session.Branch_ID",
        "Branch_ID = Session.Branch_ID.ToString()": "Branch_ID = Session.Branch_ID",
        "Branch_ID = CurrentSession.Branch_ID.ToString()": "Branch_ID = CurrentSession.Branch_ID",
        "Branch_ID = branchId.ToString()": "Branch_ID = branchId",
        "x.Branch_ID == branchId.ToString()": "x.Branch_ID == branchId",
        "voucher.Branch_ID == branchId.ToString()": "voucher.Branch_ID == branchId",
        "int.Parse(voucher.Branch_ID)": "voucher.Branch_ID",
        "Convert.ToInt32(voucher.Branch_ID)": "voucher.Branch_ID",
    }
    for old, new in replacements.items():
        text = text.replace(old, new)
    if text != original:
        source.write_text(text, encoding="utf-8", newline="\n")

# -----------------------------------------------------------------------------
# 2) استكمال مفاتيح الفرع المرجعية دون تغيير القاعدة القديمة.
# -----------------------------------------------------------------------------
branch_path = "AlTayerERP.Core/Entities/TenantBranch.cs"
branch = read(branch_path)
if "public int? Branch_Type_ID" not in branch:
    marker = '        [Column("Created_Date")]'
    additions = '''        /// <summary>معرف نوع الفرع المرجعي؛ يبقى Branch_Type كحقل توافق مؤقت.</summary>
        [Column("Branch_Type_ID")]
        public int? Branch_Type_ID { get; set; }

        /// <summary>معرف الدولة المرجعية للفرع.</summary>
        [Column("Country_ID")]
        public int? Country_ID { get; set; }

        /// <summary>معرف المحافظة المرجعية للفرع.</summary>
        [Column("Governorate_ID")]
        public int? Governorate_ID { get; set; }

        /// <summary>معرف المدينة المرجعية للفرع.</summary>
        [Column("City_ID")]
        public int? City_ID { get; set; }

'''
    if marker not in branch:
        raise RuntimeError("تعذر تحديد موضع إضافة مراجع الفرع.")
    branch = branch.replace(marker, additions + marker)
    write(branch_path, branch)

# -----------------------------------------------------------------------------
# 3) مراجع الاعتماد وأنواع المستندات للترقيم.
# -----------------------------------------------------------------------------
write(
    "AlTayerERP.Core/Entities/ApprovalAndDocumentReferenceEntities.cs",
    '''using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities;

/// <summary>حالة اعتماد مرجعية ثابتة تستخدمها دورات الموافقة.</summary>
[Table("approval_statuses")]
public sealed class ApprovalStatusReference
{
    [Key] public byte Approval_Status_ID { get; set; }
    [Required, MaxLength(30)] public string Approval_Status_Code { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Approval_Status_Name_AR { get; set; } = string.Empty;
    [MaxLength(100)] public string? Approval_Status_Name_EN { get; set; }
    public int Sort_Order { get; set; }
    public bool Is_Active { get; set; } = true;
}

/// <summary>نوع مستند مرجعي تستخدمه الروابط والترقيم.</summary>
[Table("document_types")]
public sealed class DocumentTypeReference
{
    [Key] public int Document_Type_ID { get; set; }
    [Required, MaxLength(50)] public string Document_Type_Code { get; set; } = string.Empty;
    [Required, MaxLength(150)] public string Document_Type_Name_AR { get; set; } = string.Empty;
    [MaxLength(150)] public string? Document_Type_Name_EN { get; set; }
    public int Sort_Order { get; set; }
    public bool Is_Active { get; set; } = true;
}
''',
)

# -----------------------------------------------------------------------------
# 4) منع DDL وقت تشغيل API.
# -----------------------------------------------------------------------------
write(
    "AlTayerERP.API/Services/PaymentRequestSchemaInitializer.cs",
    '''namespace AlTayerERP.API.Services;

/// <summary>
/// مكوّن توافق قديم لا ينفذ أي DDL. أصبحت جداول طلبات الصرف جزءًا من
/// Baseline EF Core، وتطبيق المخطط يتم فقط بعملية نشر Migrations صريحة.
/// </summary>
[Obsolete("تم نقل مخطط طلبات الصرف إلى EF Core Migrations.")]
public sealed class PaymentRequestSchemaInitializer
{
    private readonly ILogger<PaymentRequestSchemaInitializer> _logger;

    public PaymentRequestSchemaInitializer(ILogger<PaymentRequestSchemaInitializer> logger)
    {
        _logger = logger;
    }

    /// <summary>لا ينفذ CREATE أو ALTER أو أي SQL.</summary>
    public Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation("تم تجاوز مهيئ مخطط طلبات الصرف؛ EF Core Migrations هي المصدر الوحيد للمخطط.");
        return Task.CompletedTask;
    }
}
''',
)

# -----------------------------------------------------------------------------
# 5) AppDbContext نظيف يشمل كل جداول المرحلة الأولى.
# -----------------------------------------------------------------------------
write(
    "AlTayerERP.Infrastructure/Data/AppDbContext.cs",
    '''using AlTayerERP.Core.Entities;
using AlTayerERP.Core.Entities.Accounting;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.Infrastructure.Data;

/// <summary>
/// سياق قاعدة البيانات المعتمد للـBaseline النظيفة للمرحلة الأولى.
/// لا ينشئ أو يعدل المخطط وقت التشغيل؛ EF Core Migrations هي المصدر الوحيد.
/// </summary>
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // الهيكل المؤسسي والجغرافيا
    public DbSet<TenantGroup> Tenant_Groups => Set<TenantGroup>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<TenantBranch> Tenant_Branches => Set<TenantBranch>();
    public DbSet<FiscalYear> Fiscal_Years => Set<FiscalYear>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Governorate> Governorates => Set<Governorate>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<BranchType> Branch_Types => Set<BranchType>();

    // الأمن والصلاحيات
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserPermission> User_Permissions => Set<UserPermission>();
    public DbSet<SystemPermission> System_Permissions => Set<SystemPermission>();
    public DbSet<SystemScreen> SystemScreens => Set<SystemScreen>();
    public DbSet<LoginAttempt> Login_Attempts => Set<LoginAttempt>();
    public DbSet<RefreshToken> Refresh_Tokens => Set<RefreshToken>();

    // الإعدادات والترقيم والاعتمادات
    public DbSet<SystemSetting> System_Settings => Set<SystemSetting>();
    public DbSet<FiscalPeriod> Fiscal_Periods => Set<FiscalPeriod>();
    public DbSet<ExchangeRate> Exchange_Rates => Set<ExchangeRate>();
    public DbSet<NumberingSetting> Numbering_Settings => Set<NumberingSetting>();
    public DbSet<NumberingCounter> Numbering_Counters => Set<NumberingCounter>();
    public DbSet<FinancialPolicy> Financial_Policies => Set<FinancialPolicy>();
    public DbSet<FinancialPolicyMovement> Financial_Policy_Movements => Set<FinancialPolicyMovement>();
    public DbSet<ApprovalRequest> Approval_Requests => Set<ApprovalRequest>();
    public DbSet<ApprovalStatusReference> Approval_Statuses => Set<ApprovalStatusReference>();
    public DbSet<DocumentTypeReference> Document_Types => Set<DocumentTypeReference>();

    // الأدلة والموارد المالية
    public DbSet<AccountCodeSetting> Account_Code_Settings => Set<AccountCodeSetting>();
    public DbSet<AccountCategory> Account_Categories => Set<AccountCategory>();
    public DbSet<ChartOfAccount> Chart_Of_Accounts => Set<ChartOfAccount>();
    public DbSet<CostCenter> Cost_Centers => Set<CostCenter>();
    public DbSet<CashBox> Cash_Boxes => Set<CashBox>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<BankAccount> Bank_Accounts => Set<BankAccount>();

    // المحرك المالي والتشغيلي
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
''',
)

# -----------------------------------------------------------------------------
# 6) Fluent API: المفاتيح والعلاقات والقيود والـSeed والـCollation.
# -----------------------------------------------------------------------------
write(
    "AlTayerERP.Infrastructure/Data/Phase1ModelConfiguration.cs",
    r'''using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AlTayerERP.Core.Entities;
using AlTayerERP.Core.Entities.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AlTayerERP.Infrastructure.Data;

/// <summary>تعريف مخطط المرحلة الأولى القابل لتوليد Baseline من قاعدة فارغة.</summary>
public static class Phase1ModelConfiguration
{
    public const string CharacterSet = "utf8mb4";
    public const string Collation = "utf8mb4_unicode_ci";

    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.HasCharSet(CharacterSet);
        modelBuilder.UseCollation(Collation);

        ConfigureInstitutional(modelBuilder);
        ConfigureSecurity(modelBuilder);
        ConfigureSettings(modelBuilder);
        ConfigureAccountingMasters(modelBuilder);
        ConfigureFinancialEngine(modelBuilder);
        SeedReferenceData(modelBuilder);
        ApplyGlobalConventions(modelBuilder);
    }

    private static void ConfigureInstitutional(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TenantGroup>(e =>
        {
            e.ToTable("tenant_groups"); e.HasKey(x => x.Group_ID);
            e.Property(x => x.Group_ID).HasMaxLength(50);
            e.Property(x => x.Group_Code).HasMaxLength(30).IsRequired();
            e.Property(x => x.Group_Name_AR).HasMaxLength(150).IsRequired();
            e.Property(x => x.Group_Name_EN).HasMaxLength(150);
            e.HasIndex(x => x.Group_Code).IsUnique();
            e.HasIndex(x => new { x.Show_In_Login, x.Show_In_Tree, x.Is_Active, x.Sort_Order });
            e.Ignore(x => x.Companies_Count); e.Ignore(x => x.Main_Company_Name);
        });

        modelBuilder.Entity<Company>(e =>
        {
            e.ToTable("companies"); e.HasKey(x => x.Company_ID);
            e.Property(x => x.Company_ID).HasMaxLength(50);
            e.Property(x => x.Group_ID).HasMaxLength(50).IsRequired();
            e.Property(x => x.Company_Name_AR).HasMaxLength(200).IsRequired();
            e.Property(x => x.Company_Name_EN).HasMaxLength(200);
            e.HasOne<TenantGroup>().WithMany().HasForeignKey(x => x.Group_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.Group_ID, x.Is_Active });
        });

        modelBuilder.Entity<Country>(e =>
        {
            e.ToTable("countries", t =>
            {
                t.HasCheckConstraint("ck_countries_iso2", "iso2 IS NULL OR CHAR_LENGTH(iso2) = 2");
                t.HasCheckConstraint("ck_countries_iso3", "iso3 IS NULL OR CHAR_LENGTH(iso3) = 3");
            });
            e.HasKey(x => x.Country_ID);
            e.HasIndex(x => x.Country_Code).IsUnique();
            e.HasIndex(x => x.ISO2).IsUnique();
            e.HasIndex(x => x.ISO3).IsUnique();
        });
        modelBuilder.Entity<Governorate>(e =>
        {
            e.ToTable("governorates"); e.HasKey(x => x.Governorate_ID);
            e.HasOne(x => x.Country).WithMany(x => x.Governorates).HasForeignKey(x => x.Country_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.Country_ID, x.Governorate_Code }).IsUnique();
            e.HasIndex(x => new { x.Country_ID, x.Governorate_Name_AR }).IsUnique();
        });
        modelBuilder.Entity<City>(e =>
        {
            e.ToTable("cities"); e.HasKey(x => x.City_ID);
            e.HasOne(x => x.Country).WithMany().HasForeignKey(x => x.Country_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Governorate).WithMany(x => x.Cities).HasForeignKey(x => x.Governorate_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.Governorate_ID, x.City_Code }).IsUnique();
            e.HasIndex(x => new { x.Governorate_ID, x.City_Name_AR }).IsUnique();
        });
        modelBuilder.Entity<BranchType>(e =>
        {
            e.ToTable("branch_types"); e.HasKey(x => x.Branch_Type_ID);
            e.HasIndex(x => x.Branch_Type_Code).IsUnique();
            e.HasIndex(x => new { x.Is_Active, x.Sort_Order });
        });

        modelBuilder.Entity<TenantBranch>(e =>
        {
            e.ToTable("tenant_branches"); e.HasKey(x => x.Branch_ID);
            e.Property(x => x.Company_ID).HasMaxLength(50).IsRequired();
            e.Property(x => x.Branch_Code).HasMaxLength(30).IsRequired();
            e.Property(x => x.Branch_Name).HasMaxLength(200).IsRequired();
            e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ParentBranch).WithMany().HasForeignKey(x => x.Parent_Branch_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Currency>().WithMany().HasForeignKey(x => x.Currency_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<BranchType>().WithMany().HasForeignKey(x => x.Branch_Type_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Country>().WithMany().HasForeignKey(x => x.Country_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Governorate>().WithMany().HasForeignKey(x => x.Governorate_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<City>().WithMany().HasForeignKey(x => x.City_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.Company_ID, x.Branch_Code }).IsUnique();
        });

        modelBuilder.Entity<FiscalYear>(e =>
        {
            e.ToTable("fiscal_years", t => t.HasCheckConstraint("ck_fiscal_year_dates", "start_date <= end_date"));
            e.HasKey(x => x.Fiscal_Year_ID);
            e.Property(x => x.Company_ID).HasMaxLength(50).IsRequired();
            e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.Company_ID, x.Year_Name }).IsUnique();
        });
    }

    private static void ConfigureSecurity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(e => { e.ToTable("roles"); e.HasKey(x => x.Role_ID); e.HasIndex(x => x.Role_Code).IsUnique(); });
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users"); e.HasKey(x => x.User_ID);
            e.Property(x => x.Company_ID).HasMaxLength(50).IsRequired();
            e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Role>().WithMany().HasForeignKey(x => x.Role_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.Login_Name).IsUnique(); e.HasIndex(x => x.User_Code).IsUnique();
        });
        modelBuilder.Entity<RolePermission>(e =>
        {
            e.ToTable("role_permissions"); e.HasKey(x => x.Permission_ID);
            e.HasOne<Role>().WithMany().HasForeignKey(x => x.Role_ID).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<SystemScreen>().WithMany().HasForeignKey(x => x.Screen_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.Role_ID, x.Screen_ID }).IsUnique();
        });
        modelBuilder.Entity<UserPermission>(e =>
        {
            e.ToTable("user_permissions"); e.HasKey(x => x.Permission_ID);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.User_ID).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.User_ID, x.Permission_Category, x.Permission_Name }).IsUnique();
        });
        modelBuilder.Entity<SystemPermission>(e => { e.ToTable("system_permissions"); e.HasKey(x => x.Permission_ID); e.HasIndex(x => x.Permission_Code).IsUnique(); });
        modelBuilder.Entity<SystemScreen>(e => { e.ToTable("system_screens"); e.HasKey(x => x.Screen_ID); e.HasIndex(x => x.Screen_Code).IsUnique(); });
        modelBuilder.Entity<LoginAttempt>(e =>
        {
            e.ToTable("login_attempts"); e.HasKey(x => x.Login_Attempt_ID);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.User_ID).OnDelete(DeleteBehavior.SetNull);
            e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.SetNull);
            e.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.SetNull);
            e.HasOne<FiscalYear>().WithMany().HasForeignKey(x => x.Fiscal_Year_ID).OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => new { x.Login_Name, x.Attempted_At }); e.HasIndex(x => new { x.User_ID, x.Attempted_At }); e.HasIndex(x => x.Session_ID);
        });
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.ToTable("refresh_tokens"); e.HasKey(x => x.Refresh_Token_ID);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.User_ID).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<FiscalYear>().WithMany().HasForeignKey(x => x.Fiscal_Year_ID).OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.Token_Hash).HasMaxLength(64).IsFixedLength();
            e.HasIndex(x => x.Token_Hash).IsUnique(); e.HasIndex(x => new { x.User_ID, x.Expires_At }); e.HasIndex(x => x.Session_ID);
        });
    }

    private static void ConfigureSettings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SystemSetting>(e =>
        {
            e.ToTable("system_settings"); e.HasKey(x => x.Setting_ID);
            e.Property(x => x.Setting_Value).HasColumnType("longtext"); e.Property(x => x.Description).HasColumnType("longtext");
            e.HasIndex(x => new { x.Setting_Key, x.Scope, x.Company_ID, x.Branch_ID, x.Fiscal_Year_ID }).IsUnique();
        });
        modelBuilder.Entity<FiscalPeriod>(e =>
        {
            e.ToTable("fiscal_periods", t => t.HasCheckConstraint("ck_fiscal_period_dates", "start_date <= end_date"));
            e.HasKey(x => x.Fiscal_Period_ID);
            e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<FiscalYear>().WithMany().HasForeignKey(x => x.Fiscal_Year_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.Company_ID, x.Branch_ID, x.Fiscal_Year_ID, x.Period_Code }).IsUnique();
        });
        modelBuilder.Entity<ExchangeRate>(e =>
        {
            e.ToTable("exchange_rates", t =>
            {
                t.HasCheckConstraint("ck_exchange_rate_positive", "exchange_rate > 0");
                t.HasCheckConstraint("ck_exchange_rate_bounds", "(min_rate IS NULL OR exchange_rate >= min_rate) AND (max_rate IS NULL OR exchange_rate <= max_rate)");
            });
            e.HasKey(x => x.Exchange_Rate_ID); e.Property(x => x.Exchange_Rate_Value).HasPrecision(18, 6);
            e.Property(x => x.Min_Rate).HasPrecision(18, 6); e.Property(x => x.Max_Rate).HasPrecision(18, 6);
            e.HasOne<Currency>().WithMany().HasForeignKey(x => new { x.Company_ID, x.Currency_Code }).HasPrincipalKey(x => new { x.Company_ID, x.Currency_Code }).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.Company_ID, x.Currency_Code, x.Rate_Date }).IsUnique();
        });
        modelBuilder.Entity<NumberingSetting>(e =>
        {
            e.ToTable("numbering_settings"); e.HasKey(x => x.Numbering_ID);
            e.Property(x => x.Document_Type).HasMaxLength(50).IsRequired(); e.Property(x => x.Prefix).HasMaxLength(20);
            e.Property(x => x.Reset_Type).HasMaxLength(30).IsRequired(); e.HasIndex(x => x.Document_Type).IsUnique();
        });
        modelBuilder.Entity<NumberingCounter>(e =>
        {
            e.ToTable("numbering_counters"); e.HasKey(x => x.Counter_ID);
            e.Property(x => x.Document_Type).HasMaxLength(50).IsRequired(); e.Property(x => x.Company_ID).HasMaxLength(50);
            e.HasIndex(x => new { x.Document_Type, x.Company_ID, x.Branch_ID, x.Year_Value }).IsUnique();
            e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<FinancialPolicy>(e =>
        {
            e.ToTable("financial_limits", t => t.HasCheckConstraint("ck_financial_limit_amount", "limit_amount >= 0 AND used_amount >= 0"));
            e.HasKey(x => x.Limit_ID); e.Property(x => x.Limit_Amount).HasPrecision(19, 4); e.Property(x => x.Used_Amount).HasPrecision(19, 4);
            e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<FinancialPolicyMovement>(e =>
        {
            e.ToTable("financial_limit_movements"); e.HasKey(x => x.Movement_ID);
            e.Property(x => x.Amount).HasPrecision(19, 4); e.Property(x => x.Balance_After).HasPrecision(19, 4);
            e.HasOne<FinancialPolicy>().WithMany().HasForeignKey(x => x.Limit_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ApprovalRequest>(e =>
        {
            e.ToTable("approval_requests"); e.HasKey(x => x.Approval_ID); e.Property(x => x.Amount).HasPrecision(19, 4);
            e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ApprovalStatusReference>(e => { e.ToTable("approval_statuses"); e.HasKey(x => x.Approval_Status_ID); e.HasIndex(x => x.Approval_Status_Code).IsUnique(); });
        modelBuilder.Entity<DocumentTypeReference>(e => { e.ToTable("document_types"); e.HasKey(x => x.Document_Type_ID); e.HasIndex(x => x.Document_Type_Code).IsUnique(); });
    }

    private static void ConfigureAccountingMasters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountCodeSetting>(e =>
        {
            e.ToTable("account_code_settings"); e.HasKey(x => x.Setting_ID);
            e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.Company_ID, x.Level_No }).IsUnique();
        });
        modelBuilder.Entity<AccountCategory>(e =>
        {
            e.ToTable("account_categories"); e.HasKey(x => x.Category_ID);
            e.HasAlternateKey(x => new { x.Company_ID, x.Category_Code });
            e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ChartOfAccount>(e =>
        {
            e.ToTable("chart_of_accounts"); e.HasKey(x => x.Account_ID);
            e.Property(x => x.Account_ID).HasMaxLength(50); e.Property(x => x.Company_ID).HasMaxLength(50).IsRequired();
            e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Parent_Account_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<AccountCategory>().WithMany().HasForeignKey(x => new { x.Company_ID, x.Account_Category }).HasPrincipalKey(x => new { x.Company_ID, x.Category_Code }).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.Company_ID, x.Account_Code }).IsUnique();
        });
        modelBuilder.Entity<CostCenter>(e =>
        {
            e.ToTable("cost_centers"); e.HasKey(x => x.Cost_Center_ID);
            e.Property(x => x.Cost_Center_ID).HasMaxLength(50); e.Property(x => x.Company_ID).HasMaxLength(50).IsRequired();
            e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<CostCenter>().WithMany().HasForeignKey(x => x.Parent_Cost_Center_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.Company_ID, x.Center_Code }).IsUnique();
        });
        modelBuilder.Entity<Currency>(e =>
        {
            e.ToTable("currencies", t => t.HasCheckConstraint("ck_currency_rate_positive", "exchange_rate > 0")); e.HasKey(x => x.Currency_ID);
            e.Property(x => x.Company_ID).HasMaxLength(50).IsRequired(); e.Property(x => x.Currency_Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Exchange_Rate).HasPrecision(18, 6); e.Property(x => x.Min_Exchange_Rate).HasPrecision(18, 6); e.Property(x => x.Max_Exchange_Rate).HasPrecision(18, 6);
            e.HasAlternateKey(x => new { x.Company_ID, x.Currency_Code });
            e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<CashBox>(e =>
        {
            e.ToTable("cash_boxes", t => t.HasCheckConstraint("ck_cash_box_limits", "min_limit >= 0 AND max_limit >= min_limit")); e.HasKey(x => x.Cash_Box_ID);
            e.Property(x => x.Cash_Box_ID).HasMaxLength(50); e.Property(x => x.Company_ID).HasMaxLength(50).IsRequired();
            e.Property(x => x.Opening_Balance).HasPrecision(19, 4); e.Property(x => x.Min_Limit).HasPrecision(19, 4); e.Property(x => x.Max_Limit).HasPrecision(19, 4);
            e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Account_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Currency>().WithMany().HasForeignKey(x => new { x.Company_ID, x.Currency_Code }).HasPrincipalKey(x => new { x.Company_ID, x.Currency_Code }).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.Company_ID, x.Branch_ID, x.CashBox_Code }).IsUnique();
        });
        modelBuilder.Entity<BankAccount>(e =>
        {
            e.ToTable("bank_accounts"); e.HasKey(x => x.Bank_Account_ID);
            e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.GL_Account).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Currency>().WithMany().HasForeignKey(x => new { x.Company_ID, x.Currency_Code }).HasPrincipalKey(x => new { x.Company_ID, x.Currency_Code }).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.Company_ID, x.Account_No }).IsUnique();
        });
        modelBuilder.Entity<Party>(e =>
        {
            e.ToTable("parties"); e.HasKey(x => x.Party_ID); e.Property(x => x.Credit_Limit).HasPrecision(19, 4);
            e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Account_ID).OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => new { x.Company_ID, x.Party_Code }).IsUnique();
        });
    }

    private static void ConfigureFinancialEngine(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VoucherType>(e => { e.ToTable("voucher_types"); e.HasKey(x => x.Voucher_Type_ID); e.HasIndex(x => x.Voucher_Type_Code).IsUnique(); });
        modelBuilder.Entity<VoucherStatus>(e => { e.ToTable("voucher_statuses"); e.HasKey(x => x.Voucher_Status_ID); e.HasIndex(x => x.Voucher_Status_Code).IsUnique(); });
        modelBuilder.Entity<PaymentMethod>(e => { e.ToTable("payment_methods"); e.HasKey(x => x.Payment_Method_ID); e.HasIndex(x => x.Payment_Method_Code).IsUnique(); });

        modelBuilder.Entity<FinancialVoucherHeader>(e =>
        {
            e.ToTable("financial_voucher_headers", t =>
            {
                t.HasCheckConstraint("ck_financial_voucher_rate", "exchange_rate > 0");
                t.HasCheckConstraint("ck_financial_voucher_amounts", "amount >= 0 AND foreign_total >= 0 AND local_total >= 0");
            });
            e.HasKey(x => x.Voucher_ID);
            e.Property(x => x.Exchange_Rate).HasPrecision(18, 6); e.Property(x => x.Amount).HasPrecision(18, 2); e.Property(x => x.Foreign_Total).HasPrecision(18, 2); e.Property(x => x.Local_Total).HasPrecision(18, 2);
            e.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<FiscalYear>().WithMany().HasForeignKey(x => x.Fiscal_Year_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<VoucherType>().WithMany().HasForeignKey(x => x.Voucher_Type_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<VoucherStatus>().WithMany().HasForeignKey(x => x.Voucher_Status_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<PaymentMethod>().WithMany().HasForeignKey(x => x.Payment_Method_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Currency>().WithMany().HasForeignKey(x => x.Currency_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Party>().WithMany().HasForeignKey(x => x.Party_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Cash_Account_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<DocumentTypeReference>().WithMany().HasForeignKey(x => x.Document_Type_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ApprovalStatusReference>().WithMany().HasForeignKey(x => x.Approval_Status).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<JournalEntryHeader>().WithMany().HasForeignKey(x => x.Journal_Entry_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.Branch_ID, x.Fiscal_Year_ID, x.Voucher_Type_ID, x.Voucher_No }).IsUnique();
        });
        modelBuilder.Entity<FinancialVoucherDetail>(e =>
        {
            e.ToTable("financial_voucher_details", t =>
            {
                t.HasCheckConstraint("ck_financial_voucher_detail_rate", "exchange_rate > 0");
                t.HasCheckConstraint("ck_financial_voucher_detail_sides", "debit_amount >= 0 AND credit_amount >= 0 AND NOT (debit_amount > 0 AND credit_amount > 0)");
            });
            e.HasKey(x => x.Voucher_Detail_ID); e.Property(x => x.Exchange_Rate).HasPrecision(18, 6); e.Property(x => x.Foreign_Amount).HasPrecision(18, 2); e.Property(x => x.Local_Amount).HasPrecision(18, 2); e.Property(x => x.Debit_Amount).HasPrecision(18, 2); e.Property(x => x.Credit_Amount).HasPrecision(18, 2);
            e.HasOne(x => x.Voucher).WithMany(x => x.Details).HasForeignKey(x => x.Voucher_ID).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Account_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<CostCenter>().WithMany().HasForeignKey(x => x.Cost_Center_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Currency>().WithMany().HasForeignKey(x => x.Currency_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.Voucher_ID, x.Line_No }).IsUnique();
        });
        modelBuilder.Entity<JournalEntryHeader>(e =>
        {
            e.ToTable("journal_entry_headers", t => t.HasCheckConstraint("ck_journal_entry_balance", "total_debit = total_credit AND total_debit >= 0"));
            e.HasKey(x => x.Journal_Entry_ID); e.Property(x => x.Total_Debit).HasPrecision(18, 2); e.Property(x => x.Total_Credit).HasPrecision(18, 2);
            e.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<FiscalYear>().WithMany().HasForeignKey(x => x.Fiscal_Year_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<FinancialVoucherHeader>().WithMany().HasForeignKey(x => x.Source_Voucher_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.Entry_No).IsUnique();
        });
        modelBuilder.Entity<JournalEntryDetail>(e =>
        {
            e.ToTable("journal_entry_details", t =>
            {
                t.HasCheckConstraint("ck_journal_entry_detail_rate", "exchange_rate > 0");
                t.HasCheckConstraint("ck_journal_entry_detail_sides", "debit_amount >= 0 AND credit_amount >= 0 AND NOT (debit_amount > 0 AND credit_amount > 0)");
            });
            e.HasKey(x => x.Journal_Entry_Detail_ID); e.Property(x => x.Exchange_Rate).HasPrecision(18, 6); e.Property(x => x.Foreign_Amount).HasPrecision(18, 2); e.Property(x => x.Local_Amount).HasPrecision(18, 2); e.Property(x => x.Debit_Amount).HasPrecision(18, 2); e.Property(x => x.Credit_Amount).HasPrecision(18, 2);
            e.HasOne(x => x.JournalEntry).WithMany(x => x.Details).HasForeignKey(x => x.Journal_Entry_ID).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Account_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<CostCenter>().WithMany().HasForeignKey(x => x.Cost_Center_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Currency>().WithMany().HasForeignKey(x => x.Currency_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.Journal_Entry_ID, x.Line_No }).IsUnique();
        });
        modelBuilder.Entity<DocumentAllocation>(e =>
        {
            e.ToTable("document_allocations", t => t.HasCheckConstraint("ck_document_allocation_amounts", "document_total >= 0 AND collected_before >= 0 AND collected_now >= 0 AND remaining_balance >= 0"));
            e.HasKey(x => x.Allocation_ID); e.Property(x => x.Exchange_Rate).HasPrecision(18, 6); e.Property(x => x.Document_Total).HasPrecision(18, 2); e.Property(x => x.Collected_Before).HasPrecision(18, 2); e.Property(x => x.Collected_Now).HasPrecision(18, 2); e.Property(x => x.Remaining_Balance).HasPrecision(18, 2);
            e.HasOne(x => x.Voucher).WithMany(x => x.DocumentAllocations).HasForeignKey(x => x.Voucher_ID).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<Party>().WithMany().HasForeignKey(x => x.Party_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Currency>().WithMany().HasForeignKey(x => x.Currency_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<DocumentTypeReference>().WithMany().HasForeignKey(x => x.Document_Type_ID).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<DocumentLink>(e =>
        {
            e.ToTable("document_links"); e.HasKey(x => x.Document_Link_ID);
            e.HasOne<DocumentTypeReference>().WithMany().HasForeignKey(x => x.From_Document_Type_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<DocumentTypeReference>().WithMany().HasForeignKey(x => x.To_Document_Type_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.From_Document_Type_ID, x.From_Document_ID, x.To_Document_Type_ID, x.To_Document_ID, x.Link_Type }).IsUnique();
        });
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.ToTable("audit_logs", t => t.HasCheckConstraint("ck_audit_action_upper", "CHAR_LENGTH(action_type) BETWEEN 1 AND 64 AND action_type = UPPER(action_type)")); e.HasKey(x => x.Audit_ID);
            e.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => new { x.Table_Name, x.Record_ID, x.Action_At });
        });
        modelBuilder.Entity<VoucherActionLog>(e =>
        {
            e.ToTable("voucher_action_logs"); e.HasKey(x => x.Voucher_Action_ID);
            e.HasOne(x => x.Voucher).WithMany(x => x.VoucherActionLogs).HasForeignKey(x => x.Voucher_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.Voucher_ID, x.Action_At });
        });

        modelBuilder.Entity<PaymentRequest>(e =>
        {
            e.ToTable("payment_requests", t => t.HasCheckConstraint("ck_payment_request_total", "approved_local_total >= 0")); e.HasKey(x => x.Payment_Request_ID); e.Property(x => x.Approved_Local_Total).HasPrecision(19, 4);
            e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<FiscalYear>().WithMany().HasForeignKey(x => x.Fiscal_Year_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Party>().WithMany().HasForeignKey(x => x.Party_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<PaymentMethod>().WithMany().HasForeignKey(x => x.Payment_Method_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<FinancialVoucherHeader>().WithMany().HasForeignKey(x => x.Payment_Voucher_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Details).WithOne(x => x.PaymentRequest).HasForeignKey(x => x.Payment_Request_ID).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.Company_ID, x.Branch_ID, x.Fiscal_Year_ID, x.Request_No }).IsUnique();
            e.HasIndex(x => new { x.Company_ID, x.Branch_ID, x.Fiscal_Year_ID, x.Status });
        });
        modelBuilder.Entity<PaymentRequestLine>(e =>
        {
            e.ToTable("payment_request_lines", t => t.HasCheckConstraint("ck_payment_request_line_amounts", "exchange_rate > 0 AND foreign_amount >= 0 AND local_amount >= 0")); e.HasKey(x => x.Payment_Request_Line_ID);
            e.Property(x => x.Exchange_Rate).HasPrecision(19, 8); e.Property(x => x.Foreign_Amount).HasPrecision(19, 4); e.Property(x => x.Local_Amount).HasPrecision(19, 4);
            e.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Account_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<CostCenter>().WithMany().HasForeignKey(x => x.Cost_Center_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Currency>().WithMany().HasForeignKey(x => x.Currency_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.Payment_Request_ID, x.Line_No }).IsUnique();
        });
        modelBuilder.Entity<PaymentRequestAttachment>(e =>
        {
            e.ToTable("payment_request_attachments"); e.HasKey(x => x.Payment_Request_Attachment_ID);
            e.HasOne<PaymentRequest>().WithMany().HasForeignKey(x => x.Payment_Request_ID).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<FiscalYear>().WithMany().HasForeignKey(x => x.Fiscal_Year_ID).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.Payment_Request_ID, x.Is_Active });
        });
    }

    private static void SeedReferenceData(ModelBuilder modelBuilder)
    {
        DateTime seedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<BranchType>().HasData(
            new BranchType { Branch_Type_ID = 1, Branch_Type_Code = "MAIN", Branch_Type_Name_AR = "رئيسي", Branch_Type_Name_EN = "Main", Sort_Order = 10, Is_Active = true, Created_At = seedDate },
            new BranchType { Branch_Type_ID = 2, Branch_Type_Code = "OPERATING", Branch_Type_Name_AR = "تشغيلي", Branch_Type_Name_EN = "Operating", Sort_Order = 20, Is_Active = true, Created_At = seedDate },
            new BranchType { Branch_Type_ID = 3, Branch_Type_Code = "DISTRIBUTION", Branch_Type_Name_AR = "نقطة توزيع", Branch_Type_Name_EN = "Distribution Point", Sort_Order = 30, Is_Active = true, Created_At = seedDate },
            new BranchType { Branch_Type_ID = 4, Branch_Type_Code = "WAREHOUSE", Branch_Type_Name_AR = "مستودع", Branch_Type_Name_EN = "Warehouse", Sort_Order = 40, Is_Active = true, Created_At = seedDate });

        modelBuilder.Entity<VoucherType>().HasData(
            new VoucherType { Voucher_Type_ID = 1, Voucher_Type_Code = "RECEIPT", Voucher_Type_Name_AR = "سند قبض", Voucher_Type_Name_EN = "Receipt Voucher", Is_Active = true, Sort_Order = 10 },
            new VoucherType { Voucher_Type_ID = 2, Voucher_Type_Code = "PAYMENT", Voucher_Type_Name_AR = "سند صرف", Voucher_Type_Name_EN = "Payment Voucher", Is_Active = true, Sort_Order = 20 },
            new VoucherType { Voucher_Type_ID = 3, Voucher_Type_Code = "JOURNAL", Voucher_Type_Name_AR = "قيد يومية", Voucher_Type_Name_EN = "Journal Voucher", Is_Active = true, Sort_Order = 30 });

        modelBuilder.Entity<VoucherStatus>().HasData(
            new VoucherStatus { Voucher_Status_ID = 1, Voucher_Status_Code = "DRAFT", Voucher_Status_Name_AR = "مسودة", Voucher_Status_Name_EN = "Draft", Is_Active = true, Sort_Order = 10 },
            new VoucherStatus { Voucher_Status_ID = 2, Voucher_Status_Code = "APPROVED", Voucher_Status_Name_AR = "معتمد", Voucher_Status_Name_EN = "Approved", Is_Active = true, Sort_Order = 20 },
            new VoucherStatus { Voucher_Status_ID = 3, Voucher_Status_Code = "POSTED", Voucher_Status_Name_AR = "مرحل", Voucher_Status_Name_EN = "Posted", Is_Active = true, Sort_Order = 30 },
            new VoucherStatus { Voucher_Status_ID = 4, Voucher_Status_Code = "CANCELLED", Voucher_Status_Name_AR = "ملغي", Voucher_Status_Name_EN = "Cancelled", Is_Active = true, Sort_Order = 40 });

        modelBuilder.Entity<PaymentMethod>().HasData(
            new PaymentMethod { Payment_Method_ID = 1, Payment_Method_Code = "CASH", Payment_Method_Name_AR = "نقدي", Payment_Method_Name_EN = "Cash", Is_Cash = true, Is_Bank = false, Is_Active = true, Sort_Order = 10 },
            new PaymentMethod { Payment_Method_ID = 2, Payment_Method_Code = "BANK_TRANSFER", Payment_Method_Name_AR = "تحويل بنكي", Payment_Method_Name_EN = "Bank Transfer", Is_Cash = false, Is_Bank = true, Requires_Reference = true, Is_Active = true, Sort_Order = 20 },
            new PaymentMethod { Payment_Method_ID = 3, Payment_Method_Code = "CHEQUE", Payment_Method_Name_AR = "شيك", Payment_Method_Name_EN = "Cheque", Is_Cash = false, Is_Bank = true, Requires_Reference = true, Requires_Reference_Date = true, Is_Active = true, Sort_Order = 30 },
            new PaymentMethod { Payment_Method_ID = 4, Payment_Method_Code = "CREDIT", Payment_Method_Name_AR = "آجل", Payment_Method_Name_EN = "Credit", Is_Active = true, Sort_Order = 40 });

        modelBuilder.Entity<ApprovalStatusReference>().HasData(
            new ApprovalStatusReference { Approval_Status_ID = 0, Approval_Status_Code = "NOT_REQUIRED", Approval_Status_Name_AR = "لا يتطلب اعتماد", Approval_Status_Name_EN = "Not Required", Sort_Order = 0, Is_Active = true },
            new ApprovalStatusReference { Approval_Status_ID = 1, Approval_Status_Code = "PENDING", Approval_Status_Name_AR = "بانتظار الاعتماد", Approval_Status_Name_EN = "Pending", Sort_Order = 10, Is_Active = true },
            new ApprovalStatusReference { Approval_Status_ID = 2, Approval_Status_Code = "UNDER_REVIEW", Approval_Status_Name_AR = "تحت المراجعة", Approval_Status_Name_EN = "Under Review", Sort_Order = 20, Is_Active = true },
            new ApprovalStatusReference { Approval_Status_ID = 3, Approval_Status_Code = "APPROVED", Approval_Status_Name_AR = "معتمد", Approval_Status_Name_EN = "Approved", Sort_Order = 30, Is_Active = true },
            new ApprovalStatusReference { Approval_Status_ID = 4, Approval_Status_Code = "REJECTED", Approval_Status_Name_AR = "مرفوض", Approval_Status_Name_EN = "Rejected", Sort_Order = 40, Is_Active = true },
            new ApprovalStatusReference { Approval_Status_ID = 5, Approval_Status_Code = "RETURNED", Approval_Status_Name_AR = "معاد للتعديل", Approval_Status_Name_EN = "Returned", Sort_Order = 50, Is_Active = true });

        modelBuilder.Entity<DocumentTypeReference>().HasData(
            new DocumentTypeReference { Document_Type_ID = 1, Document_Type_Code = "BRANCH", Document_Type_Name_AR = "فرع", Document_Type_Name_EN = "Branch", Sort_Order = 10, Is_Active = true },
            new DocumentTypeReference { Document_Type_ID = 2, Document_Type_Code = "COMPANY", Document_Type_Name_AR = "شركة", Document_Type_Name_EN = "Company", Sort_Order = 20, Is_Active = true },
            new DocumentTypeReference { Document_Type_ID = 3, Document_Type_Code = "RECEIPT_VOUCHER", Document_Type_Name_AR = "سند قبض", Document_Type_Name_EN = "Receipt Voucher", Sort_Order = 30, Is_Active = true },
            new DocumentTypeReference { Document_Type_ID = 4, Document_Type_Code = "PAYMENT_VOUCHER", Document_Type_Name_AR = "سند صرف", Document_Type_Name_EN = "Payment Voucher", Sort_Order = 40, Is_Active = true },
            new DocumentTypeReference { Document_Type_ID = 5, Document_Type_Code = "JOURNAL_ENTRY", Document_Type_Name_AR = "قيد محاسبي", Document_Type_Name_EN = "Journal Entry", Sort_Order = 50, Is_Active = true },
            new DocumentTypeReference { Document_Type_ID = 6, Document_Type_Code = "PAYMENT_REQUEST", Document_Type_Name_AR = "طلب صرف", Document_Type_Name_EN = "Payment Request", Sort_Order = 60, Is_Active = true });

        modelBuilder.Entity<NumberingSetting>().HasData(
            new NumberingSetting { Numbering_ID = 1, Document_Type = "BRANCH", Prefix = "BR", Digits_Count = 4, Reset_Type = "None", Last_Number = 0, Is_Active = true },
            new NumberingSetting { Numbering_ID = 2, Document_Type = "COMPANY", Prefix = "CO", Digits_Count = 4, Reset_Type = "None", Last_Number = 0, Is_Active = true },
            new NumberingSetting { Numbering_ID = 3, Document_Type = "RECEIPT_VOUCHER", Prefix = "RV", Digits_Count = 6, Reset_Type = "BranchYear", Last_Number = 0, Use_Branch = true, Use_Year = true, Is_Active = true },
            new NumberingSetting { Numbering_ID = 4, Document_Type = "PAYMENT_VOUCHER", Prefix = "PV", Digits_Count = 6, Reset_Type = "BranchYear", Last_Number = 0, Use_Branch = true, Use_Year = true, Is_Active = true },
            new NumberingSetting { Numbering_ID = 5, Document_Type = "JOURNAL_ENTRY", Prefix = "JE", Digits_Count = 6, Reset_Type = "BranchYear", Last_Number = 0, Use_Branch = true, Use_Year = true, Is_Active = true },
            new NumberingSetting { Numbering_ID = 6, Document_Type = "PAYMENT_REQUEST", Prefix = "PRQ", Digits_Count = 6, Reset_Type = "BranchYear", Last_Number = 0, Use_Branch = true, Use_Year = true, Is_Active = true });

        string[] permissionCodes = ["VIEW", "ADD", "EDIT", "DELETE", "PRINT", "EXPORT", "IMPORT", "APPROVE", "UNAPPROVE"];
        modelBuilder.Entity<SystemPermission>().HasData(permissionCodes.Select((code, index) => new SystemPermission
        {
            Permission_ID = index + 1,
            Permission_Code = code,
            Permission_Name = code switch { "VIEW" => "عرض", "ADD" => "إضافة", "EDIT" => "تعديل", "DELETE" => "إيقاف/حذف", "PRINT" => "طباعة", "EXPORT" => "تصدير", "IMPORT" => "استيراد", "APPROVE" => "اعتماد", _ => "إلغاء اعتماد" },
            Permission_Type = "Data",
            Is_Active = true,
            Sort_Order = (index + 1) * 10,
            Created_At = seedDate
        }));

        (string Code, string Name, string Module)[] screens =
        [
            ("TenantGroups", "المجموعات التجارية", "الهيكل المؤسسي"), ("Companies", "الشركات", "الهيكل المؤسسي"),
            ("Branches", "الفروع", "الهيكل المؤسسي"), ("Countries", "الدول", "الهيكل المؤسسي"),
            ("Governorates", "المحافظات", "الهيكل المؤسسي"), ("Cities", "المدن", "الهيكل المؤسسي"),
            ("Users", "المستخدمون", "المستخدمون والصلاحيات"), ("Roles", "الأدوار", "المستخدمون والصلاحيات"),
            ("RolePermissions", "صلاحيات الأدوار", "المستخدمون والصلاحيات"), ("AuditLogs", "سجل التدقيق والرقابة", "الأمن والرقابة"),
            ("ChartOfAccounts", "دليل الحسابات", "النظام المالي"), ("CostCenters", "مراكز التكلفة", "النظام المالي"),
            ("CashBoxes", "الصناديق", "النظام المالي"), ("ReceiptVoucher", "سند القبض", "النظام المالي"),
            ("PaymentVoucher", "سند الصرف", "النظام المالي"), ("JournalVoucher", "القيود اليومية", "النظام المالي"),
            ("PaymentRequest", "طلبات الصرف", "النظام المالي"), ("FiscalYears", "السنوات المالية", "التهيئة والإعدادات"),
            ("FiscalPeriods", "الفترات المالية", "التهيئة والإعدادات"), ("NumberingSettings", "إعدادات الترقيم", "التهيئة والإعدادات")
        ];
        modelBuilder.Entity<SystemScreen>().HasData(screens.Select((screen, index) => new SystemScreen
        {
            Screen_ID = index + 1, Screen_Code = screen.Code, Screen_Name = screen.Name, Module_Name = screen.Module,
            Is_Active = true, Sort_Order = (index + 1) * 10, Created_At = seedDate
        }));
    }

    private static void ApplyGlobalConventions(ModelBuilder modelBuilder)
    {
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            string tableName = ToSnakeCase(entityType.GetTableName() ?? entityType.ClrType.Name);
            entityType.SetTableName(tableName);

            foreach (IMutableProperty property in entityType.GetProperties())
            {
                string columnName = ToSnakeCase(property.Name);
                property.SetColumnName(columnName);
                if (property.ClrType == typeof(string))
                {
                    property.SetCollation(Collation);
                    if (property.GetMaxLength() is null && property.GetColumnType() is not "json" and not "longtext")
                        property.SetMaxLength(DefaultStringLength(property.Name));
                }
                if (property.ClrType == typeof(DateTime) && property.Name.EndsWith("_At", StringComparison.Ordinal))
                    property.SetColumnType("datetime(6)");
            }

            foreach (IMutableKey key in entityType.GetKeys())
                key.SetName(SafeIdentifier($"{(key.IsPrimaryKey() ? "pk" : "ak")}_{tableName}_{string.Join("_", key.Properties.Select(p => ToSnakeCase(p.Name)))}"));

            foreach (IMutableIndex index in entityType.GetIndexes())
                index.SetDatabaseName(SafeIdentifier($"{(index.IsUnique ? "ux" : "ix")}_{tableName}_{string.Join("_", index.Properties.Select(p => ToSnakeCase(p.Name)))}"));

            foreach (IMutableForeignKey foreignKey in entityType.GetForeignKeys())
            {
                string principal = ToSnakeCase(foreignKey.PrincipalEntityType.GetTableName() ?? foreignKey.PrincipalEntityType.ClrType.Name);
                foreignKey.SetConstraintName(SafeIdentifier($"fk_{tableName}_{string.Join("_", foreignKey.Properties.Select(p => ToSnakeCase(p.Name)))}_{principal}"));
            }
        }

        // Defaults العامة بعد تثبيت أسماء الأعمدة.
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (IMutableProperty property in entityType.GetProperties())
            {
                if (property.Name == "Is_Active" && property.ClrType == typeof(bool)) property.SetDefaultValue(true);
                if (property.Name is "Sort_Order" or "Edit_Count" or "Print_Count" or "Undo_Count" or "Last_Number") property.SetDefaultValue(0);
                if (property.Name is "Created_At" && property.ClrType == typeof(DateTime) && property.ValueGenerated == ValueGenerated.Never)
                    property.SetDefaultValueSql("CURRENT_TIMESTAMP(6)");
            }
        }
    }

    private static int DefaultStringLength(string name)
    {
        if (name.EndsWith("_ID", StringComparison.OrdinalIgnoreCase)) return 50;
        if (name.Contains("Code", StringComparison.OrdinalIgnoreCase)) return 50;
        if (name.Contains("Name", StringComparison.OrdinalIgnoreCase)) return 200;
        if (name.Contains("Hash", StringComparison.OrdinalIgnoreCase)) return 128;
        if (name.Contains("Notes", StringComparison.OrdinalIgnoreCase) || name.Contains("Description", StringComparison.OrdinalIgnoreCase) || name.Contains("Reason", StringComparison.OrdinalIgnoreCase)) return 1000;
        return 500;
    }

    private static string ToSnakeCase(string value)
    {
        string normalized = Regex.Replace(value, "([a-z0-9])([A-Z])", "$1_$2").Replace("__", "_");
        return normalized.Trim('_').ToLowerInvariant();
    }

    private static string SafeIdentifier(string value)
    {
        if (value.Length <= 63) return value;
        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))[..8].ToLowerInvariant();
        return value[..54] + "_" + hash;
    }
}
''',
)

# -----------------------------------------------------------------------------
# 7) Design-time factory بدون اتصال أو AutoDetect.
# -----------------------------------------------------------------------------
write(
    "AlTayerERP.Infrastructure/Data/AppDbContextDesignFactory.cs",
    '''using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AlTayerERP.Infrastructure.Data;

/// <summary>ينشئ النموذج لأدوات EF دون فتح اتصال بقاعدة فعلية.</summary>
public sealed class AppDbContextDesignFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(
                "Server=127.0.0.1;Database=__altayer_design_time_only;User=design;Password=not_used;",
                new MySqlServerVersion(new Version(8, 0, 36)))
            .Options;
        return new AppDbContext(options);
    }
}
''',
)

# -----------------------------------------------------------------------------
# 8) أدوات EF وحفظ تاريخ Migrations القديم خارج الاكتشاف.
# -----------------------------------------------------------------------------
csproj_path = "AlTayerERP.Infrastructure/AlTayerERP.Infrastructure.csproj"
csproj = read(csproj_path)
if "Microsoft.EntityFrameworkCore.Design" not in csproj:
    csproj = csproj.replace(
        '<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.12" />',
        '<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.12" />\n\t\t<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.12">\n\t\t\t<PrivateAssets>all</PrivateAssets>\n\t\t\t<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>\n\t\t</PackageReference>',
    )
write(csproj_path, csproj)

legacy_dir = ROOT / "Database/Legacy/Migrations"
legacy_dir.mkdir(parents=True, exist_ok=True)
migrations_dir = ROOT / "AlTayerERP.Infrastructure/Migrations"
for filename in [
    "20260716223222_AddJournalEntryTables.cs",
    "20260716223222_AddJournalEntryTables.Designer.cs",
    "AppDbContextModelSnapshot.cs",
]:
    source = migrations_dir / filename
    if source.exists():
        shutil.copy2(source, legacy_dir / filename)
        source.unlink()

print("تم تجهيز نموذج EF Core للـBaseline دون إنشاء قاعدة أو تنفيذ SQL.")
