using FarmRegistry.Application.Contracts.Common;
using FarmRegistry.Infrastructure.Services;

namespace FarmRegistry.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        // Register user context (mock for now)
        services.AddScoped<IUserContext, MockUserContext>();

        return services;
    }
}