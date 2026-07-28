using AlTayerERP.API.Services;
using AlTayerERP.API.Services.Accounting;
using AlTayerERP.API.Services.Accounting.VoucherWorkflow;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using AlTayerERP.API.Security;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("لم يتم العثور على نص الاتصال DefaultConnection داخل appsettings.json.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AlTayerERPClients", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7021", "http://localhost:5021",
                "https://localhost:7022", "http://localhost:5022",
                "http://172.16.4.250:5021")
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
builder.Services.AddScoped<PaymentRequestSchemaInitializer>();
builder.Services.AddSingleton<ServerSessionService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<LoginSecurityService>();
builder.Services.AddScoped<ScreenAuthorizationService>();
builder.Services.AddScoped<SettingsResolverService>();
builder.Services.AddScoped<AuditTrailService>();

builder.Services
    .AddAuthentication(ServerSessionAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ServerSessionAuthenticationHandler>(
        ServerSessionAuthenticationHandler.SchemeName, _ => { });
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

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<PaymentRequestSchemaInitializer>().EnsureCreatedAsync();
    await scope.ServiceProvider.GetRequiredService<SystemScreenCatalogSeeder>().EnsureSeededAsync();
    await scope.ServiceProvider.GetRequiredService<VoucherReferenceDataSeeder>().EnsureSeededAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("AlTayerERPClients");
app.UseAuthentication();

app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    bool isApiRequest = path.StartsWithSegments("/api");
    bool isPublic = path.StartsWithSegments("/api/Auth/Login") ||
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

// حماية مرحلتي الترحيل وإلغاء الترحيل من تغير الحالة المحاسبية بعد حفظ السند.
app.UseMiddleware<VoucherPostingGuardMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.MapGet("/api/health", async (AppDbContext db) =>
{
    try
    {
        return await db.Database.CanConnectAsync()
            ? Results.Ok(new { api = "ready", database = "ready" })
            : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
    catch
    {
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
});

app.Run();

/// <summary>
/// يعيد فحص السند مباشرة قبل الترحيل أو إلغاء الترحيل.
/// </summary>
public sealed class VoucherPostingGuardMiddleware
{
    private readonly RequestDelegate _next;

    public VoucherPostingGuardMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        if (!HttpMethods.IsPost(context.Request.Method) ||
            !TryGetVoucherAction(context.Request.Path, out long voucherId, out string action))
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

        if (!int.TryParse(voucher.Branch_ID, out int branchId))
        {
            await RejectAsync(context, "معرف فرع السند غير صالح.");
            return;
        }

        var branch = await db.Tenant_Branches.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Branch_ID == branchId && x.Is_Active, context.RequestAborted);
        if (branch == null)
        {
            await RejectAsync(context, "فرع السند موقوف أو غير موجود.");
            return;
        }

        // لا يسمح بتغيير أثر دفتر الأستاذ داخل فترة مقفلة، سواء بالترحيل أو إلغاء الترحيل.
        bool periodIsOpen = await db.Fiscal_Periods.AsNoTracking().AnyAsync(x =>
            x.Branch_ID == branchId &&
            x.Fiscal_Year_ID == voucher.Fiscal_Year_ID &&
            x.Is_Active && !x.Is_Closed &&
            x.Start_Date.Date <= voucher.Transaction_Date.Date &&
            x.End_Date.Date >= voucher.Transaction_Date.Date,
            context.RequestAborted);

        if (!periodIsOpen)
        {
            await RejectAsync(context, action == "unpost"
                ? "لا يمكن إلغاء الترحيل لأن فترة السند المالية مقفلة أو غير فعالة."
                : "لا يمكن الترحيل خارج فترة مالية مفتوحة.");
            return;
        }

        if (action == "unpost")
        {
            await ValidateUnpostAsync(context, db, voucher);
            return;
        }

        await ValidatePostAsync(context, db, voucher, branch.Company_ID, branchId);
    }

    private async Task ValidateUnpostAsync(HttpContext context, AppDbContext db, dynamic voucher)
    {
        if (!voucher.Is_Posted || !voucher.Journal_Entry_ID.HasValue)
        {
            await RejectAsync(context, "السند غير مرحل أو لا يملك قيدًا محاسبيًا مرتبطًا.");
            return;
        }

        long journalEntryId = voucher.Journal_Entry_ID.Value;
        var journal = await db.Journal_Entry_Headers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Journal_Entry_ID == journalEntryId, context.RequestAborted);

        if (journal == null)
        {
            await RejectAsync(context, "القيد المحاسبي المرتبط بالسند غير موجود.");
            return;
        }

        if (!journal.Is_Active || !journal.Is_Posted || journal.Is_Cancelled || journal.Is_Reversed)
        {
            await RejectAsync(context, "لا يمكن إلغاء الترحيل لأن القيد غير فعال أو ملغي أو معكوس أو غير مرحل.");
            return;
        }

        if (journal.Source_Voucher_ID != voucher.Voucher_ID)
        {
            await RejectAsync(context, "ارتباط السند بالقيد غير متطابق؛ أوقف العملية وراجع سجل التدقيق.");
            return;
        }

        await _next(context);
    }

    private async Task ValidatePostAsync(
        HttpContext context,
        AppDbContext db,
        dynamic voucher,
        string companyId,
        int branchId)
    {
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

        var accountIds = ((IEnumerable<dynamic>)voucher.Details)
            .Select(x => ((string?)x.Account_ID)?.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .Distinct()
            .ToList();

        if (!isJournal && !string.IsNullOrWhiteSpace((string?)voucher.Cash_Account_ID))
            accountIds.Add(((string)voucher.Cash_Account_ID).Trim());

        accountIds = accountIds.Distinct().ToList();

        var accounts = await db.Chart_Of_Accounts.AsNoTracking()
            .Where(x => x.Company_ID == companyId && accountIds.Contains(x.Account_ID))
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
            string cashAccountId = ((string?)voucher.Cash_Account_ID)?.Trim() ?? string.Empty;
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
                x.Company_ID == companyId && x.Branch_ID == branchId &&
                x.Account_ID == cashAccountId && x.Is_Active, context.RequestAborted);
            bool activeBank = await db.Bank_Accounts.AsNoTracking().AnyAsync(x =>
                x.Company_ID == companyId && x.GL_Account == cashAccountId && x.Is_Active,
                context.RequestAborted);

            if (!activeCashBox && !activeBank)
            {
                await RejectAsync(context, "لا يمكن الترحيل: حساب النقدية غير مرتبط بصندوق أو حساب بنكي نشط.");
                return;
            }

            var cashLines = ((IEnumerable<dynamic>)voucher.Details)
                .Where(x => (byte)x.Line_Type == 1).ToList();
            if (cashLines.Count != 1 || ((string?)cashLines[0].Account_ID)?.Trim() != cashAccountId)
            {
                await RejectAsync(context, "يجب وجود سطر نقدية واحد مطابق لحساب رأس السند.");
                return;
            }

            if (isReceipt && ((decimal)cashLines[0].Debit_Amount <= 0 || (decimal)cashLines[0].Credit_Amount != 0))
            {
                await RejectAsync(context, "اتجاه حساب النقدية في سند القبض غير صحيح؛ يجب أن يكون مدينًا.");
                return;
            }

            if (isPayment && ((decimal)cashLines[0].Credit_Amount <= 0 || (decimal)cashLines[0].Debit_Amount != 0))
            {
                await RejectAsync(context, "اتجاه حساب النقدية في سند الصرف غير صحيح؛ يجب أن يكون دائنًا.");
                return;
            }
        }

        await _next(context);
    }

    private static bool TryGetVoucherAction(PathString path, out long voucherId, out string action)
    {
        voucherId = 0;
        action = string.Empty;
        string[] parts = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

        if (parts.Length != 4 ||
            !parts[0].Equals("api", StringComparison.OrdinalIgnoreCase) ||
            !parts[1].Equals("FinancialVoucher", StringComparison.OrdinalIgnoreCase) ||
            !long.TryParse(parts[2], out voucherId) || voucherId <= 0)
            return false;

        action = parts[3].ToLowerInvariant();
        return action is "post" or "unpost";
    }

    private static async Task RejectAsync(
        HttpContext context,
        string message,
        int statusCode = StatusCodes.Status400BadRequest)
    {
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new { success = false, message }, context.RequestAborted);
    }
}
