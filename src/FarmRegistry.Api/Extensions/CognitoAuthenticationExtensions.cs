using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace FarmRegistry.Api.Extensions;

/// <summary>
/// Extensões para autenticação de usuários via AWS Cognito User Pool.
/// Use para endpoints acessados por usuários logados (login/senha, redes sociais, etc).
/// </summary>
public static class CognitoUserAuthenticationExtensions
{
    /// <summary>
    /// Configura autenticação JWT para tokens de usuários do Cognito.
    /// </summary>
    /// <remarks>
    /// <para><b>Configurações necessárias:</b></para>
    /// <list type="bullet">
    ///   <item><c>COGNITO_REGION</c> - Região AWS (ex: us-east-1)</item>
    ///   <item><c>COGNITO_USER_POOL_ID</c> - ID do User Pool (ex: us-east-1_abc123)</item>
    ///   <item><c>COGNITO_CLIENT_ID</c> - ID do App Client para login de usuários</item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddCognitoUserAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var region = configuration["COGNITO_REGION"];
        var userPoolId = configuration["COGNITO_USER_POOL_ID"];
        var clientId = configuration["COGNITO_CLIENT_ID"];

        if (string.IsNullOrWhiteSpace(region))
        {
            throw new InvalidOperationException("Configuração ausente: COGNITO_REGION");
        }

        if (string.IsNullOrWhiteSpace(userPoolId))
        {
            throw new InvalidOperationException("Configuração ausente: COGNITO_USER_POOL_ID");
        }

        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new InvalidOperationException("Configuração ausente: COGNITO_CLIENT_ID");
        }

        // Authority = URL do Cognito que emite os tokens (issuer)
        // O middleware usa essa URL para buscar as chaves públicas e validar assinaturas
        var authority = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = authority,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ClockSkew = TimeSpan.FromMinutes(2)
                };

                // Validações customizadas após o token ser decodificado e assinatura verificada
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = ctx =>
                    {
                        // Cognito emite 2 tipos de token: "access" e "id"
                        // Para APIs, usamos sempre access token
                        var tokenUse = ctx.Principal?.FindFirst("token_use")?.Value;
                        if (!string.Equals(tokenUse, "access", StringComparison.Ordinal))
                        {
                            ctx.Fail("invalid token_use");
                            return Task.CompletedTask;
                        }

                        // Valida que o token foi emitido pelo App Client correto
                        var clientIdClaim = ctx.Principal?.FindFirst("client_id")?.Value;
                        if (!string.Equals(clientIdClaim, clientId, StringComparison.Ordinal))
                        {
                            ctx.Fail("invalid client_id");
                            return Task.CompletedTask;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        // FallbackPolicy = política aplicada a TODOS os endpoints que não têm [Authorize] explícito
        // Para liberar um endpoint, use [AllowAnonymous]
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}
