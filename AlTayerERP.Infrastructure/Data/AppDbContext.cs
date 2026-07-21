using AlTayerERP.Core.Entities;
using AlTayerERP.Core.Entities.Accounting;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.Infrastructure.Data
{
    /// <summary>
    /// سياق قاعدة البيانات الرئيسي لنظام AlTayerERP.
    /// يحتوي على تعريف الجداول والعلاقات وإعدادات Entity Framework Core.
    /// </summary>
    public class AppDbContext : DbContext
    {
        // مشيد الكلاس (Constructor) لتمرير إعدادات الاتصال بقاعدة البيانات
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        #region 1. الهيكل الإداري والشركات والفروع

        // جدول مجموعات المستأجرين / الشركات الأم (Multi-Tenancy)
        public DbSet<TenantGroup> Tenant_Groups { get; set; } = null!;

        // جدول الشركات التابعة للمجموعة
        public DbSet<Company> Companies { get; set; } = null!;

        // جدول الفروع الخاصة بكل شركة
        public DbSet<TenantBranch> Tenant_Branches { get; set; } = null!;

        // جدول السنوات المالية لإغلاق وفتح الحسابات
        public DbSet<FiscalYear> Fiscal_Years { get; set; } = null!;

        #endregion

        #region 2. المستخدمون والأدوار والصلاحيات والشاشات

        // جدول بيانات المستخدمين في النظام
        public DbSet<User> Users { get; set; } = null!;

        // جدول أدوار المستخدمين (مثل: مدير نظام، محاسب، مدخل بيانات)
        public DbSet<Role> Roles { get; set; } = null!;

        // جدول صلاحيات الأدوار (ربط الدور بالشاشات والعمليات)
        public DbSet<RolePermission> RolePermissions { get; set; } = null!;

        // جدول صلاحيات مخصصة لمستخدم معين مباشرة خارج صلاحيات دوره
        public DbSet<UserPermission> User_Permissions { get; set; } = null!;

        // جدول العمليات أو الصلاحيات الأساسية المتوفرة في النظام ككل
        public DbSet<SystemPermission> System_Permissions { get; set; } = null!;

        // جدول شاشات النظام البرمجية المتوفرة لضبط الوصول
        public DbSet<SystemScreen> SystemScreens { get; set; } = null!;

        #endregion

        #region 3. إعدادات الترقيم والرقابة والاعتمادات

        // جدول إعدادات عامة قابلة للتخصيص على مستوى النظام/الشركة/الفرع/السنة.
        public DbSet<SystemSetting> System_Settings { get; set; } = null!;

        // الفترات المحاسبية داخل السنة المالية لكل فرع.
        public DbSet<FiscalPeriod> Fiscal_Periods { get; set; } = null!;

        // سجل أسعار الصرف بتاريخ السريان للشركات.
        public DbSet<ExchangeRate> Exchange_Rates { get; set; } = null!;

        // جدول إعدادات ترميز وترقيم المستندات (مثل السندات والفواتير) لكل فرع
        public DbSet<NumberingSetting> Numbering_Settings { get; set; } = null!;

        // جدول العدادات الحالية لتسلسل الأرقام تجنبًا للتكرار
        public DbSet<NumberingCounter> Numbering_Counters { get; set; } = null!;

        // جدول السياسات المالية والحدود الائتمانية (مثل السقوف المالية للصرف)
        public DbSet<FinancialPolicy> Financial_Policies { get; set; } = null!;

        // جدول حركات وتغييرات السياسات المالية والرقابية
        public DbSet<FinancialPolicyMovement> Financial_Policy_Movements { get; set; } = null!;

        // جدول طلبات الاعتماد والموافقات الإدارية على المستندات
        public DbSet<ApprovalRequest> Approval_Requests { get; set; } = null!;

        #endregion

        #region 4. دليل الحسابات والعملات ومراكز التكلفة

        // جدول إعدادات أطوال ومستويات شجرة الحسابات (دليل الحسابات)
        public DbSet<AccountCodeSetting> Account_Code_Settings { get; set; } = null!;

        // جدول دليل الحسابات الرئيسي (الحسابات العامة، الأصول، الخصوم، إلخ)
        public DbSet<ChartOfAccount> Chart_Of_Accounts { get; set; } = null!;

        // جدول مراكز التكلفة لتحليل المصاريف والإيرادات بدقة
        public DbSet<CostCenter> Cost_Centers { get; set; } = null!;

        // جدول الصناديق / الخزائن المالية التابعة للفروع
        public DbSet<CashBox> Cash_Boxes { get; set; } = null!;

        // جدول العملات الأجنبية والمحلية المستخدمة في النظام
        public DbSet<Currency> Currencies { get; set; } = null!;

        // الحسابات البنكية التشغيلية للشركات.
        public DbSet<BankAccount> Bank_Accounts { get; set; } = null!;

        #endregion

        #region 5. المحرك المالي

        // جدول رأس السندات المالية (سندات القيد، القبض، الصرف)
        public DbSet<FinancialVoucherHeader> Financial_Voucher_Headers { get; set; } = null!;

        // جدول تفاصيل السندات المالية (القيود المحاسبية التفصيلية من دائن ومدين)
        public DbSet<FinancialVoucherDetail> Financial_Voucher_Details { get; set; } = null!;

        // جدول رأس القيود المحاسبية العامة
        public DbSet<JournalEntryHeader> Journal_Entry_Headers { get; set; } = null!;

        // جدول تفاصيل القيود المحاسبية العامة
        public DbSet<JournalEntryDetail> Journal_Entry_Details { get; set; } = null!;
        // جدول تخصيص المستندات وتوزيع المبالغ (تسوية الفواتير مع السندات)
        public DbSet<DocumentAllocation> Document_Allocations { get; set; } = null!;

        // جدول ربط المستندات ببعضها في الدورة المستندية لضمان الترابط
        public DbSet<DocumentLink> Document_Links { get; set; } = null!;

        // جدول سجلات التدقيق العام لحركات النظام (الأمان والرقابة)
        public DbSet<AuditLog> Audit_Logs { get; set; } = null!;

        // جدول سجل مراقبة وتتبع العمليات التي تمت على السندات المحددة (تعديل، حذف، ترحيل)
        public DbSet<VoucherActionLog> Voucher_Action_Logs { get; set; } = null!;
        // جدول أنواع السندات المالية
        public DbSet<VoucherType> Voucher_Types { get; set; } = null!;

        // جدول حالات السندات المالية
        public DbSet<VoucherStatus> Voucher_Statuses { get; set; } = null!;

        // جدول طرق السداد
        public DbSet<PaymentMethod> Payment_Methods { get; set; } = null!;

        // جدول الأطراف المالية
        public DbSet<Party> Parties { get; set; } = null!;
        #endregion

        #region إعداد الجداول والعلاقات باستخدام (Fluent API)

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region 1. الهيكل الإداري

            // إعدادات جدول مجموعات المستأجرين وتحديد المفتاح الرئيسي
            modelBuilder.Entity<TenantGroup>(entity =>
            {
                entity.ToTable("tenant_groups");
                entity.HasKey(e => e.Group_ID);
            });

            // إعدادات جدول الشركات وتحديد المفتاح الرئيسي
            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("companies");
                entity.HasKey(e => e.Company_ID);
            });

            // إعدادات جدول الفروع وتحديد المفتاح الرئيسي
            modelBuilder.Entity<TenantBranch>(entity =>
            {
                entity.ToTable("tenant_branches");
                entity.HasKey(e => e.Branch_ID);
            });

            // إعدادات جدول السنوات المالية وتحديد المفتاح الرئيسي
            modelBuilder.Entity<FiscalYear>(entity =>
            {
                entity.ToTable("fiscal_years");
                entity.HasKey(e => e.Fiscal_Year_ID);
            });

            #endregion

            #region 2. المستخدمون والصلاحيات

            // إعدادات جدول المستخدمين وتحديد المفتاح الرئيسي
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.User_ID);
            });

            // إعدادات جدول الأدوار وتحديد المفتاح الرئيسي
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("roles");
                entity.HasKey(e => e.Role_ID);
            });

            // إعدادات جدول صلاحيات الأدوار وتحديد المفتاح الرئيسي
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.ToTable("role_permissions");
                entity.HasKey(e => e.Permission_ID);
            });

            // إعدادات جدول صلاحيات المستخدمين المباشرة وتحديد المفتاح الرئيسي
            modelBuilder.Entity<UserPermission>(entity =>
            {
                entity.ToTable("user_permissions");
                entity.HasKey(e => e.Permission_ID);
            });

            // إعدادات جدول صلاحيات النظام وتحديد المفتاح الرئيسي
            modelBuilder.Entity<SystemPermission>(entity =>
            {
                entity.ToTable("system_permissions");
                entity.HasKey(e => e.Permission_ID);
            });

            // إعدادات جدول شاشات النظام وتحديد المفتاح الرئيسي
            modelBuilder.Entity<SystemScreen>(entity =>
            {
                entity.ToTable("system_screens");
                entity.HasKey(e => e.Screen_ID);
            });

            #endregion

            #region 3. الترقيم والرقابة والاعتمادات

            // إعدادات جدول إعدادات الترقيم وتحديد المفتاح الرئيسي
            modelBuilder.Entity<NumberingSetting>(entity =>
            {
                entity.ToTable("numbering_settings");
                entity.HasKey(e => e.Numbering_ID);
            });

            // إعدادات جدول عدادات الترقيم وتحديد المفتاح الرئيسي
            modelBuilder.Entity<NumberingCounter>(entity =>
            {
                entity.ToTable("numbering_counters");
                entity.HasKey(e => e.Counter_ID);
            });

            // إعدادات جدول الحدود المالية وتحديد المفتاح الرئيسي
            modelBuilder.Entity<FinancialPolicy>(entity =>
            {
                entity.ToTable("financial_limits");
                entity.HasKey(e => e.Limit_ID);
            });

            // إعدادات جدول حركات الحدود المالية وتحديد المفتاح الرئيسي
            modelBuilder.Entity<FinancialPolicyMovement>(entity =>
            {
                entity.ToTable("financial_limit_movements");
                entity.HasKey(e => e.Movement_ID);
            });

            // إعدادات جدول طلبات الاعتماد وتحديد المفتاح الرئيسي
            modelBuilder.Entity<ApprovalRequest>(entity =>
            {
                entity.ToTable("approval_requests");
                entity.HasKey(e => e.Approval_ID);
            });

            #endregion

            #region 4. دليل الحسابات والعملات

            // إعدادات جدول إعدادات ترميز شجرة الحسابات وتحديد المفتاح الرئيسي
            modelBuilder.Entity<AccountCodeSetting>(entity =>
            {
                entity.ToTable("account_code_settings");
                entity.HasKey(e => e.Setting_ID);
            });

            // إعدادات جدول شجرة الحسابات وتحديد المفتاح الرئيسي
            modelBuilder.Entity<ChartOfAccount>(entity =>
            {
                entity.ToTable("chart_of_accounts");
                entity.HasKey(e => e.Account_ID);
            });

            // إعدادات جدول مراكز التكلفة وتحديد المفتاح الرئيسي
            modelBuilder.Entity<CostCenter>(entity =>
            {
                entity.ToTable("cost_centers");
                entity.HasKey(e => e.Cost_Center_ID);
            });

            // إعدادات جدول الصناديق وتحديد المفتاح الرئيسي
            modelBuilder.Entity<CashBox>(entity =>
            {
                entity.ToTable("cash_boxes");
                entity.HasKey(e => e.Cash_Box_ID);
            });

            // إعدادات جدول العملات الأجنبية والمحلية
            modelBuilder.Entity<Currency>(entity =>
            {
                entity.ToTable("currencies");
                entity.HasKey(e => e.Currency_ID);

                // تحديد دقة الرقم العشري لسعر الصرف (18 خانة إجمالية، 6 بعد الفاصلة) لضمان الدقة
                entity.Property(e => e.Exchange_Rate)
                    .HasPrecision(18, 6);
            });

            // الحسابات البنكية التشغيلية للشركة.
            modelBuilder.Entity<BankAccount>(entity =>
            {
                entity.ToTable("bank_accounts");
                entity.HasKey(e => e.Bank_Account_ID);
                entity.HasIndex(e => new { e.Company_ID, e.Account_No }).IsUnique();
            });

            #endregion
            #region أنواع السندات والحالات وطرق السداد والأطراف

            // إعدادات جدول أنواع السندات المالية
            modelBuilder.Entity<VoucherType>(entity =>
            {
                entity.ToTable("voucher_types");
                entity.HasKey(e => e.Voucher_Type_ID);
            });

            // إعدادات جدول حالات السندات المالية
            modelBuilder.Entity<VoucherStatus>(entity =>
            {
                entity.ToTable("voucher_statuses");
                entity.HasKey(e => e.Voucher_Status_ID);
            });

            // إعدادات جدول طرق السداد
            modelBuilder.Entity<PaymentMethod>(entity =>
            {
                entity.ToTable("payment_methods");
                entity.HasKey(e => e.Payment_Method_ID);
            });

            // إعدادات جدول الأطراف المالية
            modelBuilder.Entity<Party>(entity =>
            {
                entity.ToTable("parties");
                entity.HasKey(e => e.Party_ID);

                entity.Property(e => e.Credit_Limit)
                    .HasPrecision(18, 2);
            });

            #endregion
            #region 5. رأس السندات المالية

            // إعدادات قواعد ومؤشرات رأس السندات المالية لقاعدة البيانات
            modelBuilder.Entity<FinancialVoucherHeader>(entity =>
            {
                entity.ToTable("financial_voucher_headers");
                entity.HasKey(e => e.Voucher_ID);

                // [التعديل الأول المطلوب]: منع تكرار رقم السند بناءً على (الفرع + السنة المالية + نوع السند + رقم السند)
                entity.HasIndex(e => new
                {
                    e.Branch_ID,
                    e.Fiscal_Year_ID,
                    e.Voucher_Type_ID,
                    e.Voucher_No
                })
                .IsUnique()
                .HasDatabaseName("UQ_Financial_Voucher_Branch_Year_Type_No");

                // إنشاء فهارس (Indexes) لتسريع عمليات الفرز والبحث والتقارير المالية بناءً على الحقول الأساسية
                entity.HasIndex(e => e.Voucher_Date).HasDatabaseName("IX_Financial_Voucher_Date");
                entity.HasIndex(e => e.Transaction_Date).HasDatabaseName("IX_Financial_Voucher_Transaction_Date");
                entity.HasIndex(e => e.Voucher_Type_ID).HasDatabaseName("IX_Financial_Voucher_Type");
                entity.HasIndex(e => e.Voucher_Status_ID).HasDatabaseName("IX_Financial_Voucher_Status");
                entity.HasIndex(e => e.Cash_Account_ID).HasDatabaseName("IX_Financial_Voucher_Cash_Account");
                entity.HasIndex(e => e.Party_ID).HasDatabaseName("IX_Financial_Voucher_Party");
                entity.HasIndex(e => e.Is_Posted).HasDatabaseName("IX_Financial_Voucher_Posted");
                entity.HasIndex(e => new { e.Document_Type_ID, e.Document_ID }).HasDatabaseName("IX_Financial_Voucher_Source");

                // ضبط دقة الأرقام العشرية لأسعار الصرف والإجماليات بالعملة المحلية والأجنبية
                entity.Property(e => e.Exchange_Rate).HasPrecision(18, 6);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.Foreign_Total).HasPrecision(18, 2);
                entity.Property(e => e.Local_Total).HasPrecision(18, 2);
                entity.Property(e => e.Received_From_Name).HasMaxLength(200);
                entity.Property(e => e.Review_Notes).HasMaxLength(500);
                entity.Property(e => e.Reviewed_By_User_ID).HasMaxLength(50);

                // علاقة رأس وتفاصيل (One-to-Many): السند يمتلك تفاصيل متعددة، وعند حذف السند تُحذف تفاصيله تلقائيًا
                entity.HasMany(e => e.Details)
                    .WithOne(e => e.Voucher)
                    .HasForeignKey(e => e.Voucher_ID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            #endregion

            #region 6. تفاصيل السندات والروابط المالية

            // إعدادات وقواعد قيود التفاصيل المحاسبية
            modelBuilder.Entity<FinancialVoucherDetail>(entity =>
            {
                entity.ToTable("financial_voucher_details");
                entity.HasKey(e => e.Voucher_Detail_ID);

                // فهرس فريد: يمنع تكرار رقم السطر أو السلسلة داخل نفس السند
                entity.HasIndex(e => new { e.Voucher_ID, e.Line_No })
                .IsUnique()
                .HasDatabaseName("UQ_Financial_Voucher_Detail_Line");

                // فهارس لتسريع الربط والاستعلام عن تفاصيل السندات المرتبطة بحسابات أو عملات ومشاريع معينة
                entity.HasIndex(e => e.Voucher_ID).HasDatabaseName("IX_Financial_Voucher_Details_Voucher");
                entity.HasIndex(e => e.Account_ID).HasDatabaseName("IX_Financial_Voucher_Details_Account");
                entity.HasIndex(e => e.Currency_ID).HasDatabaseName("IX_Financial_Voucher_Details_Currency");
                entity.HasIndex(e => e.Cost_Center_ID).HasDatabaseName("IX_Financial_Voucher_Details_Cost_Center");
                entity.HasIndex(e => e.Project_ID).HasDatabaseName("IX_Financial_Voucher_Details_Project");

                // ضبط الحقول المالية العشرية (مبالغ دائن ومدين والمبلغ بالعملة المحلية والأجنبية)
                entity.Property(e => e.Exchange_Rate).HasPrecision(18, 6);
                entity.Property(e => e.Foreign_Amount).HasPrecision(18, 2);
                entity.Property(e => e.Local_Amount).HasPrecision(18, 2);
                entity.Property(e => e.Debit_Amount).HasPrecision(18, 2);
                entity.Property(e => e.Credit_Amount).HasPrecision(18, 2);

                #region 7. القيود المحاسبية العامة

                // إعدادات رأس القيد المحاسبي
                modelBuilder.Entity<JournalEntryHeader>(entity =>
                {
                    entity.ToTable("journal_entry_headers");

                    entity.HasKey(e => e.Journal_Entry_ID);

                    entity.HasIndex(e => e.Entry_No)
                        .IsUnique()
                        .HasDatabaseName("UQ_Journal_Entry_No");

                    entity.HasIndex(e => e.Entry_Date);

                    entity.HasIndex(e => e.Source_Voucher_ID);

                    entity.Property(e => e.Total_Debit)
                        .HasPrecision(18, 2);

                    entity.Property(e => e.Total_Credit)
                        .HasPrecision(18, 2);

                    entity.HasMany(e => e.Details)
                        .WithOne(e => e.JournalEntry)
                        .HasForeignKey(e => e.Journal_Entry_ID)
                        .OnDelete(DeleteBehavior.Cascade);
                });


                // إعدادات تفاصيل القيود
                modelBuilder.Entity<JournalEntryDetail>(entity =>
                {
                    entity.ToTable("journal_entry_details");

                    entity.HasKey(e => e.Journal_Entry_Detail_ID);

                    entity.HasIndex(e => new
                    {
                        e.Journal_Entry_ID,
                        e.Line_No
                    })
                    .IsUnique();

                    entity.Property(e => e.Exchange_Rate)
                        .HasPrecision(18, 6);

                    entity.Property(e => e.Foreign_Amount)
                        .HasPrecision(18, 2);

                    entity.Property(e => e.Local_Amount)
                        .HasPrecision(18, 2);

                    entity.Property(e => e.Debit_Amount)
                        .HasPrecision(18, 2);

                    entity.Property(e => e.Credit_Amount)
                        .HasPrecision(18, 2);
                });

                #endregion


                // [التعديل الثاني المطلوب]: تم حذف الكود المكرر لتعريف علاقة HasOne هنا اعتمادًا على التعريف في قسم الرأس
            });

            // إعدادات جدول التخصيص والمبالغ المتبقية للوثائق والتحصيل
            modelBuilder.Entity<DocumentAllocation>(entity =>
            {
                entity.ToTable("document_allocations");
                entity.HasKey(e => e.Allocation_ID);

                // ضبط دقة الكسور العشرية للحقول المالية داخل جدول التخصيصات
                entity.Property(e => e.Exchange_Rate).HasPrecision(18, 6);
                entity.Property(e => e.Document_Total).HasPrecision(18, 2);
                entity.Property(e => e.Collected_Before).HasPrecision(18, 2);
                entity.Property(e => e.Collected_Now).HasPrecision(18, 2);
                entity.Property(e => e.Remaining_Balance).HasPrecision(18, 2);

                // ربط جدول التخصيص برأس السند المالي وحذفه في حال حذف السند
                entity.HasOne(e => e.Voucher)
                    .WithMany(e => e.DocumentAllocations)
                    .HasForeignKey(e => e.Voucher_ID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // إعدادات جدول روابط الوثائق ببعضها البعض ومفتاحه الرئيسي
            modelBuilder.Entity<DocumentLink>(entity =>
            {
                entity.ToTable("document_links");
                entity.HasKey(e => e.Document_Link_ID);
            });

            // إعدادات جدول مراقبة النظام العام ومفتاحه الرئيسي
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("audit_logs");
                entity.HasKey(e => e.Audit_ID);
            });

            // إعدادات جدول مراقبة عمليات المستخدمين المخصصة للسندات المالية
            modelBuilder.Entity<VoucherActionLog>(entity =>
            {
                entity.ToTable("voucher_action_logs");
                entity.HasKey(e => e.Voucher_Action_ID);

                // [التعديل الثالث المطلوب]: تعديل سلوك الحذف إلى Restrict لمنع حذف السندات التي تمتلك حركات رقابة مسجلة لضمان الأمن المالي والرقابي
                entity.HasOne(e => e.Voucher)
                    .WithMany(e => e.VoucherActionLogs)
                    .HasForeignKey(e => e.Voucher_ID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            #endregion
        }

        #endregion
    }
}
