using AlTayerERP.Core.Entities;
using AlTayerERP.Core.Entities.Accounting;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.Infrastructure.Data;

/// <summary>
/// سياق قاعدة بيانات المرحلة الأولى.
/// جميع قواعد المخطط معرفة في Phase1BaselineModelConfiguration،
/// ولا ينشئ السياق أو يعدل المخطط وقت تشغيل API.
/// </summary>
public class AppDbContext : DbContext
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
    public DbSet<FinancialPolicy> Financial_Policies => Set<FinancialPolicy>();
    public DbSet<FinancialPolicyMovement> Financial_Policy_Movements => Set<FinancialPolicyMovement>();
    public DbSet<ApprovalRequest> Approval_Requests => Set<ApprovalRequest>();
    public DbSet<ApprovalStatusReference> Approval_Statuses => Set<ApprovalStatusReference>();
    public DbSet<DocumentTypeReference> Document_Types => Set<DocumentTypeReference>();

    public DbSet<AccountCategory> Account_Categories => Set<AccountCategory>();
    public DbSet<AccountCodeSetting> Account_Code_Settings => Set<AccountCodeSetting>();
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
        Phase1BaselineModelConfiguration.Configure(modelBuilder);
    }
}
