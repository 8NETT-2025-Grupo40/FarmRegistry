using FarmRegistry.Domain.Common;

namespace FarmRegistry.Domain.Entities;

public sealed class Farm
{
    public Guid Id { get; }

    public string Name { get; private set; }
    public string OwnerName { get; private set; }

    public bool IsActive { get; private set; }

    private readonly List<Field> _fields = new();
    public IReadOnlyCollection<Field> Fields => _fields.AsReadOnly();

    public Farm(string name, string ownerName)
    {
        ValidateName(name);
        ValidateOwner(ownerName);

        Id = Guid.NewGuid();
        Name = name.Trim();
        OwnerName = ownerName.Trim();
        IsActive = true;
    }

    public void Update(string name, string ownerName)
    {
        ValidateName(name);
        ValidateOwner(ownerName);

        Name = name.Trim();
        OwnerName = ownerName.Trim();
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public Field AddField(string name, string culture, double areaHectares)
    {
        // regra simples: evita nomes duplicados (case-insensitive) dentro da mesma Farm
        if (_fields.Any(f => string.Equals(f.Name, name?.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new DomainException("Já existe um talhão com esse nome nesta propriedade.");

        var field = new Field(Id, name, culture, areaHectares);
        _fields.Add(field);
        return field;
    }

    public void AddExistingField(Field field)
    {
        if (field is null) throw new DomainException("Field é obrigatório.");

        // garante o relacionamento lógico Farm(1)->(N) Field
        if (field.FarmId != Id)
            field.SetFarm(Id);

        if (_fields.Any(f => f.Id == field.Id))
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

    private static void ValidateOwner(string ownerName)
    {
        if (string.IsNullOrWhiteSpace(ownerName))
            throw new DomainException("Nome do produtor é obrigatório.");
    }
}
