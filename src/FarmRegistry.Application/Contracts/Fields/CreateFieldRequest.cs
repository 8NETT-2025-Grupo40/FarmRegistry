using FarmRegistry.Domain.Entities;

namespace FarmRegistry.Application.Contracts.Fields;

public sealed record CreateFieldRequest(
    Guid FarmId,
    string Code,
    string Name,
    decimal AreaHectares,
    FieldStatus Status
);
