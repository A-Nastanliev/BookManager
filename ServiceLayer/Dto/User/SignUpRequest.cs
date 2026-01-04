using System.ComponentModel.DataAnnotations;

namespace ServiceLayer.Dto.User
{
    public class SignUpRequest
    {
        [Required]
        public string Username { get; set; }
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string ProfilePicture { get; set; }

        public SignUpRequest() { }

        public SignUpRequest(string username, string emailAddress, string password, string pfp)
        {
            Username = username;
            EmailAddress = emailAddress;
            Password = password;
            ProfilePicture = pfp;
        }
    }
}
