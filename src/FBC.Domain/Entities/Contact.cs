namespace FBC.Domain.Entities;

public class Contact : Entity
{
    public string? Type { get; set; }
    public string? Value { get; set; }
    public string? UserId { get; set; }
    public virtual User? User { get; set; }
}
