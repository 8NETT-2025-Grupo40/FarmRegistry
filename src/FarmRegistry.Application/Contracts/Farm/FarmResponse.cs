namespace FarmRegistry.Application.Contracts.Farms;

public sealed record FarmResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }

    public FarmResponse() { }

    public FarmResponse(
        Guid id,
        string name,
        string city,
        string state,
        DateTime createdAt)
    {
        Id = id;
        Name = name;
        City = city;
        State = state;
        CreatedAt = createdAt;
    }
}