using FarmRegistry.Domain.Entities;

namespace FarmRegistry.Application.Contracts.Fields;

public sealed record UpdateFieldRequest(
    Guid Id,
    Guid FarmId,
    string Code,
    string Name,
    decimal AreaHectares,
    string CropName,
    IReadOnlyCollection<FieldBoundaryPointRequest> BoundaryPoints,
    FieldStatus Status
);
