using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AlTayerERP.API.Health;

/// <summary>
/// يكتب استجابة فحوصات الصحة بصيغة JSON موحدة وآمنة للاستخدام التشغيلي.
/// </summary>
public static class HealthCheckResponseWriter
{
    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var payload = new
        {
            status = report.Status.ToString(),
            timestampUtc = DateTimeOffset.UtcNow,
            traceId = context.TraceIdentifier,
            totalDurationMs = Math.Round(report.TotalDuration.TotalMilliseconds, 2),
            checks = report.Entries
                .OrderBy(entry => entry.Key)
                .Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description,
                    durationMs = Math.Round(entry.Value.Duration.TotalMilliseconds, 2)
                })
        };

        return context.Response.WriteAsJsonAsync(payload);
    }
}
