namespace FBC.Domain.Entities;

public class Institution
{
    public Institution()
    {
        Students = new List<UserInstitution>();
        Id = GeneratorId.GenerateLong();
    }

    public string Id { get; set; }
    public string Name { get; set; }
    public string Link { get; set; }
    public bool IsHigherEducation { get; set; }

    public virtual List<UserInstitution> Students { get; set; }
}
