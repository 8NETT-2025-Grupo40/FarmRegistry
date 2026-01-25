using FarmRegistry.Application.Contracts.Repositories;
using FarmRegistry.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace FarmRegistry.Infrastructure.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Registrar as implementações existentes dos repositórios
        services.AddSingleton<IFarmRepository, FarmRepository>();
        services.AddSingleton<IFieldRepository, FieldRepository>();

        return services;
    }
}