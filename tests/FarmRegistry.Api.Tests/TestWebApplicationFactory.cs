using FarmRegistry.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FarmRegistry.Api.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly InMemoryDatabaseRoot _databaseRoot = new();
    private readonly string _databaseName = $"FarmRegistryTestDb_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            Dictionary<string, string?> settings = new()
            {
                ["Authentication:AuthMode"] = "MOCK",
                ["ConnectionStrings:DefaultConnection"] = string.Empty
            };

            config.AddInMemoryCollection(settings);
        });

        builder.ConfigureTestServices(services =>
        {
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<FarmRegistryDbContext>))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<FarmRegistryDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName, _databaseRoot);
            });
        });
    }

    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FarmRegistryDbContext>();

        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }

    public HttpClient CreateCognitoAuthenticatedClient(Guid? ownerId = null)
    {
        var customFactory = WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                Dictionary<string, string?> settings = new()
                {
                    ["Authentication:AuthMode"] = "COGNITO",
                    ["COGNITO_REGION"] = "sa-east-1",
                    ["COGNITO_USER_POOL_ID"] = "sa-east-1_test",
                    ["COGNITO_CLIENT_ID"] = "test-client"
                };

                config.AddInMemoryCollection(settings);
            });

            builder.ConfigureTestServices(services =>
            {
                services
                    .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                        options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
            });
        });

        var client = customFactory.CreateClient();
        client.DefaultRequestHeaders.Add(
            TestAuthHandler.OwnerIdHeader,
            (ownerId ?? Guid.Parse("22222222-2222-2222-2222-222222222222")).ToString());

        return client;
    }

    public HttpClient CreateCognitoUnauthorizedClient()
    {
        var customFactory = WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                Dictionary<string, string?> settings = new()
                {
                    ["Authentication:AuthMode"] = "COGNITO",
                    ["COGNITO_REGION"] = "sa-east-1",
                    ["COGNITO_USER_POOL_ID"] = "sa-east-1_test",
                    ["COGNITO_CLIENT_ID"] = "test-client"
                };

                config.AddInMemoryCollection(settings);
            });
        });

        return customFactory.CreateClient();
    }
}
