using FarmRegistry.Domain.Common;

namespace FarmRegistry.Domain.Entities;

public sealed class Field
{
    public Guid FieldId { get; }
    public Guid FarmId { get; private set; }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public double AreaHectares { get; private set; }
    public string CropName { get; private set; } = string.Empty;

    public FieldStatus Status { get; private set; }
    public DateTime StatusUpdatedAt { get; private set; }
    public DateTime CreatedAt { get; }

    private readonly List<FieldBoundaryPoint> _boundaryPoints = new();
    public IReadOnlyCollection<FieldBoundaryPoint> BoundaryPoints => _boundaryPoints.AsReadOnly();

    // Construtor vazio para Entity Framework
    private Field() 
    {
        Code = string.Empty;
        Name = string.Empty;
        CropName = string.Empty;
    }

    public Field(
        Guid farmId,
        string code,
        string name,
        double areaHectares,
        string cropName,
        IEnumerable<FieldBoundaryCoordinate> boundaryCoordinates,
        DateTime? createdAt = null)
    {
        if (farmId == Guid.Empty)
        {
            throw new DomainException("FarmId é obrigatório.");
        }

        ValidateCode(code);
        ValidateName(name);
        ValidateArea(areaHectares);
        ValidateCropName(cropName);

        FieldId = Guid.NewGuid();
        FarmId = farmId;

        Code = code.Trim();
        Name = name.Trim();
        AreaHectares = areaHectares;
        CropName = cropName.Trim();

        CreatedAt = createdAt ?? DateTime.UtcNow;

        ReplaceBoundaryPoints(boundaryCoordinates);

        Status = FieldStatus.Normal;
        StatusUpdatedAt = CreatedAt;
    }

    public void Update(
        string code,
        string name,
        double areaHectares,
        string cropName,
        IEnumerable<FieldBoundaryCoordinate> boundaryCoordinates)
    {
        ValidateCode(code);
        ValidateName(name);
        ValidateArea(areaHectares);
        ValidateCropName(cropName);

        Code = code.Trim();
        Name = name.Trim();
        AreaHectares = areaHectares;
        CropName = cropName.Trim();
        ReplaceBoundaryPoints(boundaryCoordinates);
    }

    public void Activate() => SetStatus(FieldStatus.Normal);

    public void Deactivate() => SetStatus(FieldStatus.Inativo);

    public void SetStatus(FieldStatus status)
    {
        Status = status;
        StatusUpdatedAt = DateTime.UtcNow;
    }

    internal void SetFarm(Guid farmId)
    {
        if (farmId == Guid.Empty)
        {
            throw new DomainException("FarmId é obrigatório.");
        }

        FarmId = farmId;
    }

    private void ReplaceBoundaryPoints(IEnumerable<FieldBoundaryCoordinate> boundaryCoordinates)
    {
        var coordinates = boundaryCoordinates?.ToList()
            ?? throw new DomainException("A delimitação do talhão é obrigatória.");

        if (coordinates.Count < 3)
        {
            throw new DomainException("A delimitação do talhão deve ter pelo menos 3 pontos.");
        }

        _boundaryPoints.Clear();

        for (var index = 0; index < coordinates.Count; index++)
        {
            var sequence = index + 1;
            var coordinate = coordinates[index];
            _boundaryPoints.Add(new FieldBoundaryPoint(FieldId, sequence, coordinate.Latitude, coordinate.Longitude));
        }
    }

    private static void ValidateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Código do talhão é obrigatório.");
        if (code.Trim().Length < 2)
            throw new DomainException("Código do talhão deve ter pelo menos 2 caracteres.");
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do talhão é obrigatório.");
        if (name.Trim().Length < 2)
            throw new DomainException("Nome do talhão deve ter pelo menos 2 caracteres.");
    }

    private static void ValidateArea(double areaHectares)
    {
        if (areaHectares <= 0)
            throw new DomainException("Área do talhão deve ser maior que zero.");
    }

    private static void ValidateCropName(string cropName)
    {
        if (string.IsNullOrWhiteSpace(cropName))
            throw new DomainException("Cultura plantada é obrigatória.");
        if (cropName.Trim().Length < 2)
            throw new DomainException("Cultura plantada deve ter pelo menos 2 caracteres.");
    }
}
