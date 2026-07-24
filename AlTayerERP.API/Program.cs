using AlTayerERP.API.Services;
using AlTayerERP.API.Services.Accounting;
using AlTayerERP.API.Services.Accounting.VoucherWorkflow;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using AlTayerERP.API.Security;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);

// ====================================================================
// [1] قراءة نص الاتصال وتسجيل قاعدة البيانات بمحرك Pomelo MySQL
// ====================================================================
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "لم يتم العثور على نص الاتصال DefaultConnection داخل appsettings.json."
    );

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    )
);

// ====================================================================
// [2] تسجيل سياسة CORS لربط تطبيق WinForms بالـ API
// ====================================================================
builder.Services.AddCors(options =>
{
    // تطبيق WinForms لا يعتمد على CORS، لكن Swagger والمتصفحات المحلية تحتاج نطاقاً معروفاً.
    // لا يسمح API بالوصول من أي Origin عشوائي.
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

// ====================================================================
// [3] تسجيل خدمات النظام الحالية
// ====================================================================
builder.Services.AddScoped<NumberGeneratorService>();
builder.Services.AddScoped<CostCenterNumberService>();
builder.Services.AddScoped<CashBoxNumberService>();
builder.Services.AddScoped<ApprovalService>();
builder.Services.AddScoped<FinancialPolicyService>();
builder.Services.AddScoped<AccountNumberService>();
builder.Services.AddScoped<SystemScreenCatalogSeeder>();
builder.Services.AddScoped<VoucherReferenceDataSeeder>();
// جلسات الخادم تحفظ هوية الدخول بعد التحقق ولا تعتمد على بيانات مرسلة من الواجهة.
builder.Services.AddSingleton<ServerSessionService>();
// التوكنات والقفل وسجل المحاولات خدمات خادمية لا تنفذها واجهة سطح المكتب.
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<LoginSecurityService>();
// تفويض الشاشات والعمليات من جهة الخادم.
builder.Services.AddScoped<ScreenAuthorizationService>();
// يحل الإعدادات بحسب النظام ثم الشركة ثم الفرع ثم السنة المالية.
builder.Services.AddScoped<SettingsResolverService>();
// يخدم مسارات السنوات والفترات والتدقيق؛ تسجيله يمنع فشل إنشاء الـController عند شاشة الدخول.
builder.Services.AddScoped<AuditTrailService>();

// يثبت مخطط المصادقة الافتراضي حتى تُرجع Forbid/Challenge استجابات 403/401 سليمة
// بدلاً من InvalidOperationException وHTTP 500.
builder.Services
    .AddAuthentication(ServerSessionAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ServerSessionAuthenticationHandler>(
        ServerSessionAuthenticationHandler.SchemeName,
        _ => { });
builder.Services.AddAuthorization();


// ====================================================================
// [4] تسجيل خدمات المحرك المالي الجديد
// ====================================================================
builder.Services.AddScoped<VoucherValidationService>();
builder.Services.AddScoped<VoucherAuditService>();
builder.Services.AddScoped<VoucherApprovalService>();
builder.Services.AddScoped<FinancialVoucherService>();
builder.Services.AddScoped<JournalEntryInquiryService>();



builder.Services.AddScoped<VoucherPostingService>();

// ====================================================================
// [5] تسجيل Controllers وSwagger
// ====================================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// تجهيز كتالوج الشاشات مرة عند بدء الخدمة حتى تعمل صلاحيات الأدوار مع شجرة النظام.
using (var scope = app.Services.CreateScope())
{
    var screenCatalogSeeder = scope.ServiceProvider.GetRequiredService<SystemScreenCatalogSeeder>();
    await screenCatalogSeeder.EnsureSeededAsync();
    var voucherReferenceSeeder = scope.ServiceProvider.GetRequiredService<VoucherReferenceDataSeeder>();
    await voucherReferenceSeeder.EnsureSeededAsync();
}

// ====================================================================
// [6] تفعيل Swagger في بيئة التطوير
// ====================================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ====================================================================
// [7] خط معالجة الطلبات
// ====================================================================
app.UseHttpsRedirection();

app.UseCors("AlTayerERPClients");


// يحوّل JWT الموقّع وجلسة الخادم وسياق العمل إلى ClaimsPrincipal قبل التفويض.
app.UseAuthentication();

// نقطة الدخول والتجديد والصحة وقوائم شاشة الدخول فقط عامة. بقية API تتطلب
// JWT صالحاً وجلسة قابلة للإبطال وسياق شركة/فرع/سنة موثقاً من الخادم.
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    var isApiRequest = path.StartsWithSegments("/api");
    var isPublic = path.StartsWithSegments("/api/Auth/Login") ||
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

app.UseAuthorization();

app.MapControllers();

// نقطة حالة خفيفة لتفصل بين وصول تطبيق سطح المكتب للـ API وبين جاهزية قاعدة البيانات.
app.MapGet("/api/health", async (AppDbContext db) =>
{
    try
    {
        var databaseReady = await db.Database.CanConnectAsync();
        return databaseReady
            ? Results.Ok(new { api = "ready", database = "ready" })
            : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
    catch
    {
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
});

app.Run();