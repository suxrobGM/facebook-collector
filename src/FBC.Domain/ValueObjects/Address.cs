namespace FBC.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    public Address()
    {
    }

    public Address(string street, string city, string zip, string country)
    {
        Street = street;
        City = city;
        Zip = zip;
        Country = country;
    }

    public string? Street { get; private set; }
    public string? City { get; private set; }
    public string? Zip { get; private set; }
    public string? Country { get; private set; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return Zip;
        yield return Country;
    }
}
