using FarmRegistry.Api.HealthChecks;
using FarmRegistry.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FarmRegistry.Api.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddFarmRegistryHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck(
                name: "self",
                check: () => HealthCheckResult.Healthy(),
                tags: ["live"])
            .AddDbContextCheck<FarmRegistryDbContext>(
                name: "database",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "db", "sql", "ready" }
            )
            .AddCheck<DatabaseHealthCheck>(
                name: "database_migrations",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "db", "migrations", "ready" }
            );

        return services;
    }
}
