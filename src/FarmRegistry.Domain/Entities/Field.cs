using FarmRegistry.Domain.Common;

namespace FarmRegistry.Domain.Entities;

public sealed class Field
{
    public Guid Id { get; }
    public Guid FarmId { get; private set; }

    public string Name { get; private set; }
    public string Culture { get; private set; } // cultura plantada
    public double AreaHectares { get; private set; }

    public bool IsActive { get; private set; }

    public Field(Guid farmId, string name, string culture, double areaHectares)
    {
        if (farmId == Guid.Empty) throw new DomainException("FarmId é obrigatório.");
        ValidateName(name);
        ValidateCulture(culture);
        ValidateArea(areaHectares);

        Id = Guid.NewGuid();
        FarmId = farmId;

        Name = name.Trim();
        Culture = culture.Trim();
        AreaHectares = areaHectares;

        IsActive = true;
    }

    public void Update(string name, string culture, double areaHectares)
    {
        ValidateName(name);
        ValidateCulture(culture);
        ValidateArea(areaHectares);

        Name = name.Trim();
        Culture = culture.Trim();
        AreaHectares = areaHectares;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    internal void SetFarm(Guid farmId)
    {
        if (farmId == Guid.Empty) throw new DomainException("FarmId é obrigatório.");
        FarmId = farmId;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do talhão é obrigatório.");
        if (name.Trim().Length < 2)
            throw new DomainException("Nome do talhão deve ter pelo menos 2 caracteres.");
    }

    private static void ValidateCulture(string culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
            throw new DomainException("Cultura do talhão é obrigatória.");
    }

    private static void ValidateArea(double areaHectares)
    {
        if (areaHectares <= 0)
            throw new DomainException("Área do talhão deve ser maior que zero.");
    }
}
