using FarmRegistry.Api.Authentication;
using FarmRegistry.Application.Common;
using FarmRegistry.Application.Configuration;
using FarmRegistry.Application.Contracts.Common;

namespace FarmRegistry.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        var authOptions = configuration.GetSection(AuthenticationOptions.SectionName).Get<AuthenticationOptions>()
                         ?? new AuthenticationOptions();

        services.Configure<AuthenticationOptions>(configuration.GetSection(AuthenticationOptions.SectionName));

        services.AddHttpContextAccessor();

        var authMode = Enum.TryParse<AuthenticationMode>(authOptions.AuthMode, ignoreCase: true, out var mode)
            ? mode
            : AuthenticationMode.Mock;

        switch (authMode)
        {
            case AuthenticationMode.Mock:
                services.AddScoped<IUserContext, MockAuthenticationProvider>();
                break;

            case AuthenticationMode.Cognito:
                services.AddCognitoUserAuthentication(configuration);
                services.AddScoped<IUserContext, CognitoAuthenticationProvider>();
                break;

            default:
                services.AddScoped<IUserContext, MockAuthenticationProvider>();
                break;
        }

        return services;
    }
}
