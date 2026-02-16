using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace FarmRegistry.Api.Tests.Controllers;

public class AuthenticationModeTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AuthenticationModeTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task MockMode_GetFarmsWithoutHeader_ShouldReturnOk()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/registry/api/v1/farms");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task MockMode_GetFarmsWithInvalidHeader_ShouldReturnBadRequest()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Mock-User-Id", "invalid-guid");

        var response = await client.GetAsync("/registry/api/v1/farms");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Title.Should().Be("Dados de entrada inválidos.");
        problem.Detail.Should().Contain("GUID");
    }

    [Fact]
    public async Task MockMode_GetFarmsWithEmptyHeader_ShouldReturnBadRequestProblemDetails()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("X-Mock-User-Id", "   ");

        var response = await client.GetAsync("/registry/api/v1/farms");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Title.Should().Be("Dados de entrada inválidos.");
        problem.Detail.Should().Contain("não pode ser vazio");
    }

    [Fact]
    public async Task CognitoMode_GetFarmsWithoutToken_ShouldReturnUnauthorized()
    {
        using var client = _factory.CreateCognitoUnauthorizedClient();

        var response = await client.GetAsync("/registry/api/v1/farms");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CognitoMode_GetFarmsWithAuthenticatedUser_ShouldReturnOk()
    {
        using var client = _factory.CreateCognitoAuthenticatedClient();

        var response = await client.GetAsync("/registry/api/v1/farms");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
