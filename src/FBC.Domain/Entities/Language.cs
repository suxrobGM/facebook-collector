namespace FBC.Domain.Entities;

public class Language : Entity
{
    public string? Name { get; set; }
    public virtual List<User> Speakers { get; set; } = new();
}
