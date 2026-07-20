using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AlTayerERP.API.Health;

/// <summary>
/// يفحص جاهزية الاتصال بقاعدة البيانات دون ربط تشغيل الخدمة بنجاح الاتصال وقت الإقلاع.
/// </summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(
        IServiceScopeFactory scopeFactory,
        ILogger<DatabaseHealthCheck> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

            return canConnect
                ? HealthCheckResult.Healthy("قاعدة البيانات متاحة.")
                : HealthCheckResult.Unhealthy("قاعدة البيانات غير متاحة.");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Database readiness check failed.");
            return HealthCheckResult.Unhealthy("قاعدة البيانات غير متاحة.", exception);
        }
    }
}
