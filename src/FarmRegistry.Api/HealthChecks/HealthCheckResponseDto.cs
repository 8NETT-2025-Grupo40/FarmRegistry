namespace FarmRegistry.Api.HealthChecks;

public sealed record HealthCheckResponseDto(
    string Status,
    IReadOnlyList<HealthCheckEntryDto> Checks,
    double TotalDuration);

public sealed record HealthCheckEntryDto(
    string Name,
    string Status,
    string? Description,
    double Duration);
