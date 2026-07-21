using AlTayerERP.API.Services;
using AlTayerERP.API.Services.Accounting;
using AlTayerERP.API.Services.Accounting.VoucherWorkflow;
using AlTayerERP.Infrastructure.Data;
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
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
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

app.UseCors("AllowAll");


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
        await context.Response.WriteAsJsonAsync(new { message = "انتهت الجلسة أو أنها غير صالحة. سجل الدخول من جديد." });
        return;
    }

    context.Items["ServerSession"] = session;
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