namespace FBC.Domain.Entities;

public class LanguageEntity : Entity
{
    public string? Language { get; set; }
    public List<User> Users { get; set; } = new List<User>();
}
