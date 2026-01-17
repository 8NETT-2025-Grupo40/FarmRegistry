using FarmRegistry.Domain.Common;

namespace FarmRegistry.Domain.Entities;

public sealed class Farm
{
    public Guid FarmId { get; }

    public string Name { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public DateTime CreatedAt { get; }

    public bool IsActive { get; private set; }

    private readonly List<Field> _fields = new();
    public IReadOnlyCollection<Field> Fields => _fields.AsReadOnly();

    public Farm(string name, string city, string state, DateTime? createdAt = null)
    {
        ValidateName(name);
        ValidateCity(city);
        ValidateState(state);

        FarmId = Guid.NewGuid();
        Name = name.Trim();
        City = city.Trim();
        State = state.Trim().ToUpperInvariant();
        CreatedAt = createdAt ?? DateTime.UtcNow;

        IsActive = true;
    }

    public void Update(string name, string city, string state)
    {
        ValidateName(name);
        ValidateCity(city);
        ValidateState(state);

        Name = name.Trim();
        City = city.Trim();
        State = state.Trim().ToUpperInvariant();
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public Field AddField(string code, string name, double areaHectares)
    {
        ValidateFieldCode(code);

        if (_fields.Any(f => string.Equals(f.Code, code.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new DomainException("Já existe um talhão com esse código nesta propriedade.");

        var field = new Field(FarmId, code, name, areaHectares);
        _fields.Add(field);
        return field;
    }

    public void AddExistingField(Field field)
    {
        if (field is null) throw new DomainException("Field é obrigatório.");

        if (field.FarmId != FarmId)
            field.SetFarm(FarmId);

        if (_fields.Any(f => f.FieldId == field.FieldId))
            throw new DomainException("Este talhão já foi adicionado na propriedade.");

        _fields.Add(field);
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome da propriedade é obrigatório.");
        if (name.Trim().Length < 2)
            throw new DomainException("Nome da propriedade deve ter pelo menos 2 caracteres.");
    }

    private static void ValidateCity(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException("Cidade é obrigatória.");
    }

    private static void ValidateState(string state)
    {
        if (string.IsNullOrWhiteSpace(state))
            throw new DomainException("Estado é obrigatório.");
        if (state.Trim().Length != 2)
            throw new DomainException("Estado deve conter 2 caracteres (UF).");
    }

    private static void ValidateFieldCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Código do talhão é obrigatório.");
        if (code.Trim().Length < 2)
            throw new DomainException("Código do talhão deve ter pelo menos 2 caracteres.");
    }
}
