namespace FBC.Domain.Entities;

public class UserInstitution
{
    public string StudentId { get; set; }
    public virtual User Student { get; set; }

    public string InstitutionId { get; set; }
    public virtual Institution Institution { get; set; }
}

public class UserInstitutionComparer : IEqualityComparer<UserInstitution>
{
    public bool Equals(UserInstitution x, UserInstitution y)
    {
        return x.InstitutionId == y.InstitutionId && x.StudentId == y.StudentId;
    }

    public int GetHashCode(UserInstitution obj)
    {
        return obj.Institution.Id.GetHashCode();
    }
}