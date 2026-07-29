using System.Text;
using AlTayerERP.Core.Entities;
using AlTayerERP.Core.Entities.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AlTayerERP.Infrastructure.Data;

/// <summary>
/// يطبق قرارات Baseline المرحلة الأولى بعد إعدادات AppDbContext الحالية.
/// لا ينفذ هذا المكوّن أي DDL ولا يتصل بقاعدة البيانات.
/// </summary>
public sealed class Phase1ModelCustomizer : ModelCustomizer
{
    public const string CharacterSet = "utf8mb4";
    public const string Collation = "utf8mb4_unicode_ci";

    public Phase1ModelCustomizer(ModelCustomizerDependencies dependencies)
        : base(dependencies)
    {
    }

    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);
        Phase1BaselineModel.Configure(modelBuilder);
    }
}

/// <summary>تعريف نموذج قاعدة المرحلة الأولى النظيفة.</summary>
internal static class Phase1BaselineModel
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation(Phase1ModelCustomizer.Collation)
            .HasCharSet(Phase1ModelCustomizer.CharacterSet);

        RegisterReferenceEntities(modelBuilder);
        ConfigureInstitutionalRelationships(modelBuilder);
        ConfigureSecurityRelationships(modelBuilder);
        ConfigureFinancialRelationships(modelBuilder);
        ConfigureIndexesAndChecks(modelBuilder);
        ConfigureReferenceSeed(modelBuilder);
        ApplyGlobalConventions(modelBuilder);
    }

    private static void RegisterReferenceEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountCategory>();
        modelBuilder.Entity<Country>();
        modelBuilder.Entity<Governorate>();
        modelBuilder.Entity<City>();
        modelBuilder.Entity<BranchType>();
        modelBuilder.Entity<ApprovalStatusReference>();
        modelBuilder.Entity<DocumentTypeReference>();
    }

    private static void ConfigureInstitutionalRelationships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TenantGroup>(entity =>
        {
            entity.Property(x => x.Group_ID).HasMaxLength(50);
            entity.Property(x => x.Group_Code).HasMaxLength(30);
            entity.HasIndex(x => x.Group_Code).IsUnique();
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.Property(x => x.Company_ID).HasMaxLength(50);
            entity.Property(x => x.Group_ID).HasMaxLength(50);
            entity.HasOne<TenantGroup>()
                .WithMany()
                .HasForeignKey(x => x.Group_ID)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Group_ID, x.Is_Active });
        });

        modelBuilder.Entity<TenantBranch>(entity =>
        {
            entity.Property(x => x.Company_ID).HasMaxLength(50);
            entity.Property(x => x.Branch_Code).HasMaxLength(30);
            entity.Property(x => x.Branch_Type).HasMaxLength(30);
            entity.HasOne<Company>()
                .WithMany()
                .HasForeignKey(x => x.Company_ID)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ParentBranch)
                .WithMany()
                .HasForeignKey(x => x.Parent_Branch_ID)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Country>()
                .WithMany()
                .HasForeignKey(x => x.Country_ID)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Governorate>()
                .WithMany()
                .HasForeignKey(x => x.Governorate_ID)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<City>()
                .WithMany()
                .HasForeignKey(x => x.City_ID)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Company_ID, x.Branch_Code }).IsUnique();
        });

        modelBuilder.Entity<FiscalYear>(entity =>
        {
            entity.Property(x => x.Company_ID).HasMaxLength(50);
            entity.HasOne<Company>()
                .WithMany()
                .HasForeignKey(x => x.Company_ID)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Company_ID, x.Year_Name }).IsUnique();
        });

        modelBuilder.Entity<Governorate>(entity =>
        {
            entity.HasOne<Country>()
                .WithMany()
                .HasForeignKey(x => x.Country_ID)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Country_ID, x.Governorate_Code }).IsUnique();
            entity.HasIndex(x => new { x.Country_ID, x.Governorate_Name_AR }).IsUnique();
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasOne<Country>()
                .WithMany()
                .HasForeignKey(x => x.Country_ID)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Governorate>()
                .WithMany()
                .HasForeignKey(x => x.Governorate_ID)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Governorate_ID, x.City_Code }).IsUnique();
            entity.HasIndex(x => new { x.Governorate_ID, x.City_Name_AR }).IsUnique();
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasIndex(x => x.Country_Code).IsUnique();
            entity.HasIndex(x => x.ISO2).IsUnique();
            entity.HasIndex(x => x.ISO3).IsUnique();
        });

        modelBuilder.Entity<BranchType>(entity =>
        {
            entity.HasIndex(x => x.Branch_Type_Code).IsUnique();
            entity.HasIndex(x => new { x.Is_Active, x.Sort_Order });
        });
    }

    private static void ConfigureSecurityRelationships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(x => x.Company_ID).HasMaxLength(50);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Role>().WithMany().HasForeignKey(x => x.Role_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.Login_Name).IsUnique();
            entity.HasIndex(x => new { x.Company_ID, x.User_Code }).IsUnique();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(x => x.Role_Code).IsUnique();
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasOne<Role>().WithMany().HasForeignKey(x => x.Role_ID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<SystemScreen>().WithMany().HasForeignKey(x => x.Screen_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Role_ID, x.Screen_ID }).IsUnique();
        });

        modelBuilder.Entity<UserPermission>(entity =>
        {
            entity.HasOne<User>().WithMany().HasForeignKey(x => x.User_ID).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.User_ID, x.Permission_Category, x.Permission_Name }).IsUnique();
        });

        modelBuilder.Entity<SystemPermission>().HasIndex(x => x.Permission_Code).IsUnique();
        modelBuilder.Entity<SystemScreen>().HasIndex(x => x.Screen_Code).IsUnique();

        modelBuilder.Entity<LoginAttempt>(entity =>
        {
            entity.HasOne<User>().WithMany().HasForeignKey(x => x.User_ID).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasOne<User>().WithMany().HasForeignKey(x => x.User_ID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<FiscalYear>().WithMany().HasForeignKey(x => x.Fiscal_Year_ID).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureFinancialRelationships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.HasIndex(x => new { x.Setting_Key, x.Scope, x.Company_ID, x.Branch_ID, x.Fiscal_Year_ID }).IsUnique();
        });

        modelBuilder.Entity<FiscalPeriod>(entity =>
        {
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<FiscalYear>().WithMany().HasForeignKey(x => x.Fiscal_Year_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Company_ID, x.Branch_ID, x.Fiscal_Year_ID, x.Period_Code }).IsUnique();
        });

        modelBuilder.Entity<ExchangeRate>(entity =>
        {
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Company_ID, x.Currency_Code, x.Rate_Date }).IsUnique();
            entity.Property(x => x.Exchange_Rate_Value).HasPrecision(18, 6);
            entity.Property(x => x.Min_Rate).HasPrecision(18, 6);
            entity.Property(x => x.Max_Rate).HasPrecision(18, 6);
        });

        modelBuilder.Entity<AccountCategory>(entity =>
        {
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Company_ID, x.Category_Code }).IsUnique();
        });

        modelBuilder.Entity<ChartOfAccount>(entity =>
        {
            entity.Property(x => x.Account_ID).HasMaxLength(50);
            entity.Property(x => x.Company_ID).HasMaxLength(50);
            entity.Property(x => x.Parent_Account_ID).HasMaxLength(50);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Parent_Account_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Company_ID, x.Account_Code }).IsUnique();
        });

        modelBuilder.Entity<CostCenter>(entity =>
        {
            entity.Property(x => x.Cost_Center_ID).HasMaxLength(50);
            entity.Property(x => x.Company_ID).HasMaxLength(50);
            entity.Property(x => x.Parent_Cost_Center_ID).HasMaxLength(50);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<CostCenter>().WithMany().HasForeignKey(x => x.Parent_Cost_Center_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Company_ID, x.Center_Code }).IsUnique();
        });

        modelBuilder.Entity<CashBox>(entity =>
        {
            entity.Property(x => x.Cash_Box_ID).HasMaxLength(50);
            entity.Property(x => x.Company_ID).HasMaxLength(50);
            entity.Property(x => x.Account_ID).HasMaxLength(50);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Account_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Company_ID, x.Branch_ID, x.CashBox_Code }).IsUnique();
            entity.Property(x => x.Opening_Balance).HasPrecision(19, 4);
            entity.Property(x => x.Max_Limit).HasPrecision(19, 4);
            entity.Property(x => x.Min_Limit).HasPrecision(19, 4);
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Company_ID, x.Currency_Code }).IsUnique();
            entity.Property(x => x.Exchange_Rate).HasPrecision(19, 8);
            entity.Property(x => x.Min_Exchange_Rate).HasPrecision(19, 8);
            entity.Property(x => x.Max_Exchange_Rate).HasPrecision(19, 8);
        });

        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Party>(entity =>
        {
            entity.Property(x => x.Party_ID).HasMaxLength(50);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Account_ID).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.Company_ID, x.Party_Code }).IsUnique();
            entity.Property(x => x.Credit_Limit).HasPrecision(19, 4);
        });

        modelBuilder.Entity<FinancialVoucherHeader>(entity =>
        {
            entity.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<FiscalYear>().WithMany().HasForeignKey(x => x.Fiscal_Year_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Currency>().WithMany().HasForeignKey(x => x.Currency_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Party>().WithMany().HasForeignKey(x => x.Party_ID).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<JournalEntryHeader>(entity =>
        {
            entity.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<FiscalYear>().WithMany().HasForeignKey(x => x.Fiscal_Year_ID).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FinancialVoucherDetail>(entity =>
        {
            entity.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Account_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<CostCenter>().WithMany().HasForeignKey(x => x.Cost_Center_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Currency>().WithMany().HasForeignKey(x => x.Currency_ID).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<JournalEntryDetail>(entity =>
        {
            entity.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Account_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<CostCenter>().WithMany().HasForeignKey(x => x.Cost_Center_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Currency>().WithMany().HasForeignKey(x => x.Currency_ID).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PaymentRequest>(entity =>
        {
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<FiscalYear>().WithMany().HasForeignKey(x => x.Fiscal_Year_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Party>().WithMany().HasForeignKey(x => x.Party_ID).OnDelete(DeleteBehavior.SetNull);
            entity.HasMany(x => x.Details).WithOne(x => x.PaymentRequest)
                .HasForeignKey(x => x.Payment_Request_ID).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PaymentRequestLine>(entity =>
        {
            entity.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Account_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<CostCenter>().WithMany().HasForeignKey(x => x.Cost_Center_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Currency>().WithMany().HasForeignKey(x => x.Currency_ID).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PaymentRequestAttachment>(entity =>
        {
            entity.HasOne<PaymentRequest>().WithMany().HasForeignKey(x => x.Payment_Request_ID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<FiscalYear>().WithMany().HasForeignKey(x => x.Fiscal_Year_ID).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureIndexesAndChecks(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApprovalStatusReference>().HasIndex(x => x.Approval_Status_Code).IsUnique();
        modelBuilder.Entity<DocumentTypeReference>().HasIndex(x => x.Document_Type_Code).IsUnique();

        modelBuilder.Entity<FiscalPeriod>().ToTable(table =>
            table.HasCheckConstraint("ck_fiscal_period_dates", "`start_date` <= `end_date`"));
        modelBuilder.Entity<ExchangeRate>().ToTable(table =>
            table.HasCheckConstraint("ck_exchange_rate_positive", "`exchange_rate` > 0"));
        modelBuilder.Entity<FinancialVoucherDetail>().ToTable(table =>
            table.HasCheckConstraint("ck_voucher_line_debit_credit", "NOT (`debit_amount` > 0 AND `credit_amount` > 0)"));
        modelBuilder.Entity<JournalEntryDetail>().ToTable(table =>
            table.HasCheckConstraint("ck_journal_line_debit_credit", "NOT (`debit_amount` > 0 AND `credit_amount` > 0)"));
        modelBuilder.Entity<PaymentRequestLine>().ToTable(table =>
            table.HasCheckConstraint("ck_payment_request_rate_positive", "`exchange_rate` > 0"));
    }

    private static void ConfigureReferenceSeed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BranchType>().HasData(
            new BranchType { Branch_Type_ID = 1, Branch_Type_Code = "MAIN", Branch_Type_Name_AR = "رئيسي", Branch_Type_Name_EN = "Main", Sort_Order = 10, Is_Active = true },
            new BranchType { Branch_Type_ID = 2, Branch_Type_Code = "OPERATING", Branch_Type_Name_AR = "تشغيلي", Branch_Type_Name_EN = "Operating", Sort_Order = 20, Is_Active = true },
            new BranchType { Branch_Type_ID = 3, Branch_Type_Code = "DISTRIBUTION", Branch_Type_Name_AR = "نقطة توزيع", Branch_Type_Name_EN = "Distribution Point", Sort_Order = 30, Is_Active = true },
            new BranchType { Branch_Type_ID = 4, Branch_Type_Code = "WAREHOUSE", Branch_Type_Name_AR = "مستودع", Branch_Type_Name_EN = "Warehouse", Sort_Order = 40, Is_Active = true });

        modelBuilder.Entity<VoucherType>().HasData(
            new VoucherType { Voucher_Type_ID = 1, Voucher_Type_Code = "RECEIPT", Voucher_Type_Name_AR = "سند قبض", Voucher_Type_Name_EN = "Receipt Voucher", Is_Active = true, Sort_Order = 10 },
            new VoucherType { Voucher_Type_ID = 2, Voucher_Type_Code = "PAYMENT", Voucher_Type_Name_AR = "سند صرف", Voucher_Type_Name_EN = "Payment Voucher", Is_Active = true, Sort_Order = 20 },
            new VoucherType { Voucher_Type_ID = 3, Voucher_Type_Code = "JOURNAL", Voucher_Type_Name_AR = "قيد يومية", Voucher_Type_Name_EN = "Journal Voucher", Is_Active = true, Sort_Order = 30 });

        modelBuilder.Entity<VoucherStatus>().HasData(
            new VoucherStatus { Voucher_Status_ID = 1, Voucher_Status_Code = "DRAFT", Voucher_Status_Name_AR = "مسودة", Voucher_Status_Name_EN = "Draft", Is_Active = true, Sort_Order = 10 },
            new VoucherStatus { Voucher_Status_ID = 2, Voucher_Status_Code = "APPROVED", Voucher_Status_Name_AR = "معتمد", Voucher_Status_Name_EN = "Approved", Is_Active = true, Sort_Order = 20 },
            new VoucherStatus { Voucher_Status_ID = 3, Voucher_Status_Code = "POSTED", Voucher_Status_Name_AR = "مرحل", Voucher_Status_Name_EN = "Posted", Is_Active = true, Sort_Order = 30 },
            new VoucherStatus { Voucher_Status_ID = 4, Voucher_Status_Code = "CANCELLED", Voucher_Status_Name_AR = "ملغي", Voucher_Status_Name_EN = "Cancelled", Is_Active = true, Sort_Order = 40 },
            new VoucherStatus { Voucher_Status_ID = 5, Voucher_Status_Code = "REJECTED", Voucher_Status_Name_AR = "مرفوض", Voucher_Status_Name_EN = "Rejected", Is_Active = true, Sort_Order = 50 });

        modelBuilder.Entity<PaymentMethod>().HasData(
            new PaymentMethod { Payment_Method_ID = 1, Payment_Method_Code = "CASH", Payment_Method_Name_AR = "نقدي", Payment_Method_Name_EN = "Cash", Is_Cash = true, Is_Bank = false, Is_Active = true, Sort_Order = 10 },
            new PaymentMethod { Payment_Method_ID = 2, Payment_Method_Code = "BANK_TRANSFER", Payment_Method_Name_AR = "تحويل بنكي", Payment_Method_Name_EN = "Bank Transfer", Requires_Reference = true, Requires_Reference_Date = true, Is_Cash = false, Is_Bank = true, Is_Active = true, Sort_Order = 20 },
            new PaymentMethod { Payment_Method_ID = 3, Payment_Method_Code = "CHEQUE", Payment_Method_Name_AR = "شيك", Payment_Method_Name_EN = "Cheque", Requires_Reference = true, Requires_Reference_Date = true, Is_Cash = false, Is_Bank = true, Is_Active = true, Sort_Order = 30 });

        modelBuilder.Entity<ApprovalStatusReference>().HasData(
            new ApprovalStatusReference { Approval_Status_ID = 1, Approval_Status_Code = "PENDING", Approval_Status_Name_AR = "بانتظار الاعتماد", Approval_Status_Name_EN = "Pending", Sort_Order = 10, Is_Active = true },
            new ApprovalStatusReference { Approval_Status_ID = 2, Approval_Status_Code = "UNDER_REVIEW", Approval_Status_Name_AR = "تحت المراجعة", Approval_Status_Name_EN = "Under Review", Sort_Order = 20, Is_Active = true },
            new ApprovalStatusReference { Approval_Status_ID = 3, Approval_Status_Code = "APPROVED", Approval_Status_Name_AR = "معتمد", Approval_Status_Name_EN = "Approved", Sort_Order = 30, Is_Active = true },
            new ApprovalStatusReference { Approval_Status_ID = 4, Approval_Status_Code = "REJECTED", Approval_Status_Name_AR = "مرفوض", Approval_Status_Name_EN = "Rejected", Sort_Order = 40, Is_Active = true },
            new ApprovalStatusReference { Approval_Status_ID = 5, Approval_Status_Code = "RETURNED", Approval_Status_Name_AR = "معاد للتعديل", Approval_Status_Name_EN = "Returned", Sort_Order = 50, Is_Active = true });

        modelBuilder.Entity<DocumentTypeReference>().HasData(
            new DocumentTypeReference { Document_Type_ID = 1, Document_Type_Code = "RECEIPT_VOUCHER", Document_Type_Name_AR = "سند قبض", Document_Type_Name_EN = "Receipt Voucher", Sort_Order = 10, Is_Active = true },
            new DocumentTypeReference { Document_Type_ID = 2, Document_Type_Code = "PAYMENT_VOUCHER", Document_Type_Name_AR = "سند صرف", Document_Type_Name_EN = "Payment Voucher", Sort_Order = 20, Is_Active = true },
            new DocumentTypeReference { Document_Type_ID = 3, Document_Type_Code = "JOURNAL_ENTRY", Document_Type_Name_AR = "قيد يومية", Document_Type_Name_EN = "Journal Entry", Sort_Order = 30, Is_Active = true },
            new DocumentTypeReference { Document_Type_ID = 4, Document_Type_Code = "PAYMENT_REQUEST", Document_Type_Name_AR = "طلب صرف", Document_Type_Name_EN = "Payment Request", Sort_Order = 40, Is_Active = true });

        modelBuilder.Entity<SystemPermission>().HasData(
            Permission(1, "VIEW", "عرض", "DATA", 10),
            Permission(2, "ADD", "إضافة", "DATA", 20),
            Permission(3, "EDIT", "تعديل", "DATA", 30),
            Permission(4, "DELETE", "إيقاف", "DATA", 40),
            Permission(5, "PRINT", "طباعة", "REPORT", 50),
            Permission(6, "EXPORT", "تصدير", "REPORT", 60),
            Permission(7, "IMPORT", "استيراد", "DATA", 70),
            Permission(8, "APPROVE", "اعتماد", "EXTRA", 80),
            Permission(9, "UNAPPROVE", "إلغاء اعتماد", "EXTRA", 90));

        modelBuilder.Entity<SystemScreen>().HasData(
            Screen(1001, "TenantGroups", "المجموعات التجارية", "الهيكل المؤسسي", 10),
            Screen(1002, "Companies", "الشركات", "الهيكل المؤسسي", 20),
            Screen(1003, "Branches", "الفروع", "الهيكل المؤسسي", 30),
            Screen(1004, "Countries", "الدول", "الهيكل المؤسسي", 40),
            Screen(1005, "Governorates", "المحافظات", "الهيكل المؤسسي", 50),
            Screen(1006, "Cities", "المدن", "الهيكل المؤسسي", 60),
            Screen(1101, "Users", "المستخدمون", "المستخدمون والصلاحيات", 10),
            Screen(1102, "Roles", "الأدوار", "المستخدمون والصلاحيات", 20),
            Screen(1103, "RolePermissions", "صلاحيات الأدوار", "المستخدمون والصلاحيات", 30),
            Screen(1201, "AuditLogs", "سجل التدقيق والرقابة", "الأمن والرقابة", 10),
            Screen(1301, "ReceiptVoucher", "سند القبض", "النظام المالي", 10),
            Screen(1302, "PaymentVoucher", "سند الصرف", "النظام المالي", 20),
            Screen(1303, "JournalVoucher", "قيد اليومية", "النظام المالي", 30),
            Screen(1304, "PaymentRequest", "طلب الصرف", "النظام المالي", 40));
    }

    private static SystemPermission Permission(int id, string code, string name, string type, int order) =>
        new() { Permission_ID = id, Permission_Code = code, Permission_Name = name, Permission_Type = type, Is_Active = true, Sort_Order = order, Created_At = new DateTime(2026, 1, 1) };

    private static SystemScreen Screen(int id, string code, string name, string module, int order) =>
        new() { Screen_ID = id, Screen_Code = code, Screen_Name = name, Module_Name = module, Is_Active = true, Sort_Order = order, Created_At = new DateTime(2026, 1, 1) };

    private static void ApplyGlobalConventions(ModelBuilder modelBuilder)
    {
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            string tableName = entityType.GetTableName() ?? entityType.ClrType.Name;
            entityType.SetTableName(ToSnakeCase(tableName));

            foreach (IMutableProperty property in entityType.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.GetColumnName(StoreObjectIdentifier.Table(entityType.GetTableName()!, entityType.GetSchema())) ?? property.Name));

                if (property.ClrType == typeof(string) || property.ClrType == typeof(string?))
                {
                    if (!string.Equals(property.GetColumnType(), "json", StringComparison.OrdinalIgnoreCase))
                    {
                        property.SetCollation(Phase1ModelCustomizer.Collation);
                        if (property.GetMaxLength() is null)
                            property.SetMaxLength(InferStringLength(property.Name));
                    }
                }

                Type numericType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                if (numericType == typeof(decimal) && property.GetPrecision() is null)
                {
                    bool isRate = property.Name.Contains("Rate", StringComparison.OrdinalIgnoreCase);
                    property.SetPrecision(19);
                    property.SetScale(isRate ? 8 : 4);
                }

                if (property.Name is "Created_At" or "Created_Date")
                    property.SetDefaultValueSql("CURRENT_TIMESTAMP(6)");
                else if (property.Name == "Is_Active")
                    property.SetDefaultValue(true);
                else if (property.Name is "Sort_Order" or "Edit_Count" or "Last_Number")
                    property.SetDefaultValue(0);
            }

            foreach (IMutableKey key in entityType.GetKeys())
                key.SetName(ToSnakeCase(key.GetName() ?? $"pk_{entityType.GetTableName()}"));

            foreach (IMutableIndex index in entityType.GetIndexes())
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName() ?? $"ix_{entityType.GetTableName()}_{string.Join('_', index.Properties.Select(x => x.Name))}"));

            foreach (IMutableForeignKey foreignKey in entityType.GetForeignKeys())
                foreignKey.SetConstraintName(ToSnakeCase(foreignKey.GetConstraintName() ?? $"fk_{entityType.GetTableName()}_{foreignKey.PrincipalEntityType.GetTableName()}"));
        }
    }

    private static int InferStringLength(string propertyName)
    {
        if (propertyName.EndsWith("_ID", StringComparison.OrdinalIgnoreCase) || propertyName.EndsWith("Id", StringComparison.OrdinalIgnoreCase)) return 50;
        if (propertyName.Contains("Code", StringComparison.OrdinalIgnoreCase)) return 50;
        if (propertyName.Contains("Email", StringComparison.OrdinalIgnoreCase)) return 254;
        if (propertyName.Contains("Phone", StringComparison.OrdinalIgnoreCase) || propertyName.Contains("Mobile", StringComparison.OrdinalIgnoreCase)) return 30;
        if (propertyName.Contains("Name", StringComparison.OrdinalIgnoreCase)) return 200;
        if (propertyName.Contains("Reason", StringComparison.OrdinalIgnoreCase) || propertyName.Contains("Description", StringComparison.OrdinalIgnoreCase) || propertyName.Contains("Notes", StringComparison.OrdinalIgnoreCase)) return 1000;
        if (propertyName.Contains("Path", StringComparison.OrdinalIgnoreCase) || propertyName.Contains("Storage", StringComparison.OrdinalIgnoreCase)) return 500;
        return 250;
    }

    private static string ToSnakeCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        var result = new StringBuilder(value.Length + 8);
        for (int index = 0; index < value.Length; index++)
        {
            char current = value[index];
            if (char.IsUpper(current) && index > 0 && value[index - 1] != '_') result.Append('_');
            result.Append(char.ToLowerInvariant(current));
        }
        return result.ToString().Replace("__", "_", StringComparison.Ordinal);
    }
}
