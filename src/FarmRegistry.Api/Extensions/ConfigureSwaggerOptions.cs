using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Asp.Versioning.ApiExplorer;

namespace FarmRegistry.Api.Extensions;

public class ConfigureSwaggerOptions : IConfigureNamedOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateVersionInfo(description));
        }

        // Adicionar suporte para header X-Mock-User-Id
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
                new string[] { }
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