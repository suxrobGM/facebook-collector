namespace FBC.Domain.Entities;

public class Employee : Entity
{
    public string? Position { get; set; }
    public bool IsCurrentlyWorking { get; set; }
    public DateTime? StartWorkDate { get; set; }
    public DateTime? EndWorkDate { get; set; }

    public string? CompanyId { get; set; }
    public virtual Company? Company { get; set; }

    public string? UserId { get; set; }
    public virtual User? User { get; set; }
}
