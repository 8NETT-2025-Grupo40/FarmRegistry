namespace FarmRegistry.Application.Contracts.Farms;

public sealed record UpdateFarmRequest(
    Guid Id,
    string Name,
    string City,
    string State
);
