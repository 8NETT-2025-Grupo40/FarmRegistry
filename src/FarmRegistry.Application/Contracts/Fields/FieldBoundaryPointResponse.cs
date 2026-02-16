namespace FarmRegistry.Application.Contracts.Fields;

public sealed record FieldBoundaryPointResponse
{
    public int Sequence { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }

    public FieldBoundaryPointResponse()
    {
    }

    public FieldBoundaryPointResponse(int sequence, double latitude, double longitude)
    {
        Sequence = sequence;
        Latitude = latitude;
        Longitude = longitude;
    }
}
