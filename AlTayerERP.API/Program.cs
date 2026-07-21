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