using FarmRegistry.Api.Authentication;
using FarmRegistry.Application.Common;
using FarmRegistry.Application.Configuration;
using FarmRegistry.Application.Contracts.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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
                ConfigureJwtAuthentication(services, authOptions.Jwt);
                services.AddScoped<IUserContext, CognitoAuthenticationProvider>();
                break;

            default:
                services.AddScoped<IUserContext, MockAuthenticationProvider>();
                break;
        }

        return services;
    }

    private static void ConfigureJwtAuthentication(IServiceCollection services, JwtOptions jwtOptions)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = jwtOptions.Authority;
                options.Audience = jwtOptions.Audience;
                options.RequireHttpsMetadata = jwtOptions.RequireHttpsMetadata;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = jwtOptions.ValidateIssuer,
                    ValidateAudience = jwtOptions.ValidateAudience,
                    ValidateLifetime = jwtOptions.ValidateLifetime,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.ValidIssuer,
                    ValidAudience = jwtOptions.Audience,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();
    }
}