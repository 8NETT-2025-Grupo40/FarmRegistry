using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FarmRegistry.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace FarmRegistry.Api.Tests.Controllers;

public class ResourceFlowTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _factory;
    private readonly HttpClient _client;

    public ResourceFlowTests(IntegrationTestFixture factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateFarm_WithTooLongName_ShouldReturnValidationProblem()
    {
        var request = new
        {
            name = new string('A', 121),
            city = "Ribeirao Preto",
            state = "SP"
        };

        var response = await _client.PostAsJsonAsync("/registry/api/v1/farms", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(content);

        json.RootElement.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.TryGetProperty("Name", out _).Should().BeTrue();
    }

    [Fact]
    public async Task CreateField_WithStatusZero_ShouldReturnValidationProblem()
    {
        var farmId = await CreateFarmAsync(_client);

        var request = new
        {
            farmId,
            code = "TALHAO-01",
            name = "Talhao Norte",
            areaHectares = 10.5m,
            cropName = "Milho",
            boundaryPoints = new[]
            {
                new { latitude = -21.2211, longitude = -47.8301 },
                new { latitude = -21.2208, longitude = -47.8296 },
                new { latitude = -21.2215, longitude = -47.8291 }
            },
            status = 0
        };

        var response = await _client.PostAsJsonAsync("/registry/api/v1/fields", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(content);

        json.RootElement.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.TryGetProperty("Status", out _).Should().BeTrue();
    }

    [Fact]
    public async Task CreateField_WithNonExistentFarm_ShouldReturnNotFound()
    {
        var request = new
        {
            farmId = Guid.NewGuid(),
            code = "TALHAO-01",
            name = "Talhao Norte",
            areaHectares = 10.5m,
            cropName = "Milho",
            boundaryPoints = new[]
            {
                new { latitude = -21.2211, longitude = -47.8301 },
                new { latitude = -21.2208, longitude = -47.8296 },
                new { latitude = -21.2215, longitude = -47.8291 }
            },
            status = 1
        };

        var response = await _client.PostAsJsonAsync("/registry/api/v1/fields", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateFarm_WithNonExistentId_ShouldReturnNotFound()
    {
        var farmId = Guid.NewGuid();
        var request = new
        {
            id = farmId,
            name = "Fazenda Inexistente",
            city = "Campinas",
            state = "SP"
        };

        var response = await _client.PutAsJsonAsync($"/registry/api/v1/farms/{farmId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteField_ShouldPhysicallyDeleteFieldAndBoundaryPoints()
    {
        var farmId = await CreateFarmAsync(_client);
        var fieldId = await CreateFieldAsync(_client, farmId, "TALHAO-DEL-01");

        var deleteResponse = await _client.DeleteAsync($"/registry/api/v1/fields/{fieldId}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/registry/api/v1/fields/{fieldId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FarmRegistryDbContext>();

        dbContext.Fields.Any(field => field.FieldId == fieldId).Should().BeFalse();
        dbContext.FieldBoundaryPoints.Any(point => point.FieldId == fieldId).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteFarm_ShouldPhysicallyDeleteFarmAndRelatedFields()
    {
        var farmId = await CreateFarmAsync(_client);
        var fieldId = await CreateFieldAsync(_client, farmId, "TALHAO-DEL-02");

        var deleteResponse = await _client.DeleteAsync($"/registry/api/v1/farms/{farmId}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getFarmResponse = await _client.GetAsync($"/registry/api/v1/farms/{farmId}");
        getFarmResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FarmRegistryDbContext>();

        dbContext.Farms.Any(farm => farm.FarmId == farmId).Should().BeFalse();
        dbContext.Fields.Any(field => field.FieldId == fieldId).Should().BeFalse();
        dbContext.FieldBoundaryPoints.Any(point => point.FieldId == fieldId).Should().BeFalse();
    }

    [Fact]
    public async Task CreateField_WithInactiveFarm_ShouldReturnConflict()
    {
        var farmId = await CreateFarmAsync(_client);
        await DeactivateFarmAsync(_client, farmId);

        var request = CreateFieldRequest(farmId, "TALHAO-CONFLICT-01");
        var response = await _client.PostAsJsonAsync("/registry/api/v1/fields", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Detail.Should().Contain("inativa");
    }

    [Fact]
    public async Task GetFields_WithInactiveFarm_ShouldReturnOk()
    {
        var farmId = await CreateFarmAsync(_client);
        await CreateFieldAsync(_client, farmId, "TALHAO-INACTIVE-READ-01");
        await DeactivateFarmAsync(_client, farmId);

        var response = await _client.GetAsync($"/registry/api/v1/fields?farmId={farmId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateField_WithInactiveFarm_ShouldReturnConflict()
    {
        var farmId = await CreateFarmAsync(_client);
        var fieldId = await CreateFieldAsync(_client, farmId, "TALHAO-CONFLICT-02");
        await DeactivateFarmAsync(_client, farmId);

        var request = new
        {
            id = fieldId,
            farmId,
            code = "TALHAO-CONFLICT-02",
            name = "Talhao Atualizado",
            areaHectares = 11.0m,
            cropName = "Soja",
            boundaryPoints = new[]
            {
                new { latitude = -21.2211, longitude = -47.8301 },
                new { latitude = -21.2208, longitude = -47.8296 },
                new { latitude = -21.2215, longitude = -47.8291 }
            },
            status = 2
        };

        var response = await _client.PutAsJsonAsync($"/registry/api/v1/fields/{fieldId}", request);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ActivateField_WithInactiveFarm_ShouldReturnConflict()
    {
        var farmId = await CreateFarmAsync(_client);
        var fieldId = await CreateFieldAsync(_client, farmId, "TALHAO-CONFLICT-03");

        var deactivateFieldResponse = await _client.PatchAsync($"/registry/api/v1/fields/{fieldId}/deactivate", null);
        deactivateFieldResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await DeactivateFarmAsync(_client, farmId);

        var response = await _client.PatchAsync($"/registry/api/v1/fields/{fieldId}/activate", null);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task DeleteField_WithInactiveFarm_ShouldReturnConflict()
    {
        var farmId = await CreateFarmAsync(_client);
        var fieldId = await CreateFieldAsync(_client, farmId, "TALHAO-CONFLICT-04");
        await DeactivateFarmAsync(_client, farmId);

        var response = await _client.DeleteAsync($"/registry/api/v1/fields/{fieldId}");
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetFieldById_WithDifferentOwner_ShouldReturnNotFound()
    {
        var farmId = await CreateFarmAsync(_client);
        var fieldId = await CreateFieldAsync(_client, farmId, "TALHAO-OWNER-01");

        using var otherClient = _factory.CreateClient();
        otherClient.DefaultRequestHeaders.Add("X-Mock-User-Id", Guid.NewGuid().ToString());

        var response = await otherClient.GetAsync($"/registry/api/v1/fields/{fieldId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static async Task DeactivateFarmAsync(HttpClient client, Guid farmId)
    {
        var response = await client.PatchAsync($"/registry/api/v1/farms/{farmId}/deactivate", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task<Guid> CreateFarmAsync(HttpClient client)
    {
        var request = new
        {
            name = $"Fazenda {Guid.NewGuid():N}",
            city = "Ribeirao Preto",
            state = "SP"
        };

        var response = await client.PostAsJsonAsync("/registry/api/v1/farms", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content.ReadFromJsonAsync<IdResponse>();
        payload.Should().NotBeNull();

        return payload!.Id;
    }

    private static async Task<Guid> CreateFieldAsync(HttpClient client, Guid farmId, string code)
    {
        var request = CreateFieldRequest(farmId, code);

        var response = await client.PostAsJsonAsync("/registry/api/v1/fields", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content.ReadFromJsonAsync<IdResponse>();
        payload.Should().NotBeNull();

        return payload!.Id;
    }

    private static object CreateFieldRequest(Guid farmId, string code)
    {
        return new
        {
            farmId,
            code,
            name = "Talhao de Teste",
            areaHectares = 10.5m,
            cropName = "Milho",
            boundaryPoints = new[]
            {
                new { latitude = -21.2211, longitude = -47.8301 },
                new { latitude = -21.2208, longitude = -47.8296 },
                new { latitude = -21.2215, longitude = -47.8291 }
            },
            status = 1
        };
    }

    private sealed record IdResponse(Guid Id);
}
