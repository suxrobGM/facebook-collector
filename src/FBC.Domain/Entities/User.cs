using FBC.Domain.ValueObjects;

namespace FBC.Domain.Entities;

public class User : Entity
{
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Bio { get; set; }
    public Gender Gender { get; set; }
    public string? ReligiousView { get; set; }
    public string? CurrentCityId { get; set; }
    public string? HometowId { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Quote { get; set; }
    public bool IsMyFriend { get; set; }
    public string? ProfilePhotoSrc { get; set; }
    public string? HeaderPhotoSrc { get; set; }
    public DateTime? Birthday { get; set; }
    public DateTime? MemberSince { get; set; }
    public DateTime? Timestamp { get; set; } = DateTime.Now;

    public virtual City? CurrentCity { get; set; }
    public virtual City? Hometown { get; set; }
    public virtual List<City> LivedCities { get; set; } = new();
    public virtual List<Skill> Skills { get; set; } = new();
    public virtual List<Contact> Contacts { get; set; } = new();
    public virtual List<Language> Languages { get; set; } = new();
    public virtual List<Education> Educations { get; set; } = new();
    public virtual List<Employee> WorkPlaces { get; set; } = new();

    public void GenerateUsername()
    {
        var rand = new Random();
        UserName = $"{FirstName}.{LastName}.{rand.Next(1000, 9999)}".TranslateToLatin().IgnoreChars().ToLower();
    }

    public void ParseFullName(string userFullName)
    {
        var nameTokens = userFullName.Split(' ');

        if (nameTokens.Length > 1)
        {
            string lastName = "";
            for (int i = 1; i < nameTokens.Length; i++)
            {
                lastName += nameTokens[i];
            }

            FirstName = nameTokens[0];
            LastName = lastName;
        }
        else
        {
            FirstName = userFullName;
            LastName = "";
        }
    }

    public override string ToString()
    {
        return $"{Id} - {UserName} {FirstName} {LastName} {Gender} {Bio}";
    }
}

public class UserComparer : IEqualityComparer<User>
{
    public bool Equals(User? user1, User? user2)
    {
        return user1?.Id == user2?.Id;
    }

    public int GetHashCode(User user)
    {
        return user.Id.GetHashCode();
    }
}
