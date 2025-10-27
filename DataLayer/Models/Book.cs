namespace DataLayer.Models
{
    [Index(nameof(ISBN), IsUnique = true)]
    public class Book
    {
        [Key]
        public int Id { get; set; }

        [Length(13,13)]
        [Required]
        public string ISBN { get; set; }
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        [Required]
        public string Cover { get; set; }

        [Range(1, int.MaxValue)]
        public int TotalPages { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public int AuthorId { get; set; }
        [ForeignKey(nameof(AuthorId))]
        public Author Author { get; set; }

        public int? GenreId { get; set; }
        [ForeignKey(nameof(GenreId))]
        public Genre Genre { get; set; }

        public int? PublisherId { get; set; }
        [ForeignKey(nameof(PublisherId))]
        public Publisher Publisher { get; set; }

        [Required]
        public List<UserBook> UserBooks { get; set; } = new();

        [Required]
        public List<BookRating> Ratings { get; set; } = new();

        [Required]
        public List<BookComment> Comments { get; set; } = new();
    }
}
