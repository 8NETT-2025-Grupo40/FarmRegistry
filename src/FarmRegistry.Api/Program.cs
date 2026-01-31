using FarmRegistry.Api.Extensions;
using FarmRegistry.Api.Middleware;
using FarmRegistry.Application.Configuration;
using FarmRegistry.Application.Extensions;
using FarmRegistry.Infrastructure.Extensions;
using Asp.Versioning;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

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
builder.Services.AddInfrastructureServices();
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var apiVersionDescriptionProvider = app.Services.GetRequiredService<Asp.Versioning.ApiExplorer.IApiVersionDescriptionProvider>();
        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        }

        // Configure Swagger UI to be served at root in development
        options.RoutePrefix = string.Empty; // Serve Swagger UI at root "/"
    });

    // Redirect root URL to Swagger UI
    app.MapGet("/", () => Results.Redirect("/swagger"))
       .ExcludeFromDescription();
}

app.UseHttpsRedirection();
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
app.MapControllers();

app.Run();
