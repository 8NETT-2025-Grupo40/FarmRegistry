using FarmRegistry.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FarmRegistry.Api.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly FarmRegistryDbContext _dbContext;

    public DatabaseHealthCheck(FarmRegistryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verifica se o banco está acessível
            await _dbContext.Database.CanConnectAsync(cancellationToken);

            // Verifica se as migrations estão aplicadas
            var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
            
            if (pendingMigrations.Any())
            {
                return HealthCheckResult.Degraded(
                    description: $"Database has {pendingMigrations.Count()} pending migrations",
                    data: new Dictionary<string, object>
                    {
                        { "pendingMigrations", pendingMigrations }
                    }
                );
            }

            return HealthCheckResult.Healthy("Database is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                description: "Database connection failed",
                exception: ex
            );
        }
    }
}