using AlTayerERP.API.Services;
using AlTayerERP.API.Services.Accounting;
using AlTayerERP.API.Services.Accounting.VoucherWorkflow;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using AlTayerERP.API.Security;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ملف محلي اختياري لاتصال قاعدة البيانات. يبقى خارج Git وفق .gitignore.
builder.Configuration.AddJsonFile(
    "appsettings.Local.json",
    optional: true,
    reloadOnChange: false
);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "لم يتم ضبط DefaultConnection. عيّنه محلياً عبر User Secrets أو المتغير ConnectionStrings__DefaultConnection؛ لا تضع كلمة المرور داخل ملفات الإعداد المتتبعة."
    );

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    )
);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AlTayerERPClients", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7021",
                "http://localhost:5021",
                "https://localhost:7022",
                "http://localhost:5022")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddScoped<NumberGeneratorService>();
builder.Services.AddScoped<CostCenterNumberService>();
builder.Services.AddScoped<CashBoxNumberService>();
builder.Services.AddScoped<ApprovalService>();
builder.Services.AddScoped<FinancialPolicyService>();
builder.Services.AddScoped<AccountNumberService>();
builder.Services.AddScoped<SystemScreenCatalogSeeder>();
builder.Services.AddScoped<VoucherReferenceDataSeeder>();
builder.Services.AddSingleton<ServerSessionService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<LoginSecurityService>();
builder.Services.AddScoped<ScreenAuthorizationService>();
builder.Services.AddScoped<SettingsResolverService>();
builder.Services.AddScoped<AuditTrailService>();

builder.Services
    .AddAuthentication(ServerSessionAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ServerSessionAuthenticationHandler>(
        ServerSessionAuthenticationHandler.SchemeName,
        _ => { });
builder.Services.AddAuthorization();

builder.Services.AddScoped<VoucherValidationService>();
builder.Services.AddScoped<VoucherAuditService>();
builder.Services.AddScoped<VoucherApprovalService>();
builder.Services.AddScoped<FinancialVoucherService>();
builder.Services.AddScoped<JournalEntryInquiryService>();
builder.Services.AddScoped<VoucherPostingService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (builder.Configuration.GetValue<bool>("DatabaseBootstrap:EnableReferenceDataSeeding"))
{
    using var scope = app.Services.CreateScope();

    var screenCatalogSeeder = scope.ServiceProvider.GetRequiredService<SystemScreenCatalogSeeder>();
    await screenCatalogSeeder.EnsureSeededAsync();

    var voucherReferenceSeeder = scope.ServiceProvider.GetRequiredService<VoucherReferenceDataSeeder>();
    await voucherReferenceSeeder.EnsureSeededAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AlTayerERPClients");
app.UseAuthentication();

app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    var isApiRequest = path.StartsWithSegments("/api");
    var isPublic = path.StartsWithSegments("/api/Auth/Login") ||
                   path.StartsWithSegments("/api/Auth/LoginOptions") ||
                   path.StartsWithSegments("/api/Auth/LoginCompanies") ||
                   path.StartsWithSegments("/api/Auth/Refresh") ||
                   path.StartsWithSegments("/api/health") ||
                   (HttpMethods.IsGet(context.Request.Method) &&
                    (path.StartsWithSegments("/api/Branches/GetCompaniesLookup") ||
                     path.StartsWithSegments("/api/Branches/GetActiveBranchesLookup") ||
                     path.StartsWithSegments("/api/FiscalYears/Lookup")));

    if (isApiRequest && !isPublic && context.User.Identity?.IsAuthenticated != true)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new { message = "انتهت الجلسة أو رمز الوصول غير صالح. سجل الدخول من جديد." });
        return;
    }

    await next();
});

// يعيد التحقق من سلامة الحسابات والصندوق أو البنك مباشرة قبل ترحيل السند.
app.UseMiddleware<VoucherPostingGuardMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.MapGet("/api/health", async (AppDbContext db, IWebHostEnvironment environment) =>
{
    try
    {
        var databaseReady = await db.Database.CanConnectAsync();
        if (!databaseReady)
            return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);

        // اسم القاعدة وحده مسموح في Development لتأكيد بيئة UAT؛ لا يكشف بيانات اتصال.
        return environment.IsDevelopment()
            ? Results.Ok(new
            {
                api = "ready",
                database = "ready",
                databaseName = db.Database.GetDbConnection().Database
            })
            : Results.Ok(new { api = "ready", database = "ready" });
    }
    catch
    {
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
});

app.Run();

/// <summary>
/// حارس محاسبي يعيد فحص السند عند لحظة الترحيل لمنع ترحيل سند قديم
/// إذا أوقف حسابه أو صندوقه أو بنكه بعد الحفظ.
/// </summary>
public sealed class VoucherPostingGuardMiddleware
{
    private readonly RequestDelegate _next;

    public VoucherPostingGuardMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        if (!HttpMethods.IsPost(context.Request.Method) ||
            !TryGetVoucherId(context.Request.Path, out long voucherId))
        {
            await _next(context);
            return;
        }

