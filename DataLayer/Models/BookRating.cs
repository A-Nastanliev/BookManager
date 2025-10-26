namespace DataLayer.Models
{
    public class BookRating
    {
        [Key]
        public int Id { get; set; }

        public int BookId { get; set; }
        [ForeignKey(nameof(BookId))]
        public Book Book { get; set; }

        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [Range(1, 10)]
        public byte Rating { get; set; }
    }
}
