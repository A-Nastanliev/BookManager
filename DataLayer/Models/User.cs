namespace DataLayer.Models
{
    [Index(nameof(EmailAddress), IsUnique = true)]
    [Index(nameof(Username), IsUnique = true)]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Length(4,40)]
        public string Username { get; set; }

        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public List<UserBook> UserBooks { get; set; } = new();

        [Required]
        public List<BookRating> BookRatings { get; set; } = new ();

        [Required]
        public List<BookComment> BookComments { get; set; } = new ();
    }
}
