using FarmRegistry.Application.Contracts.Repositories;
using FarmRegistry.Infrastructure.Persistence;
using FarmRegistry.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FarmRegistry.Infrastructure.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Use SQL Server do Docker
        services.AddDbContext<FarmRegistryDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Registre os repositórios reais
        services.AddScoped<IFarmRepository, FarmRepository>();
        services.AddScoped<IFieldRepository, FieldRepository>();

        return services;
    }
}