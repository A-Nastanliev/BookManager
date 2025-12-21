namespace DataLayer.Models
{
    [PrimaryKey(nameof(UserId), nameof(BookId))]
    public class UserBook
    {
        public UserBookStatus Status { get; set; }

        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        public int BookId { get; set; }
        [ForeignKey(nameof(BookId))]
        public Book Book { get; set; }

        [Required]
        public List<ReadingLog> ReadingLogs { get; set; } = new();
    }
}
