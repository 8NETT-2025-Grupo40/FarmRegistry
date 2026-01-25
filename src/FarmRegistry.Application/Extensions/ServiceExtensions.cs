using FluentValidation;
using FarmRegistry.Application.Mapping;
using FarmRegistry.Application.Services;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace FarmRegistry.Application.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Add AutoMapper
        services.AddAutoMapper(typeof(FarmRegistryProfile));

        // Add FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Add Application Services
        services.AddScoped<IFarmService, FarmService>();
        services.AddScoped<IFieldService, FieldService>();

        return services;
    }
}