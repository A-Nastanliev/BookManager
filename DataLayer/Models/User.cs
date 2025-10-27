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

        public UserRole Role { get; set; }

        [Required]
        public List<UserBook> UserBooks { get; set; } = new();

        [Required]
        public List<BookRating> BookRatings { get; set; } = new ();

        [Required]
        public List<BookComment> BookComments { get; set; } = new ();

        [Required]
        public List<UserRestriction> UserRestrictions { get; set; } = new();

        private UserRestriction currentRestriction;

        [NotMapped]
        public UserRestriction CurrentRestriction
        {
            get
            {
                if(currentRestriction != null)  
                    return currentRestriction;

                if (UserRestrictions == null || UserRestrictions.Count == 0)
                    return null;

                var now = DateTime.UtcNow;

                currentRestriction = UserRestrictions
                    .FirstOrDefault(r => r.StartDate <= now && (r.EndDate == null || r.EndDate > now));

                return currentRestriction;

            }
            set
            {
                currentRestriction = value;
            }
        }

        [NotMapped]
        public bool HasActiveRestriction => CurrentRestriction != null;
    }
}
