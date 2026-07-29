using System.Text;
using AlTayerERP.Core.Entities;
using AlTayerERP.Core.Entities.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AlTayerERP.Infrastructure.Data;

/// <summary>
/// المصدر المركزي لقواعد مخطط المرحلة الأولى الجديدة.
/// لا ينفذ SQL ولا يتصل بقاعدة البيانات؛ يعرّف نموذج EF Core فقط.
/// </summary>
public static class Phase1BaselineModelConfiguration
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
        ConfigureReferenceSeeds(modelBuilder);
        ApplyGlobalConventions(modelBuilder);
    }

    private static void ConfigureInstitutional(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TenantGroup>(entity =>
        {
            entity.ToTable("tenant_groups");
            entity.HasKey(x => x.Group_ID);
            entity.Property(x => x.Group_ID).HasMaxLength(50).ValueGeneratedNever();
            entity.Property(x => x.Group_Code).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Group_Name_AR).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Group_Name_EN).HasMaxLength(150);
            entity.Property(x => x.Short_Name).HasMaxLength(100);
            entity.Property(x => x.Parent_Group_ID).HasMaxLength(50);
            entity.Property(x => x.Main_Company_ID).HasMaxLength(50);
            entity.Property(x => x.Default_Currency_Code).HasMaxLength(10);
            entity.Property(x => x.Country_Name).HasMaxLength(100);
            entity.Property(x => x.City_Name).HasMaxLength(100);
            entity.Property(x => x.Short_Address).HasMaxLength(300);
            entity.Property(x => x.Phone).HasMaxLength(50);
            entity.Property(x => x.Email).HasMaxLength(150);
            entity.Property(x => x.Manager_Name).HasMaxLength(150);
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.Is_Active).HasDefaultValue(true);
            entity.Property(x => x.Show_In_Login).HasDefaultValue(true);
            entity.Property(x => x.Show_In_Tree).HasDefaultValue(true);
            entity.Property(x => x.Edit_Count).HasDefaultValue(0);
            entity.HasIndex(x => x.Group_Code).IsUnique();
            entity.HasIndex(x => new { x.Show_In_Login, x.Is_Active, x.Sort_Order });
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("companies");
            entity.HasKey(x => x.Company_ID);
            entity.Property(x => x.Company_ID).HasMaxLength(50).ValueGeneratedNever();
            entity.Property(x => x.Group_ID).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Company_Name_AR).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Company_Name_EN).HasMaxLength(200);
            entity.Property(x => x.Company_Prefix).HasMaxLength(20);
            entity.Property(x => x.Activity_Type).HasMaxLength(100);
            entity.Property(x => x.Tax_Number).HasMaxLength(100);
            entity.Property(x => x.Phone).HasMaxLength(50);
            entity.Property(x => x.Mobile).HasMaxLength(50);
            entity.Property(x => x.Email).HasMaxLength(150);
            entity.Property(x => x.Address).HasMaxLength(500);
            entity.Property(x => x.Is_Active).HasDefaultValue(true);
            entity.Property(x => x.Edit_Count).HasDefaultValue(0);
            entity.HasOne<TenantGroup>().WithMany().HasForeignKey(x => x.Group_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Group_ID, x.Is_Active });
        });

        modelBuilder.Entity<TenantBranch>(entity =>
        {
            entity.ToTable("tenant_branches");
            entity.HasKey(x => x.Branch_ID);
            entity.Property(x => x.Company_ID).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Branch_Code).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Branch_Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Branch_Name_EN).HasMaxLength(200);
            entity.Property(x => x.Address).HasMaxLength(500);
            entity.Property(x => x.Branch_Type).HasMaxLength(30);
            entity.Property(x => x.Phone).HasMaxLength(50);
            entity.Property(x => x.Mobile).HasMaxLength(50);
            entity.Property(x => x.Email).HasMaxLength(150);
            entity.Property(x => x.Website).HasMaxLength(200);
            entity.Property(x => x.Manager_Name).HasMaxLength(150);
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.Is_Active).HasDefaultValue(true);
            entity.Property(x => x.Edit_Count).HasDefaultValue(0);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ParentBranch).WithMany().HasForeignKey(x => x.Parent_Branch_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Currency>().WithMany().HasForeignKey(x => x.Currency_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Country>().WithMany().HasForeignKey(x => x.Country_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Governorate>().WithMany().HasForeignKey(x => x.Governorate_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<City>().WithMany().HasForeignKey(x => x.City_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<BranchType>().WithMany().HasForeignKey(x => x.Branch_Type_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Company_ID, x.Branch_Code }).IsUnique();
            entity.HasIndex(x => x.Country_ID);
            entity.HasIndex(x => x.Governorate_ID);
            entity.HasIndex(x => x.City_ID);
        });

        modelBuilder.Entity<FiscalYear>(entity =>
        {
            entity.ToTable("fiscal_years");
            entity.HasKey(x => x.Fiscal_Year_ID);
            entity.Property(x => x.Company_ID).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Year_Name).HasMaxLength(100).IsRequired();
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Company_ID, x.Year_Name }).IsUnique();
            entity.ToTable(t => t.HasCheckConstraint("ck_fiscal_year_dates", "`start_date` <= `end_date`"));
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.ToTable("countries");
            entity.HasKey(x => x.Country_ID);
            entity.HasIndex(x => x.Country_Code).IsUnique();
            entity.HasIndex(x => x.ISO2).IsUnique();
            entity.HasIndex(x => x.ISO3).IsUnique();
        });

        modelBuilder.Entity<Governorate>(entity =>
        {
            entity.ToTable("governorates");
            entity.HasKey(x => x.Governorate_ID);
            entity.HasOne(x => x.Country).WithMany(x => x.Governorates).HasForeignKey(x => x.Country_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Country_ID, x.Governorate_Code }).IsUnique();
            entity.HasIndex(x => new { x.Country_ID, x.Governorate_Name_AR }).IsUnique();
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.ToTable("cities");
            entity.HasKey(x => x.City_ID);
            entity.HasOne(x => x.Country).WithMany().HasForeignKey(x => x.Country_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Governorate).WithMany(x => x.Cities).HasForeignKey(x => x.Governorate_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Governorate_ID, x.City_Code }).IsUnique();
            entity.HasIndex(x => new { x.Governorate_ID, x.City_Name_AR }).IsUnique();
        });

        modelBuilder.Entity<BranchType>(entity =>
        {
            entity.ToTable("branch_types");
            entity.HasKey(x => x.Branch_Type_ID);
            entity.HasIndex(x => x.Branch_Type_Code).IsUnique();
            entity.HasIndex(x => new { x.Is_Active, x.Sort_Order });
        });
    }

    private static void ConfigureSecurity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(x => x.Role_ID);
            entity.Property(x => x.Role_Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Role_Code).HasMaxLength(50);
            entity.HasIndex(x => x.Role_Code).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(x => x.User_ID);
            entity.Property(x => x.Company_ID).HasMaxLength(50).IsRequired();
            entity.Property(x => x.User_Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Login_Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Full_Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Password_Hash).HasMaxLength(500).IsRequired();
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Role>().WithMany().HasForeignKey(x => x.Role_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.Login_Name).IsUnique();
            entity.HasIndex(x => new { x.Company_ID, x.User_Code }).IsUnique();
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("role_permissions");
            entity.HasKey(x => x.Permission_ID);
            entity.HasOne<Role>().WithMany().HasForeignKey(x => x.Role_ID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<SystemScreen>().WithMany().HasForeignKey(x => x.Screen_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Role_ID, x.Screen_ID }).IsUnique();
        });

        modelBuilder.Entity<UserPermission>(entity =>
        {
            entity.ToTable("user_permissions");
            entity.HasKey(x => x.Permission_ID);
            entity.Property(x => x.Permission_Category).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Permission_Name).HasMaxLength(100).IsRequired();
            entity.HasOne<User>().WithMany().HasForeignKey(x => x.User_ID).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.User_ID, x.Permission_Category, x.Permission_Name }).IsUnique();
        });

        modelBuilder.Entity<SystemPermission>(entity =>
        {
            entity.ToTable("system_permissions");
            entity.HasKey(x => x.Permission_ID);
            entity.HasIndex(x => x.Permission_Code).IsUnique();
        });

        modelBuilder.Entity<SystemScreen>(entity =>
        {
            entity.ToTable("system_screens");
            entity.HasKey(x => x.Screen_ID);
            entity.Property(x => x.Screen_Code).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Screen_Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Module_Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.Screen_Code).IsUnique();
        });

        modelBuilder.Entity<LoginAttempt>(entity =>
        {
            entity.ToTable("login_attempts");
            entity.HasKey(x => x.Login_Attempt_ID);
            entity.Property(x => x.Login_Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Company_ID).HasMaxLength(50);
            entity.HasOne<User>().WithMany().HasForeignKey(x => x.User_ID).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.Login_Name, x.Attempted_At });
            entity.HasIndex(x => new { x.User_ID, x.Attempted_At });
            entity.HasIndex(x => x.Session_ID);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(x => x.Refresh_Token_ID);
            entity.Property(x => x.Token_Hash).HasMaxLength(64).IsFixedLength().IsRequired();
            entity.Property(x => x.Session_ID).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Company_ID).HasMaxLength(50).IsRequired();
            entity.HasOne<User>().WithMany().HasForeignKey(x => x.User_ID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<FiscalYear>().WithMany().HasForeignKey(x => x.Fiscal_Year_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.Token_Hash).IsUnique();
            entity.HasIndex(x => new { x.User_ID, x.Expires_At });
            entity.HasIndex(x => x.Session_ID);
        });
    }

    private static void ConfigureSettings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.ToTable("system_settings");
            entity.HasKey(x => x.Setting_ID);
            entity.HasIndex(x => new { x.Setting_Key, x.Scope, x.Company_ID, x.Branch_ID, x.Fiscal_Year_ID }).IsUnique();
            entity.ToTable(t => t.HasCheckConstraint("ck_system_settings_scope", "`scope` IN ('SYSTEM','COMPANY','BRANCH','FISCAL_YEAR')"));
        });

        modelBuilder.Entity<FiscalPeriod>(entity =>
        {
            entity.ToTable("fiscal_periods");
            entity.HasKey(x => x.Fiscal_Period_ID);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<FiscalYear>().WithMany().HasForeignKey(x => x.Fiscal_Year_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Company_ID, x.Branch_ID, x.Fiscal_Year_ID, x.Period_Code }).IsUnique();
            entity.ToTable(t => t.HasCheckConstraint("ck_fiscal_period_dates", "`start_date` <= `end_date`"));
        });

        modelBuilder.Entity<ExchangeRate>(entity =>
        {
            entity.ToTable("exchange_rates");
            entity.HasKey(x => x.Exchange_Rate_ID);
            entity.Property(x => x.Exchange_Rate_Value).HasPrecision(18, 6);
            entity.Property(x => x.Min_Rate).HasPrecision(18, 6);
            entity.Property(x => x.Max_Rate).HasPrecision(18, 6);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Company_ID, x.Currency_Code, x.Rate_Date }).IsUnique();
            entity.ToTable(t => t.HasCheckConstraint("ck_exchange_rate_positive", "`exchange_rate_value` > 0"));
        });

        modelBuilder.Entity<NumberingSetting>(entity =>
        {
            entity.ToTable("numbering_settings");
            entity.HasKey(x => x.Numbering_ID);
            entity.Property(x => x.Document_Type).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Prefix).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Reset_Type).HasMaxLength(30).IsRequired();
            entity.HasIndex(x => x.Document_Type).IsUnique();
        });

        modelBuilder.Entity<NumberingCounter>(entity =>
        {
            entity.ToTable("numbering_counters");
            entity.HasKey(x => x.Counter_ID);
            entity.Property(x => x.Document_Type).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Company_ID).HasMaxLength(50).HasDefaultValue(string.Empty);
            entity.Property(x => x.Branch_ID).HasDefaultValue(0);
            entity.Property(x => x.Year_Value).HasDefaultValue(0);
            entity.HasIndex(x => new { x.Document_Type, x.Company_ID, x.Branch_ID, x.Year_Value }).IsUnique();
        });

        modelBuilder.Entity<FinancialPolicy>(entity =>
        {
            entity.ToTable("financial_limits");
            entity.HasKey(x => x.Limit_ID);
            entity.Property(x => x.Company_ID).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Entity_Type).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Entity_ID).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Limit_Type).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Currency_Code).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Limit_Amount).HasPrecision(19, 4);
            entity.Property(x => x.Used_Amount).HasPrecision(19, 4);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FinancialPolicyMovement>(entity =>
        {
            entity.ToTable("financial_limit_movements");
            entity.HasKey(x => x.Movement_ID);
            entity.Property(x => x.Company_ID).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(19, 4);
            entity.Property(x => x.Balance_After).HasPrecision(19, 4);
            entity.HasOne<FinancialPolicy>().WithMany().HasForeignKey(x => x.Limit_ID).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ApprovalRequest>(entity =>
        {
            entity.ToTable("approval_requests");
            entity.HasKey(x => x.Approval_ID);
            entity.Property(x => x.Company_ID).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ApprovalStatusReference>(entity =>
        {
            entity.ToTable("approval_statuses");
            entity.HasKey(x => x.Approval_Status_ID);
            entity.HasIndex(x => x.Approval_Status_Code).IsUnique();
        });

        modelBuilder.Entity<NumberingDocumentType>(entity =>
        {
            entity.ToTable("numbering_document_types");
            entity.HasKey(x => x.Numbering_Document_Type_ID);
            entity.HasIndex(x => x.Document_Type_Code).IsUnique();
        });
    }

    private static void ConfigureAccountingMasters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountCategory>(entity =>
        {
            entity.ToTable("account_categories");
            entity.HasKey(x => x.Category_ID);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Company_ID, x.Category_Code }).IsUnique();
        });

        modelBuilder.Entity<AccountCodeSetting>(entity =>
        {
            entity.ToTable("account_code_settings");
            entity.HasKey(x => x.Setting_ID);
            entity.Property(x => x.Company_ID).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Padding_Char).HasMaxLength(1).IsRequired();
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Company_ID, x.Level_No }).IsUnique();
        });

        modelBuilder.Entity<ChartOfAccount>(entity =>
        {
            entity.ToTable("chart_of_accounts");
            entity.HasKey(x => x.Account_ID);
            entity.Property(x => x.Account_ID).HasMaxLength(50).ValueGeneratedNever();
            entity.Property(x => x.Company_ID).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Parent_Account_ID).HasMaxLength(50);
            entity.Property(x => x.Account_Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Account_Name_AR).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Account_Name_EN).HasMaxLength(200);
            entity.Property(x => x.Account_Type).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Currency_Code).HasMaxLength(20);
            entity.Property(x => x.Account_Category).HasMaxLength(50);
            entity.Property(x => x.Normal_Balance).HasMaxLength(10);
            entity.Property(x => x.Control_Account_Type).HasMaxLength(30);
            entity.Property(x => x.Account_Path).HasMaxLength(1000);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Parent_Account_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Company_ID, x.Account_Code }).IsUnique();
            entity.HasIndex(x => new { x.Company_ID, x.Is_Control_Account, x.Control_Account_Type });
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("ck_chart_normal_balance", "`normal_balance` IS NULL OR `normal_balance` IN ('Debit','Credit')");
                t.HasCheckConstraint("ck_chart_postable_summary", "NOT (`is_postable` = 1 AND `is_summary_account` = 1)");
            });
        });

        modelBuilder.Entity<CostCenter>(entity =>
        {
            entity.ToTable("cost_centers");
            entity.HasKey(x => x.Cost_Center_ID);
            entity.Property(x => x.Cost_Center_ID).HasMaxLength(50).ValueGeneratedNever();
            entity.Property(x => x.Company_ID).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Parent_Cost_Center_ID).HasMaxLength(50);
            entity.Property(x => x.Center_Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Center_Name_AR).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Center_Name_EN).HasMaxLength(200);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<CostCenter>().WithMany().HasForeignKey(x => x.Parent_Cost_Center_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Company_ID, x.Center_Code }).IsUnique();
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.ToTable("currencies");
            entity.HasKey(x => x.Currency_ID);
            entity.Property(x => x.Company_ID).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Currency_Code).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Currency_Name_AR).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Exchange_Rate).HasPrecision(18, 6);
            entity.Property(x => x.Min_Exchange_Rate).HasPrecision(18, 6);
            entity.Property(x => x.Max_Exchange_Rate).HasPrecision(18, 6);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Company_ID, x.Currency_Code }).IsUnique();
        });

        modelBuilder.Entity<CashBox>(entity =>
        {
            entity.ToTable("cash_boxes");
            entity.HasKey(x => x.Cash_Box_ID);
            entity.Property(x => x.Cash_Box_ID).HasMaxLength(50).ValueGeneratedNever();
            entity.Property(x => x.Company_ID).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Account_ID).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Currency_Code).HasMaxLength(20).IsRequired();
            entity.Property(x => x.CashBox_Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Box_Name_AR).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Opening_Balance).HasPrecision(19, 4);
            entity.Property(x => x.Max_Limit).HasPrecision(19, 4);
            entity.Property(x => x.Min_Limit).HasPrecision(19, 4);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Account_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Company_ID, x.Branch_ID, x.CashBox_Code }).IsUnique();
        });

        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.ToTable("bank_accounts");
            entity.HasKey(x => x.Bank_Account_ID);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Company_ID, x.Account_No }).IsUnique();
        });

        modelBuilder.Entity<Party>(entity =>
        {
            entity.ToTable("parties");
            entity.HasKey(x => x.Party_ID);
            entity.Property(x => x.Party_ID).HasMaxLength(50).ValueGeneratedNever();
            entity.Property(x => x.Credit_Limit).HasPrecision(18, 2);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Account_ID).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.Company_ID, x.Party_Code }).IsUnique();
        });
    }

    private static void ConfigureFinancialEngine(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VoucherType>(entity =>
        {
            entity.ToTable("voucher_types");
            entity.HasKey(x => x.Voucher_Type_ID);
            entity.HasIndex(x => x.Voucher_Type_Code).IsUnique();
        });
        modelBuilder.Entity<VoucherStatus>(entity =>
        {
            entity.ToTable("voucher_statuses");
            entity.HasKey(x => x.Voucher_Status_ID);
            entity.HasIndex(x => x.Voucher_Status_Code).IsUnique();
        });
        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.ToTable("payment_methods");
            entity.HasKey(x => x.Payment_Method_ID);
            entity.HasIndex(x => x.Payment_Method_Code).IsUnique();
        });

        modelBuilder.Entity<FinancialVoucherHeader>(entity =>
        {
            entity.ToTable("financial_voucher_headers");
            entity.HasKey(x => x.Voucher_ID);
            entity.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<FiscalYear>().WithMany().HasForeignKey(x => x.Fiscal_Year_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<VoucherType>().WithMany().HasForeignKey(x => x.Voucher_Type_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<VoucherStatus>().WithMany().HasForeignKey(x => x.Voucher_Status_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Currency>().WithMany().HasForeignKey(x => x.Currency_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Party>().WithMany().HasForeignKey(x => x.Party_ID).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<PaymentMethod>().WithMany().HasForeignKey(x => x.Payment_Method_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Cash_Account_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Branch_ID, x.Fiscal_Year_ID, x.Voucher_Type_ID, x.Voucher_No }).IsUnique();
            entity.HasIndex(x => x.Voucher_Date);
            entity.HasIndex(x => x.Is_Posted);
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("ck_voucher_exchange_rate", "`exchange_rate` > 0");
                t.HasCheckConstraint("ck_voucher_totals", "`amount` >= 0 AND `foreign_total` >= 0 AND `local_total` >= 0");
            });
        });

        modelBuilder.Entity<FinancialVoucherDetail>(entity =>
        {
            entity.ToTable("financial_voucher_details");
            entity.HasKey(x => x.Voucher_Detail_ID);
            entity.HasOne(x => x.Voucher).WithMany(x => x.Details).HasForeignKey(x => x.Voucher_ID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Account_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<CostCenter>().WithMany().HasForeignKey(x => x.Cost_Center_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Currency>().WithMany().HasForeignKey(x => x.Currency_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Voucher_ID, x.Line_No }).IsUnique();
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("ck_voucher_detail_rate", "`exchange_rate` > 0");
                t.HasCheckConstraint("ck_voucher_detail_debit_credit", "NOT (`debit_amount` > 0 AND `credit_amount` > 0)");
                t.HasCheckConstraint("ck_voucher_detail_nonnegative", "`debit_amount` >= 0 AND `credit_amount` >= 0 AND `foreign_amount` >= 0 AND `local_amount` >= 0");
            });
        });

        modelBuilder.Entity<JournalEntryHeader>(entity =>
        {
            entity.ToTable("journal_entry_headers");
            entity.HasKey(x => x.Journal_Entry_ID);
            entity.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<FiscalYear>().WithMany().HasForeignKey(x => x.Fiscal_Year_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<FinancialVoucherHeader>().WithMany().HasForeignKey(x => x.Source_Voucher_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.Entry_No).IsUnique();
            entity.HasIndex(x => x.Entry_Date);
            entity.HasIndex(x => x.Source_Voucher_ID);
            entity.ToTable(t => t.HasCheckConstraint("ck_journal_balanced", "`total_debit` = `total_credit`"));
        });

        modelBuilder.Entity<JournalEntryDetail>(entity =>
        {
            entity.ToTable("journal_entry_details");
            entity.HasKey(x => x.Journal_Entry_Detail_ID);
            entity.HasOne(x => x.JournalEntry).WithMany(x => x.Details).HasForeignKey(x => x.Journal_Entry_ID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Account_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<CostCenter>().WithMany().HasForeignKey(x => x.Cost_Center_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Currency>().WithMany().HasForeignKey(x => x.Currency_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Journal_Entry_ID, x.Line_No }).IsUnique();
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("ck_journal_detail_rate", "`exchange_rate` > 0");
                t.HasCheckConstraint("ck_journal_detail_debit_credit", "NOT (`debit_amount` > 0 AND `credit_amount` > 0)");
            });
        });

        modelBuilder.Entity<DocumentAllocation>(entity =>
        {
            entity.ToTable("document_allocations");
            entity.HasKey(x => x.Allocation_ID);
            entity.HasOne(x => x.Voucher).WithMany(x => x.DocumentAllocations).HasForeignKey(x => x.Voucher_ID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Party>().WithMany().HasForeignKey(x => x.Party_ID).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<Currency>().WithMany().HasForeignKey(x => x.Currency_ID).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DocumentLink>(entity =>
        {
            entity.ToTable("document_links");
            entity.HasKey(x => x.Document_Link_ID);
            entity.HasIndex(x => new { x.From_Module_ID, x.From_Document_Type_ID, x.From_Document_ID });
            entity.HasIndex(x => new { x.To_Module_ID, x.To_Document_Type_ID, x.To_Document_ID });
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");
            entity.HasKey(x => x.Audit_ID);
            entity.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.Table_Name, x.Record_ID, x.Action_At });
            entity.ToTable(t => t.HasCheckConstraint("ck_audit_action", "CHAR_LENGTH(`action_type`) BETWEEN 1 AND 64 AND `action_type` = UPPER(`action_type`)"));
        });

        modelBuilder.Entity<VoucherActionLog>(entity =>
        {
            entity.ToTable("voucher_action_logs");
            entity.HasKey(x => x.Voucher_Action_ID);
            entity.HasOne(x => x.Voucher).WithMany(x => x.VoucherActionLogs).HasForeignKey(x => x.Voucher_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Voucher_ID, x.Action_At });
        });

        modelBuilder.Entity<PaymentRequest>(entity =>
        {
            entity.ToTable("payment_requests");
            entity.HasKey(x => x.Payment_Request_ID);
            entity.Property(x => x.Company_ID).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Request_No).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Beneficiary_Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Party_ID).HasMaxLength(50);
            entity.Property(x => x.Header_Reference_No).HasMaxLength(100);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.Review_Reason).HasMaxLength(500);
            entity.Property(x => x.Approval_Reason).HasMaxLength(500);
            entity.Property(x => x.Created_By).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Updated_By).HasMaxLength(50);
            entity.Property(x => x.Approved_Local_Total).HasPrecision(19, 4);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<FiscalYear>().WithMany().HasForeignKey(x => x.Fiscal_Year_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Party>().WithMany().HasForeignKey(x => x.Party_ID).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<PaymentMethod>().WithMany().HasForeignKey(x => x.Payment_Method_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<FinancialVoucherHeader>().WithMany().HasForeignKey(x => x.Payment_Voucher_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Company_ID, x.Branch_ID, x.Fiscal_Year_ID, x.Request_No }).IsUnique();
            entity.HasIndex(x => new { x.Company_ID, x.Branch_ID, x.Fiscal_Year_ID, x.Status });
        });

        modelBuilder.Entity<PaymentRequestLine>(entity =>
        {
            entity.ToTable("payment_request_lines");
            entity.HasKey(x => x.Payment_Request_Line_ID);
            entity.Property(x => x.Exchange_Rate).HasPrecision(19, 8);
            entity.Property(x => x.Foreign_Amount).HasPrecision(19, 4);
            entity.Property(x => x.Local_Amount).HasPrecision(19, 4);
            entity.Property(x => x.Account_ID).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Cost_Center_ID).HasMaxLength(50);
            entity.Property(x => x.Reference_No).HasMaxLength(100);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.HasOne(x => x.PaymentRequest).WithMany(x => x.Details).HasForeignKey(x => x.Payment_Request_ID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ChartOfAccount>().WithMany().HasForeignKey(x => x.Account_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<CostCenter>().WithMany().HasForeignKey(x => x.Cost_Center_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Currency>().WithMany().HasForeignKey(x => x.Currency_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Payment_Request_ID, x.Line_No }).IsUnique();
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("ck_payment_request_line_rate", "`exchange_rate` > 0");
                t.HasCheckConstraint("ck_payment_request_line_amounts", "`foreign_amount` >= 0 AND `local_amount` >= 0");
            });
        });

        modelBuilder.Entity<PaymentRequestAttachment>(entity =>
        {
            entity.ToTable("payment_request_attachments");
            entity.HasKey(x => x.Payment_Request_Attachment_ID);
            entity.Property(x => x.Company_ID).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Original_File_Name).HasMaxLength(260).IsRequired();
            entity.Property(x => x.Storage_Key).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Content_Type).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Created_By).HasMaxLength(50).IsRequired();
            entity.HasOne<PaymentRequest>().WithMany().HasForeignKey(x => x.Payment_Request_ID).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.Company_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<TenantBranch>().WithMany().HasForeignKey(x => x.Branch_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<FiscalYear>().WithMany().HasForeignKey(x => x.Fiscal_Year_ID).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.Payment_Request_ID, x.Is_Active });
        });
    }

    private static void ConfigureReferenceSeeds(ModelBuilder modelBuilder)
    {
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<BranchType>().HasData(
            new BranchType { Branch_Type_ID = 1, Branch_Type_Code = "MAIN", Branch_Type_Name_AR = "رئيسي", Branch_Type_Name_EN = "Main", Sort_Order = 10, Is_Active = true, Created_At = seedDate },
            new BranchType { Branch_Type_ID = 2, Branch_Type_Code = "OPERATING", Branch_Type_Name_AR = "تشغيلي", Branch_Type_Name_EN = "Operating", Sort_Order = 20, Is_Active = true, Created_At = seedDate },
            new BranchType { Branch_Type_ID = 3, Branch_Type_Code = "DISTRIBUTION", Branch_Type_Name_AR = "نقطة توزيع", Branch_Type_Name_EN = "Distribution Point", Sort_Order = 30, Is_Active = true, Created_At = seedDate },
            new BranchType { Branch_Type_ID = 4, Branch_Type_Code = "WAREHOUSE", Branch_Type_Name_AR = "مستودع", Branch_Type_Name_EN = "Warehouse", Sort_Order = 40, Is_Active = true, Created_At = seedDate });

        modelBuilder.Entity<VoucherType>().HasData(
            new VoucherType { Voucher_Type_ID = 1, Voucher_Type_Code = "RECEIPT", Voucher_Type_Name_AR = "سند قبض", Voucher_Type_Name_EN = "Receipt Voucher", Is_Active = true, Sort_Order = 1 },
            new VoucherType { Voucher_Type_ID = 2, Voucher_Type_Code = "PAYMENT", Voucher_Type_Name_AR = "سند صرف", Voucher_Type_Name_EN = "Payment Voucher", Is_Active = true, Sort_Order = 2 },
            new VoucherType { Voucher_Type_ID = 3, Voucher_Type_Code = "JOURNAL", Voucher_Type_Name_AR = "قيد يومية", Voucher_Type_Name_EN = "Journal Voucher", Is_Active = true, Sort_Order = 3 },
            new VoucherType { Voucher_Type_ID = 4, Voucher_Type_Code = "ADJUSTMENT", Voucher_Type_Name_AR = "قيد تسوية", Voucher_Type_Name_EN = "Adjustment Voucher", Is_Active = true, Sort_Order = 4 },
            new VoucherType { Voucher_Type_ID = 5, Voucher_Type_Code = "OPENING", Voucher_Type_Name_AR = "قيد افتتاحي", Voucher_Type_Name_EN = "Opening Voucher", Is_Active = true, Sort_Order = 5 });

        modelBuilder.Entity<VoucherStatus>().HasData(
            Status(1, "DRAFT", "مسودة", 1), Status(2, "PENDING", "معلق", 2),
            Status(3, "REVIEWED", "تمت المراجعة", 3), Status(4, "RETURNED", "معاد للتصحيح", 4),
            Status(5, "APPROVED", "معتمد", 5), Status(6, "POSTED", "مرحل", 6),
            Status(7, "CANCELLED", "ملغي", 7), Status(8, "REVERSED", "معكوس", 8));

        modelBuilder.Entity<PaymentMethod>().HasData(
            Method(1, "CASH", "نقدي", true, false, false, false, 1),
            Method(2, "CHEQUE", "شيك", false, true, true, true, 2),
            Method(3, "BANK_TRANSFER", "تحويل بنكي", false, true, true, true, 3));

        modelBuilder.Entity<ApprovalStatusReference>().HasData(
            Approval(1, "PENDING", "بانتظار الاعتماد", 1), Approval(2, "UNDER_REVIEW", "تحت المراجعة", 2),
            Approval(3, "APPROVED", "معتمد", 3), Approval(4, "REJECTED", "مرفوض", 4),
            Approval(5, "RETURNED", "معاد للتعديل", 5), Approval(6, "CANCELLED", "ملغي", 6));

        var numberingTypes = new[]
        {
            (1, "BRANCH", "فرع"), (2, "COMPANY", "شركة"), (3, "RECEIPT_VOUCHER", "سند قبض"),
            (4, "PAYMENT_VOUCHER", "سند صرف"), (5, "PAYMENT_REQUEST", "طلب صرف"),
            (6, "JOURNAL_ENTRY", "قيد محاسبي"), (7, "CUSTOMER", "عميل"), (8, "VEHICLE", "مركبة"),
            (9, "DRIVER", "سائق"), (10, "SHIPMENT", "بوليصة شحن"), (11, "TICKET", "تذكرة"), (12, "TRIP", "رحلة")
        };
        modelBuilder.Entity<NumberingDocumentType>().HasData(numberingTypes.Select(x => new NumberingDocumentType
        {
            Numbering_Document_Type_ID = x.Item1,
            Document_Type_Code = x.Item2,
            Document_Type_Name_AR = x.Item3,
            Is_Active = true,
            Sort_Order = x.Item1
        }));

        var operations = new[] { "VIEW", "ADD", "EDIT", "DELETE", "PRINT", "EXPORT", "IMPORT", "APPROVE", "UNAPPROVE" };
        modelBuilder.Entity<SystemPermission>().HasData(operations.Select((code, index) => new SystemPermission
        {
            Permission_ID = index + 1,
            Permission_Code = code,
            Permission_Name = PermissionArabic(code),
            Permission_Type = "DATA",
            Is_Active = true,
            Sort_Order = index + 1,
            Created_At = seedDate
        }));

        var screens = ScreenSeeds();
        modelBuilder.Entity<SystemScreen>().HasData(screens.Select((x, index) => new SystemScreen
        {
            Screen_ID = index + 1,
            Screen_Code = x.Code,
            Screen_Name = x.Name,
            Module_Name = x.Module,
            Is_Active = true,
            Sort_Order = x.Sort,
            Created_At = seedDate
        }));
    }

    private static VoucherStatus Status(int id, string code, string name, int sort) => new()
    {
        Voucher_Status_ID = id, Voucher_Status_Code = code, Voucher_Status_Name_AR = name,
        Is_Active = true, Sort_Order = sort
    };

    private static PaymentMethod Method(int id, string code, string name, bool cash, bool bank, bool reference, bool referenceDate, int sort) => new()
    {
        Payment_Method_ID = id, Payment_Method_Code = code, Payment_Method_Name_AR = name,
        Is_Cash = cash, Is_Bank = bank, Requires_Reference = reference,
        Requires_Reference_Date = referenceDate, Is_Active = true, Sort_Order = sort
    };

    private static ApprovalStatusReference Approval(int id, string code, string name, int sort) => new()
    {
        Approval_Status_ID = id, Approval_Status_Code = code,
        Approval_Status_Name_AR = name, Is_Active = true, Sort_Order = sort
    };

    private static string PermissionArabic(string code) => code switch
    {
        "VIEW" => "عرض", "ADD" => "إضافة", "EDIT" => "تعديل", "DELETE" => "إيقاف أو حذف",
        "PRINT" => "طباعة", "EXPORT" => "تصدير", "IMPORT" => "استيراد",
        "APPROVE" => "اعتماد", "UNAPPROVE" => "إلغاء اعتماد", _ => code
    };

    private static (string Code, string Name, string Module, int Sort)[] ScreenSeeds() =>
    [
        ("TenantGroups", "المجموعات التجارية", "الهيكل المؤسسي", 5),
        ("Companies", "الشركات", "الهيكل المؤسسي", 10), ("Branches", "الفروع", "الهيكل المؤسسي", 20),
        ("Countries", "الدول", "الهيكل المؤسسي", 25), ("Governorates", "المحافظات", "الهيكل المؤسسي", 26),
        ("Cities", "المدن", "الهيكل المؤسسي", 27), ("Users", "المستخدمون", "المستخدمون والصلاحيات", 40),
        ("Roles", "الأدوار", "المستخدمون والصلاحيات", 50), ("RolePermissions", "صلاحيات الأدوار", "المستخدمون والصلاحيات", 60),
        ("AuditLogs", "سجل التدقيق والرقابة", "الأمن والرقابة", 70), ("Sessions", "الجلسات النشطة", "المستخدمون والصلاحيات", 80),
        ("GeneralSettings", "الإعدادات العامة والمالية", "التهيئة والإعدادات", 70),
        ("SystemScreens", "كتالوج شاشات النظام", "التهيئة والإعدادات", 80),
        ("NumberingSettings", "إعدادات الترقيم", "التهيئة والإعدادات", 90),
        ("FiscalYears", "السنوات المالية", "التهيئة والإعدادات", 95),
        ("FiscalPeriods", "الفترات المالية", "التهيئة والإعدادات", 100),
        ("ExchangeRates", "أسعار الصرف", "التهيئة والإعدادات", 110),
        ("PaymentMethods", "طرق السداد", "التهيئة والإعدادات", 120),
        ("VoucherTypes", "أنواع السندات", "التهيئة والإعدادات", 130),
        ("VoucherStatuses", "حالات السندات", "التهيئة والإعدادات", 140),
        ("ApprovalPolicies", "سياسات الاعتماد والسقوف", "التهيئة والإعدادات", 150),
        ("ChartOfAccounts", "الدليل المحاسبي", "الحسابات", 160), ("Currencies", "العملات", "الحسابات", 170),
        ("CostCenters", "مراكز التكلفة", "الحسابات", 180), ("CashBoxes", "الصناديق", "الحسابات", 190),
        ("Banks", "البنوك والحسابات البنكية", "الحسابات", 200), ("Parties", "الأطراف المالية", "الحسابات", 210),
        ("ReceiptVoucher", "سند القبض", "الحسابات", 220), ("PaymentVoucher", "سند الصرف", "الحسابات", 230),
        ("PaymentRequest", "طلب الصرف", "الحسابات", 235), ("JournalVoucher", "القيد اليومي", "الحسابات", 240),
        ("DocumentSearch", "البحث عن المستندات", "الحسابات", 250),
        ("ApprovalRequests", "طلبات الاعتماد", "الحسابات", 260),
        ("FinancialLimits", "السقوف المالية وحركات الاستخدام", "الحسابات", 270),
        ("TrialBalance", "ميزان المراجعة", "التقارير المالية", 280),
        ("GeneralLedger", "الأستاذ العام", "التقارير المالية", 290)
    ];

    private static void ApplyGlobalConventions(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName() ?? entityType.ClrType.Name;
            entityType.SetTableName(ToSnakeCase(tableName));

            foreach (var property in entityType.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));

                // MySQL لا يقبل COLLATE إلا للأنواع النصية. لا تطبّقه على المفاتيح
                // الرقمية أو التواريخ أو decimal أو byte[]، وإلا سيولّد EF DDL غير صالح.
                var clrType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                if (clrType != typeof(string))
                    continue;

                if (property.GetMaxLength() is null)
                    property.SetMaxLength(DefaultLength(property.Name));

                property.SetCollation(Collation);
            }
        }
    }

    private static int DefaultLength(string propertyName)
    {
        if (propertyName.EndsWith("_ID", StringComparison.OrdinalIgnoreCase)) return 50;
        if (propertyName.Contains("Notes", StringComparison.OrdinalIgnoreCase) ||
            propertyName.Contains("Description", StringComparison.OrdinalIgnoreCase) ||
            propertyName.Contains("Reason", StringComparison.OrdinalIgnoreCase)) return 500;
        if (propertyName.Contains("Name", StringComparison.OrdinalIgnoreCase)) return 200;
        if (propertyName.Contains("Code", StringComparison.OrdinalIgnoreCase) ||
            propertyName.Contains("Type", StringComparison.OrdinalIgnoreCase) ||
            propertyName.Contains("Status", StringComparison.OrdinalIgnoreCase)) return 100;
        return 255;
    }

    private static string ToSnakeCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        var builder = new StringBuilder(value.Length + 8);
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (character is '-' or ' ')
            {
                if (builder.Length > 0 && builder[^1] != '_') builder.Append('_');
                continue;
            }

            if (char.IsUpper(character) && index > 0 && value[index - 1] != '_' &&
                (char.IsLower(value[index - 1]) || (index + 1 < value.Length && char.IsLower(value[index + 1]))))
                builder.Append('_');

            builder.Append(char.ToLowerInvariant(character));
        }
        return builder.ToString().Trim('_');
    }
}
