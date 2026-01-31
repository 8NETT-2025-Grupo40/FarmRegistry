namespace FarmRegistry.Application.Configuration;

public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    public string AuthMode { get; set; } = "MOCK";
    public JwtOptions Jwt { get; set; } = new();
}

public sealed class JwtOptions
{
    public string Authority { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string ValidIssuer { get; set; } = string.Empty;
    public bool ValidateIssuer { get; set; } = true;
    public bool ValidateAudience { get; set; } = true;
    public bool ValidateLifetime { get; set; } = true;
    public bool RequireHttpsMetadata { get; set; } = true;
}