namespace DataLayer.Models
{
    [PrimaryKey(nameof(UserId), nameof(BookId))]
    public class BookRating
    {
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
