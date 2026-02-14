using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FarmRegistry.Api.Tests;

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";
    public const string OwnerIdHeader = "X-Test-Owner-Id";
    private static readonly Guid DefaultOwnerId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var ownerId = DefaultOwnerId;
        if (Request.Headers.TryGetValue(OwnerIdHeader, out var ownerIdHeader) &&
            Guid.TryParse(ownerIdHeader.FirstOrDefault(), out var parsedOwnerId))
        {
            ownerId = parsedOwnerId;
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, ownerId.ToString()),
            new Claim("sub", ownerId.ToString()),
            new Claim(ClaimTypes.Name, "Test User")
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
