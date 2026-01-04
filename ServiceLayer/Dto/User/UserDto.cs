using DataLayer.Enums;
using DataLayer.Models;
using Org.BouncyCastle.Asn1.Mozilla;
using System.ComponentModel.DataAnnotations;

namespace ServiceLayer.Dto.User
{
    public class UserDto
    {
        [EmailAddress]
        public string EmailAddress { get; set; }
        [Required]
        public string Username { get; set; }
        public string ProfilePicture { get; set;  }
        public UserRole Role { get; set; }       
        public DateTime CreatedAt { get; set; }
        public RestrictionDto CurrentRestriction { get; set; }

        public UserDto() { }

        public UserDto(string email, string username, string pfp, UserRole role, DateTime createdAt, RestrictionDto restriction)
        {
            EmailAddress = email;
            Username = username;
            ProfilePicture = pfp;
            Role = role;
            CreatedAt = createdAt;
            CurrentRestriction = restriction;
        }

        public UserDto(string username, string pfp, RestrictionDto restriction)
        {
            Username = username;
            ProfilePicture = pfp; ;
            CurrentRestriction = restriction;
        }
    }
}
