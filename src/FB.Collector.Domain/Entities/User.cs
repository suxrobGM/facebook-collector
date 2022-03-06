using System;
using System.Collections.Generic;
using SuxrobGM.Sdk.Extensions;

namespace FBShared.Models
{
    public class User
    {
        public User()
        {
            ScrappedTime = DateTime.Now;
            Institutions = new List<UserInstitution>();
            Works = new List<Employee>();
        }

        public string Id { get; set; }
        public string Username { get; set; }        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Bio { get; set; }
        public string Gender { get; set; }
        public string LivedCities { get; set; }     
        public string Hometown { get; set; }
        public string MaritalStatus { get; set; }
        public string Skills { get; set; }
        public string Languages { get; set; }
        public string ReligiousView { get; set; }
        public string ContactNumbers { get; set; }
        public string Instagram { get; set; }
        public string Twitter { get; set; }
        public string YouTube { get; set; }
        public string LinkedIn { get; set; }
        public string GitHub { get; set; }
        public string VK { get; set; }
        public string OK { get; set; }
        public string WebSites { get; set; }
        public string Quote { get; set; }
        public bool IsMyFriend { get; set; }
        public string ProfilePhotoSrc { get; set; }
        public string HeaderPhotoSrc { get; set; }        
        public DateTime? Birthday { get; set; }
        public DateTime? MemberSince { get; set; }
        public DateTime? ScrappedTime { get; set; }
        
        public virtual List<UserInstitution> Institutions { get; set; }
        public virtual List<Employee> Works { get; set; }

        public void GenerateUsername()
        {
            var rand = new Random();
            Username = $"{FirstName}.{LastName}.{rand.Next(1000, 9999)}".TranslateToLatin().IgnoreChars().ToLower();
        }
        public void ParseFLName(string userFullName)
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
            return $"{Id} - {Username} {FirstName} {LastName} {Gender} {Bio}";
        }
    }

    public class UserComparer : IEqualityComparer<User>
    {
        public bool Equals(User user1, User user2)
        {
            return user1.Id == user2.Id;
        }

        public int GetHashCode(User user)
        {
            return user.Id.GetHashCode();
        }
    }
}
