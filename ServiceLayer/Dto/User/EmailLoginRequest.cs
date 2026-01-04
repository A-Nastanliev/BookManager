using System.ComponentModel.DataAnnotations;

namespace ServiceLayer.Dto.User
{
    public class EmailLoginRequest
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

        public EmailLoginRequest() { }

        public EmailLoginRequest(string email, string password) 
        {
            Email = email;
            Password = password;
        }
    }
}
