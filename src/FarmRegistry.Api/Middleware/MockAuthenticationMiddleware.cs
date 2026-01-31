using System.Security.Claims;

namespace FarmRegistry.Api.Middleware;

public sealed class MockAuthenticationMiddleware
{
    private const string MockUserIdHeader = "X-Mock-User-Id";
    private readonly RequestDelegate _next;

    public MockAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(MockUserIdHeader, out var userIdValue))
        {
            var userIdString = userIdValue.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(userIdString))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("X-Mock-User-Id header cannot be empty when provided");
                return;
            }

            if (!Guid.TryParse(userIdString, out var userId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("X-Mock-User-Id header must be a valid GUID");
                return;
            }

            // Criar identity e claims para o usuário mock
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, $"Mock User ({userId})"),
                new Claim("sub", userId.ToString())
            };

            var identity = new ClaimsIdentity(claims, "Mock");
            context.User = new ClaimsPrincipal(identity);
        }
        else
        {
            // Se não há header, criar usuário padrão
            var defaultUserId = "11111111-1111-1111-1111-111111111111";
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, defaultUserId),
                new Claim(ClaimTypes.Name, "Mock User (Default)"),
                new Claim("sub", defaultUserId)
            };

            var identity = new ClaimsIdentity(claims, "Mock");
            context.User = new ClaimsPrincipal(identity);
        }

        await _next(context);
    }
}