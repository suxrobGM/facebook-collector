namespace FBC.Domain.Entities;

public class UserEducation : IAggregateRoot
{
    public string? StudentId { get; set; }
    public virtual User? Student { get; set; }

    public string? InstitutionId { get; set; }
    public virtual Education? Institution { get; set; }
}

public class UserInstitutionComparer : IEqualityComparer<UserEducation>
{
    public bool Equals(UserEducation x, UserEducation y)
    {
        return x.InstitutionId == y.InstitutionId && x.StudentId == y.StudentId;
    }

    public int GetHashCode(UserEducation obj)
    {
        return obj.Institution.Id.GetHashCode();
    }
}