using FarmRegistry.Application.Common;
using FarmRegistry.Application.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Asp.Versioning.ApiExplorer;

namespace FarmRegistry.Api.Extensions;

public class ConfigureSwaggerOptions : IConfigureNamedOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;
    private readonly AuthenticationMode _authenticationMode;

    public ConfigureSwaggerOptions(
        IApiVersionDescriptionProvider provider,
        IOptions<AuthenticationOptions> authenticationOptions)
    {
        _provider = provider;

        var authMode = authenticationOptions.Value.AuthMode;
        _authenticationMode = Enum.TryParse<AuthenticationMode>(authMode, ignoreCase: true, out var mode)
            ? mode
            : AuthenticationMode.Mock;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateVersionInfo(description));
        }

        if (_authenticationMode == AuthenticationMode.Mock)
        {
            options.AddSecurityDefinition("MockAuth", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Name = "X-Mock-User-Id",
                Description = "Mock User ID for authentication (GUID format). Leave empty to use default user."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "MockAuth"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            return;
        }

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Description = "JWT access token do Cognito."
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    }

    public void Configure(string? name, SwaggerGenOptions options) => Configure(options);

    private static OpenApiInfo CreateVersionInfo(ApiVersionDescription desc)
    {
        var info = new OpenApiInfo()
        {
            Title = "FarmRegistry API",
            Version = desc.ApiVersion.ToString(),
            Description = "API para gerenciamento de fazendas e talhões.",
            Contact = new OpenApiContact
            {
                Name = "FarmRegistry Team",
                Email = "farmregistry@example.com"
            }
        };

        if (desc.IsDeprecated)
        {
            info.Description += " Esta versão da API foi descontinuada.";
        }

        return info;
    }
}
