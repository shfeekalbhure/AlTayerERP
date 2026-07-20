using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace AlTayerERP.API.Health;

/// <summary>
/// يكتب نتيجة فحوصات الصحة بصيغة JSON ثابتة تصلح للمراقبة والدعم الفني.
/// </summary>
public static class HealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var response = new
        {
            status = report.Status.ToString(),
            durationMs = Math.Round(report.TotalDuration.TotalMilliseconds, 2),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                durationMs = Math.Round(entry.Value.Duration.TotalMilliseconds, 2),
                description = entry.Value.Description
            })
        };

        return context.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptions));
    }
}
