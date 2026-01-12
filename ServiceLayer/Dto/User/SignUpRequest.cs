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
        public IFormFile ProfilePicture { get; set; }

        public SignUpRequest() { }
    
    }
}
