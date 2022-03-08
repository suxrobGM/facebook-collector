namespace FBC.Domain.Entities;

public class Skill : Entity
{
    public string? Name { get; set; }
    public virtual List<User> Users { get; set; } = new();
}
