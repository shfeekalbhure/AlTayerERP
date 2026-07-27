using AlTayerERP.API.Services;
using AlTayerERP.API.Services.Accounting;
using AlTayerERP.API.Services.Accounting.VoucherWorkflow;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using AlTayerERP.API.Security;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AlTayerERPClients", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7021",
                "http://localhost:5021",
                "https://localhost:7022",
                "http://localhost:5022",
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

using (var scope = app.Services.CreateScope())
{
    var paymentRequestSchema = scope.ServiceProvider.GetRequiredService<PaymentRequestSchemaInitializer>();
    await paymentRequestSchema.EnsureCreatedAsync();

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

app.UseAuthorization();
app.MapControllers();

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