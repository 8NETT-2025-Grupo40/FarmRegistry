using Asp.Versioning;
using FluentValidation.AspNetCore;
using FarmRegistry.Api.Extensions;
using FarmRegistry.Api.HealthChecks;
using FarmRegistry.Api.Middleware;
using FarmRegistry.Application.Configuration;
using FarmRegistry.Application.Extensions;
using FarmRegistry.Infrastructure.Extensions;
using FarmRegistry.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var problemDetails = new ValidationProblemDetails(context.ModelState)
            {
                Title = "Dados de entrada inválidos.",
                Detail = "Corrija os campos informados e tente novamente.",
                Status = StatusCodes.Status400BadRequest
            };

            return new BadRequestObjectResult(problemDetails);
        };
    });

builder.Services.AddFluentValidationAutoValidation();

// Add API versioning (NEW .NET 8 way)
builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new ApiVersion(1, 0);
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("version"),
        new HeaderApiVersionReader("X-Version"),
        new UrlSegmentApiVersionReader()
    );
})
.AddApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

// Add layer services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddApiServices(builder.Configuration);

// Add Health Checks
builder.Services.AddFarmRegistryHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Aplicar migrations automaticamente no startup (apenas para desenvolvimento)
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<FarmRegistryDbContext>();
    dbContext.Database.Migrate();
}

app.UseSwagger(options =>
{
    options.RouteTemplate = "registry/swagger/{documentName}/swagger.json";
});

app.UseSwaggerUI(options =>
{
    var apiVersionDescriptionProvider = app.Services.GetRequiredService<Asp.Versioning.ApiExplorer.IApiVersionDescriptionProvider>();
    foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint($"/registry/swagger/{description.GroupName}/swagger.json",
            description.GroupName.ToUpperInvariant());
    }

    options.RoutePrefix = "registry/swagger";
});

// Remover ou comentar UseHttpsRedirection para ambiente Docker
// app.UseHttpsRedirection();

app.UseRouting();

// Authentication & Authorization
var authOptions = builder.Configuration.GetSection(AuthenticationOptions.SectionName).Get<AuthenticationOptions>()
                 ?? new AuthenticationOptions();

if (authOptions.AuthMode?.Equals("MOCK", StringComparison.OrdinalIgnoreCase) == true)
{
    app.UseMiddleware<MockAuthenticationMiddleware>();
}
else
{
    app.UseAuthentication();
}

app.UseAuthorization();

app.MapHealthChecks("/registry/health", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("live"),
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    },
    ResponseWriter = HealthCheckResponseWriter.WriteAsync
}).AllowAnonymous();

app.MapHealthChecks("/registry/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready"),
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    },
    ResponseWriter = HealthCheckResponseWriter.WriteAsync
}).AllowAnonymous();

app.MapControllers();

app.Run();

public partial class Program;
