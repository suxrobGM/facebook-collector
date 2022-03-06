namespace FBC.Domain.Entities;

public class Company
{
    public Company()
    {
        Employees = new List<Employee>();
        Id = GeneratorId.GenerateLong();
    }

    public string Id { get; set; }
    public string Name { get; set; }
    public string Link { get; set; }

    public virtual List<Employee> Employees { get; set; }
}
