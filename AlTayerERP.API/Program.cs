using AlTayerERP.API.Health;
using AlTayerERP.API.Infrastructure;
using AlTayerERP.API.Services;
using AlTayerERP.API.Services.Accounting;
using AlTayerERP.API.Services.Accounting.VoucherWorkflow;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// ====================================================================
// [1] إعداد قاعدة البيانات
// ====================================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "نص الاتصال غير مضبوط. عيّن ConnectionStrings__DefaultConnection كمتغير بيئة " +
        "أو استخدم User Secrets أثناء التطوير المحلي.");
}

var configuredServerVersion =
    builder.Configuration["Database:ServerVersion"] ?? "8.0.0-mysql";

var serverVersion = ServerVersion.Parse(configuredServerVersion);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        serverVersion,
        mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
    }
});

// ====================================================================
// [2] CORS: تطبيق سطح المكتب لا يحتاج CORS، وتُحدد الأصول فقط لعملاء الويب.
// ====================================================================
var allowedOrigins =
    builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ConfiguredClients", policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod();

        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins);
        }
        else if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin();
        }
    });
});

// ====================================================================
// [3] معالجة الأخطاء وفحوصات الصحة
// ====================================================================
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] =
            context.HttpContext.TraceIdentifier;

        context.ProblemDetails.Extensions["timestampUtc"] =
            DateTimeOffset.UtcNow;
    };
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services
    .AddHealthChecks()
    .AddCheck(
        "self",
        () => HealthCheckResult.Healthy("خدمة API تعمل."),
        tags: new[] { "live" })
    .AddCheck<DatabaseHealthCheck>(
        "database",
        tags: new[] { "ready" });

// ====================================================================
// [4] خدمات النظام الحالية
// ====================================================================
builder.Services.AddScoped<NumberGeneratorService>();
builder.Services.AddScoped<CostCenterNumberService>();
builder.Services.AddScoped<CashBoxNumberService>();
builder.Services.AddScoped<ApprovalService>();
builder.Services.AddScoped<FinancialPolicyService>();
builder.Services.AddScoped<AccountNumberService>();

// ====================================================================
// [5] خدمات المحرك المالي
// ====================================================================
builder.Services.AddScoped<VoucherValidationService>();
builder.Services.AddScoped<VoucherAuditService>();
builder.Services.AddScoped<VoucherApprovalService>();
builder.Services.AddScoped<FinancialVoucherService>();
builder.Services.AddScoped<JournalEntryInquiryService>();
builder.Services.AddScoped<VoucherPostingService>();

// ====================================================================
// [6] Controllers وSwagger
// ====================================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ====================================================================
// [7] خط معالجة الطلبات
// ====================================================================
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("ConfiguredClients");
app.UseAuthorization();

app.MapHealthChecks(
    "/health/live",
    new HealthCheckOptions
    {
        Predicate = registration => registration.Tags.Contains("live"),
        ResponseWriter = HealthCheckResponseWriter.WriteAsync
    })
    .AllowAnonymous();

app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions
    {
        Predicate = registration => registration.Tags.Contains("ready"),
        ResponseWriter = HealthCheckResponseWriter.WriteAsync
    })
    .AllowAnonymous();

app.MapControllers();
app.Run();
