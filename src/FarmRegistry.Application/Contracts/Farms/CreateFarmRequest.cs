namespace FarmRegistry.Application.Contracts.Farms;

public sealed record CreateFarmRequest(
    string Name,
    string City,
    string State
);
