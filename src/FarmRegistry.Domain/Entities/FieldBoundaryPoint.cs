using FarmRegistry.Domain.Common;

namespace FarmRegistry.Domain.Entities;

public sealed class FieldBoundaryPoint
{
    public Guid FieldBoundaryPointId { get; }
    public Guid FieldId { get; private set; }
    public int Sequence { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    private FieldBoundaryPoint()
    {
    }

    public FieldBoundaryPoint(Guid fieldId, int sequence, double latitude, double longitude)
    {
        if (fieldId == Guid.Empty)
        {
            throw new DomainException("FieldId é obrigatório.");
        }

        ValidateSequence(sequence);
        ValidateLatitude(latitude);
        ValidateLongitude(longitude);

        FieldBoundaryPointId = Guid.NewGuid();
        FieldId = fieldId;
        Sequence = sequence;
        Latitude = latitude;
        Longitude = longitude;
    }

    private static void ValidateSequence(int sequence)
    {
        if (sequence <= 0)
        {
            throw new DomainException("A sequência do ponto deve ser maior que zero.");
        }
    }

    private static void ValidateLatitude(double latitude)
    {
        if (latitude < -90 || latitude > 90)
        {
            throw new DomainException("Latitude deve estar entre -90 e 90 graus.");
        }
    }

    private static void ValidateLongitude(double longitude)
    {
        if (longitude < -180 || longitude > 180)
        {
            throw new DomainException("Longitude deve estar entre -180 e 180 graus.");
        }
    }
}
