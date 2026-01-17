using FarmRegistry.Domain.Entities;

namespace FarmRegistry.Application.Contracts.Fields;

public sealed record FieldResponse
{
    public Guid Id { get; init; }
    public Guid FarmId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal AreaHectares { get; init; }
    public FieldStatus Status { get; init; }
    public DateTime? StatusUpdatedAt { get; init; }
    public DateTime CreatedAt { get; init; }

    public FieldResponse() { }

    public FieldResponse(
        Guid id,
        Guid farmId,
        string code,
        string name,
        decimal areaHectares,
        FieldStatus status,
        DateTime? statusUpdatedAt,
        DateTime createdAt)
    {
        Id = id;
        FarmId = farmId;
        Code = code;
        Name = name;
        AreaHectares = areaHectares;
        Status = status;
        StatusUpdatedAt = statusUpdatedAt;
        CreatedAt = createdAt;
    }
}
