using System.Net;
using System.Net.Http.Json;

namespace FarmRegistry.Api.Tests.Controllers;

public class HealthControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetLiveness_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/registry/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<HealthResponse>();
        payload.Should().NotBeNull();
        payload!.Status.Should().Be("Healthy");
        payload.Checks.Should().Contain(check => check.Name == "self");
    }

    [Fact]
    public async Task GetReadiness_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/registry/ready");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<HealthResponse>();
        payload.Should().NotBeNull();
        payload!.Status.Should().Be("Healthy");
        payload.Checks.Should().Contain(check => check.Name == "database");
    }

    [Fact]
    public async Task Swagger_ShouldContainApiEndpoints()
    {
        var response = await _client.GetAsync("/registry/swagger/v1/swagger.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("/registry/api/v1/farms");
        content.Should().Contain("/registry/api/v1/fields");
    }

    private sealed record HealthResponse(string Status, IReadOnlyList<HealthCheckEntry> Checks, double TotalDuration);

    private sealed record HealthCheckEntry(string Name, string Status, string? Description, double Duration);
}
