using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AlTayerERP.API.Infrastructure.Health;

/// <summary>
/// يخرج نتيجة فحوص الصحة بصيغة JSON موحدة وقابلة للقراءة من أدوات المراقبة.
/// </summary>
public static class HealthCheckResponseWriter
{
    public static Task WriteAsync(
        HttpContext context,
        HealthReport report)
    {
        context.Response.ContentType =
            "application/json; charset=utf-8";

        var response = new
        {
            status = report.Status.ToString(),
            traceId = context.TraceIdentifier,
            totalDurationMs = Math.Round(
                report.TotalDuration.TotalMilliseconds,
                2),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                durationMs = Math.Round(
                    entry.Value.Duration.TotalMilliseconds,
                    2)
            })
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
