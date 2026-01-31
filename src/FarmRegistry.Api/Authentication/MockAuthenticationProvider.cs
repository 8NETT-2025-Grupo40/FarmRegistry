using FarmRegistry.Application.Contracts.Common;
using Microsoft.AspNetCore.Http;

namespace FarmRegistry.Api.Authentication;

public sealed class MockAuthenticationProvider : IUserContext
{
    private const string MockUserIdHeader = "X-Mock-User-Id";
    
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MockAuthenticationProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid OwnerId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Request.Headers.TryGetValue(MockUserIdHeader, out var userIdValue) == true)
            {
                var userIdString = userIdValue.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(userIdString) && Guid.TryParse(userIdString, out var userId))
                {
                    return userId;
                }
            }

            // Fallback para um usuário padrão se o header não estiver presente ou for inválido
            return Guid.Parse("11111111-1111-1111-1111-111111111111");
        }
    }

    public string? UserName
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Request.Headers.TryGetValue(MockUserIdHeader, out var userIdValue) == true)
            {
                var userIdString = userIdValue.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(userIdString))
                {
                    return $"Mock User ({userIdString})";
                }
            }
            return "Mock User (Default)";
        }
    }

    public bool IsAuthenticated => true;
}