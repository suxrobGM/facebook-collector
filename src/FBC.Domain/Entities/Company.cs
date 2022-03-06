namespace FBC.Domain.Entities;

public class Company : Entity
{
    public string? Name { get; set; }
    public string? Link { get; set; }

    public virtual List<Employee> Employees { get; set; } = new();
}
