using AlTayerERP.API.Services;
using AlTayerERP.API.Services.Accounting;
using AlTayerERP.API.Services.Accounting.VoucherWorkflow;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// لا تُحفظ بيانات الاعتماد داخل Git. يمكن تمريرها عبر:
// ConnectionStrings__DefaultConnection أو User Secrets أو مدير أسرار البيئة.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "لم يتم العثور على نص الاتصال DefaultConnection. أضفه عبر User Secrets أو متغير البيئة ConnectionStrings__DefaultConnection.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)));

// تقييد CORS حسب البيئة. تطبيق WinForms لا يحتاج CORS، لكن إبقاء السياسة
// قابلة للضبط يتيح دعم عميل ويب موثوق عند الحاجة دون فتح API للعالم.
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ConfiguredOrigins", policy =>
    {
        if (allowedOrigins.Length == 0)
        {
            policy.SetIsOriginAllowed(_ => false);
            return;
        }

        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// خدمات النظام الحالية.
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
// تفويض الشاشات والعمليات من جهة الخادم.
builder.Services.AddScoped<ScreenAuthorizationService>();

// خدمات المحرك المالي.
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

// تجهيز كتالوج الشاشات والبيانات المرجعية عند بدء الخدمة.
using (var scope = app.Services.CreateScope())
{
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

app.UseHttpsRedirection();
app.UseCors("ConfiguredOrigins");

// حماية عامة: بعد الدخول لا يمكن استدعاء واجهات العمل دون رمز جلسة صادر من الخادم.
// تستثنى فقط نقاط الدخول وحالة الخدمة وقوائم شاشة الدخول المحدودة.
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    var isPublic = path.StartsWithSegments("/api/Auth/Login") ||
                   path.StartsWithSegments("/api/health") ||
                   (HttpMethods.IsGet(context.Request.Method) &&
                    (path.StartsWithSegments("/api/Branches/GetCompaniesLookup") ||
                     path.StartsWithSegments("/api/Branches/GetActiveBranchesLookup") ||
                     path.StartsWithSegments("/api/FiscalYears/Lookup")));

    if (isPublic)
    {
        await next();
        return;
    }

    var sessions = context.RequestServices.GetRequiredService<ServerSessionService>();
    if (!sessions.TryGet(context.Request.Headers["X-Session-Token"].ToString(), out var session))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new
        {
            message = "انتهت الجلسة أو أنها غير صالحة. سجل الدخول من جديد."
        });
        return;
    }

    context.Items["ServerSession"] = session;
    await next();
});

app.UseAuthorization();
app.MapControllers();

// نقطة حالة تفصل بين وصول العميل للـ API وجاهزية قاعدة البيانات.
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

public partial class Program;
