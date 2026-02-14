using System.Net;

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
