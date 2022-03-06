namespace FBC.Domain.ValueObjects;

public sealed class OtherName : ValueObject
{
    public OtherName()
    {
    }

    public OtherName(NameType nameType, string name)
    {
        Type = nameType;
        Name = name;
    }

    public NameType? Type { get; private set; }
    public string? Name { get; private set; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Type;
        yield return Name;
    }
}
