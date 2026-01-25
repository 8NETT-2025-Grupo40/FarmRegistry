using FarmRegistry.Domain.Common;

namespace FarmRegistry.Domain.Entities;

public sealed class Field
{
    public Guid FieldId { get; }
    public Guid FarmId { get; private set; }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public double AreaHectares { get; private set; }

    public FieldStatus Status { get; private set; }
    public DateTime StatusUpdatedAt { get; private set; }
    public DateTime CreatedAt { get; }

    // Construtor vazio para Entity Framework
    private Field() 
    {
        Code = string.Empty;
        Name = string.Empty;
    }

    public Field(Guid farmId, string code, string name, double areaHectares, DateTime? createdAt = null)
    {
        if (farmId == Guid.Empty) throw new DomainException("FarmId é obrigatório.");
        ValidateCode(code);
        ValidateName(name);
        ValidateArea(areaHectares);

        FieldId = Guid.NewGuid();
        FarmId = farmId;

        Code = code.Trim();
        Name = name.Trim();
        AreaHectares = areaHectares;

        CreatedAt = createdAt ?? DateTime.UtcNow;

        Status = FieldStatus.Normal;
        StatusUpdatedAt = CreatedAt;
    }

    public void Update(string code, string name, double areaHectares)
    {
        ValidateCode(code);
        ValidateName(name);
        ValidateArea(areaHectares);

        Code = code.Trim();
        Name = name.Trim();
        AreaHectares = areaHectares;
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
        if (farmId == Guid.Empty) throw new DomainException("FarmId é obrigatório.");
        FarmId = farmId;
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
}