        var voucher = await db.Financial_Voucher_Headers.AsNoTracking()
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.Voucher_ID == voucherId, context.RequestAborted);

        if (voucher == null)
        {
            await RejectAsync(context, "السند المالي غير موجود.", StatusCodes.Status404NotFound);
            return;
        }

        string? typeCode = await db.Voucher_Types.AsNoTracking()
            .Where(x => x.Voucher_Type_ID == voucher.Voucher_Type_ID && x.Is_Active)
            .Select(x => x.Voucher_Type_Code)
            .SingleOrDefaultAsync(context.RequestAborted);

        typeCode = typeCode?.Trim().ToUpperInvariant();
        bool isReceipt = typeCode == "RECEIPT";
        bool isPayment = typeCode == "PAYMENT";
        bool isJournal = typeCode == "JOURNAL";

        if (!isReceipt && !isPayment && !isJournal)
        {
            await RejectAsync(context, "نوع السند غير موجود أو غير مدعوم في الترحيل.");
            return;
        }

        int branchId = voucher.Branch_ID;

        var branch = await db.Tenant_Branches.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Branch_ID == branchId && x.Is_Active, context.RequestAborted);
        if (branch == null)
        {
            await RejectAsync(context, "فرع السند موقوف أو غير موجود.");
            return;
        }

        var accountIds = voucher.Details
            .Select(x => x.Account_ID?.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .Distinct()
            .ToList();

        if (!isJournal && !string.IsNullOrWhiteSpace(voucher.Cash_Account_ID))
            accountIds.Add(voucher.Cash_Account_ID.Trim());

        accountIds = accountIds.Distinct().ToList();

        var accounts = await db.Chart_Of_Accounts.AsNoTracking()
            .Where(x => x.Company_ID == branch.Company_ID && accountIds.Contains(x.Account_ID))
            .Select(x => new
            {
                x.Account_ID,
                x.Account_Type,
                x.Account_Category,
                x.Normal_Balance,
                x.Is_Active,
                x.Is_Postable,
                x.Is_Summary_Account,
                x.Allow_ManualEntry,
                x.Is_Control_Account
            })
            .ToListAsync(context.RequestAborted);

        if (accounts.Count != accountIds.Count)
        {
            await RejectAsync(context, "لا يمكن الترحيل: يوجد حساب لا يتبع شركة الفرع أو لم يعد موجودًا.");
            return;
        }

        if (accounts.Any(x => !x.Is_Active || !x.Is_Postable || x.Is_Summary_Account))
        {
            await RejectAsync(context, "لا يمكن الترحيل: جميع الحسابات يجب أن تكون نشطة ونهائية وقابلة للترحيل.");
            return;
        }

        if (accounts.Any(x => !x.Allow_ManualEntry || x.Is_Control_Account))
        {
            await RejectAsync(context, "لا يمكن ترحيل سند يدوي يحتوي على حساب رقابي أو حساب يمنع الإدخال اليدوي.");
            return;
        }

        if (!isJournal)
        {
            if (string.IsNullOrWhiteSpace(voucher.Cash_Account_ID))
            {
                await RejectAsync(context, "حساب الصندوق أو البنك غير محدد في السند.");
                return;
            }

            string cashAccountId = voucher.Cash_Account_ID.Trim();
            var cashAccount = accounts.SingleOrDefault(x => x.Account_ID == cashAccountId);

            bool validCashAccount = cashAccount != null &&
                string.Equals(cashAccount.Account_Type, "Asset", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(cashAccount.Normal_Balance, "Debit", StringComparison.OrdinalIgnoreCase) &&
                (string.Equals(cashAccount.Account_Category, "Cash", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(cashAccount.Account_Category, "Bank", StringComparison.OrdinalIgnoreCase));

            if (!validCashAccount)
            {
                await RejectAsync(context, "حساب الصندوق أو البنك لا يطابق تصنيف النقدية المعتمد.");
                return;
            }

            bool activeCashBox = await db.Cash_Boxes.AsNoTracking().AnyAsync(x =>
                x.Company_ID == branch.Company_ID &&
                x.Branch_ID == branchId &&
                x.Account_ID == cashAccountId &&
                x.Is_Active, context.RequestAborted);

            bool activeBank = await db.Bank_Accounts.AsNoTracking().AnyAsync(x =>
                x.Company_ID == branch.Company_ID &&
                x.GL_Account == cashAccountId &&
                x.Is_Active, context.RequestAborted);

            if (!activeCashBox && !activeBank)
            {
                await RejectAsync(context, "لا يمكن الترحيل: حساب النقدية غير مرتبط بصندوق أو حساب بنكي نشط.");
                return;
            }

            var cashLines = voucher.Details.Where(x => x.Line_Type == 1).ToList();
            if (cashLines.Count != 1 || cashLines[0].Account_ID?.Trim() != cashAccountId)
            {
                await RejectAsync(context, "يجب وجود سطر نقدية واحد مطابق لحساب رأس السند.");
                return;
            }

            if (isReceipt && (cashLines[0].Debit_Amount <= 0 || cashLines[0].Credit_Amount != 0))
            {
                await RejectAsync(context, "اتجاه حساب النقدية في سند القبض غير صحيح؛ يجب أن يكون مدينًا.");
                return;
            }

            if (isPayment && (cashLines[0].Credit_Amount <= 0 || cashLines[0].Debit_Amount != 0))
            {
                await RejectAsync(context, "اتجاه حساب النقدية في سند الصرف غير صحيح؛ يجب أن يكون دائنًا.");
                return;
            }
        }

        await _next(context);
    }

    private static bool TryGetVoucherId(PathString path, out long voucherId)
    {
        voucherId = 0;
        string[] parts = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

        return parts.Length == 4 &&
               parts[0].Equals("api", StringComparison.OrdinalIgnoreCase) &&
               parts[1].Equals("FinancialVoucher", StringComparison.OrdinalIgnoreCase) &&
               parts[3].Equals("post", StringComparison.OrdinalIgnoreCase) &&
               long.TryParse(parts[2], out voucherId) &&
               voucherId > 0;
    }

    private static async Task RejectAsync(
        HttpContext context,
        string message,
        int statusCode = StatusCodes.Status400BadRequest)
    {
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(
            new { success = false, message },
            context.RequestAborted);
    }
}
