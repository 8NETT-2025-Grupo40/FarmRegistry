namespace FarmRegistry.Application.Configuration;

public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    public string AuthMode { get; set; } = "MOCK";
}
