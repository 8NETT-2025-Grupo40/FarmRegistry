using FarmRegistry.Application.Contracts.Common;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace FarmRegistry.Api.Authentication;

public sealed class CognitoAuthenticationProvider : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CognitoAuthenticationProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid OwnerId
    {
        get
        {
            Console.WriteLine("=== CognitoAuthenticationProvider.OwnerId ===");
            var context = _httpContextAccessor.HttpContext;
            var user = context?.User;

            Console.WriteLine($"User exists: {user != null}");
            Console.WriteLine($"Identity exists: {user?.Identity != null}");
            Console.WriteLine($"IsAuthenticated: {user?.Identity?.IsAuthenticated}");

            if (user?.Identity?.IsAuthenticated == true)
            {
                // Primeiro tenta o claim 'sub' (padrão JWT)
                var subClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("sub");
                if (subClaim != null && Guid.TryParse(subClaim.Value, out var subGuid))
                {
                    return subGuid;
                }

                // Fallback para email se 'sub' não for um GUID válido
                var emailClaim = user.FindFirst(ClaimTypes.Email) ?? user.FindFirst("email");
                if (emailClaim != null)
                {
                    // Gerar um GUID determinístico baseado no email
                    var emailBytes = System.Text.Encoding.UTF8.GetBytes(emailClaim.Value);
                    var hash = System.Security.Cryptography.SHA256.HashData(emailBytes);
                    var guidBytes = new byte[16];
                    Array.Copy(hash, guidBytes, 16);
                    return new Guid(guidBytes);
                }
            }

            // Fallback para usuário anônimo
            return Guid.Empty;
        }
    }

    public string? UserName
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            var user = context?.User;

            if (user?.Identity?.IsAuthenticated == true)
            {
                var nameClaim = user.FindFirst(ClaimTypes.Name) ?? user.FindFirst("preferred_username");
                if (nameClaim != null)
                {
                    return nameClaim.Value;
                }

                var emailClaim = user.FindFirst(ClaimTypes.Email) ?? user.FindFirst("email");
                if (emailClaim != null)
                {
                    return emailClaim.Value;
                }
            }

            return null;
        }
    }

    public bool IsAuthenticated => 
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
}