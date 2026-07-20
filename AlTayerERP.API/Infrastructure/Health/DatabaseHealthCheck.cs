using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AlTayerERP.API.Infrastructure.Health;

/// <summary>
/// يتحقق من جاهزية اتصال الـ API بقاعدة بيانات MySQL.
/// يستخدم في بوابة الجاهزية ولا ينفذ أي تعديل على البيانات.
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
            await using AsyncServiceScope scope =
                _scopeFactory.CreateAsyncScope();

            AppDbContext dbContext =
                scope.ServiceProvider.GetRequiredService<AppDbContext>();

            bool canConnect = await dbContext.Database
                .CanConnectAsync(cancellationToken);

            return canConnect
                ? HealthCheckResult.Healthy("Database connection is ready.")
                : HealthCheckResult.Unhealthy(
                    "Database connection is unavailable.");
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Database readiness health check failed.");

            return HealthCheckResult.Unhealthy(
                "Database readiness health check failed.",
                exception);
        }
    }
}
