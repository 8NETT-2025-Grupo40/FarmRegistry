using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

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
                await WriteBadRequestAsync(
                    context,
                    "Dados de entrada inválidos.",
                    "O header X-Mock-User-Id não pode ser vazio quando informado.");

                return;
            }

            if (!Guid.TryParse(userIdString, out var userId))
            {
                await WriteBadRequestAsync(
                    context,
                    "Dados de entrada inválidos.",
                    "O header X-Mock-User-Id deve ser um GUID válido.");

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
            // Se não houver header, criar usuário padrão
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

    private static async Task WriteBadRequestAsync(HttpContext context, string title, string detail)
    {
        var problemDetails = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = StatusCodes.Status400BadRequest
        };

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }
}
