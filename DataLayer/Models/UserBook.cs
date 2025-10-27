namespace DataLayer.Models
{
    public class UserBook
    {
        [Key]
        public int Id { get; set; }

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
