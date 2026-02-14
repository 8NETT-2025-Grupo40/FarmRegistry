using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace FarmRegistry.Api.HealthChecks;

internal static class HealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var payload = new HealthCheckResponseDto(
            report.Status.ToString(),
            report.Entries.Select(entry => new HealthCheckEntryDto(
                entry.Key,
                entry.Value.Status.ToString(),
                entry.Value.Description,
                entry.Value.Duration.TotalMilliseconds)).ToArray(),
            report.TotalDuration.TotalMilliseconds);

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }
}
