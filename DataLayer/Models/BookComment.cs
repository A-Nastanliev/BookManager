namespace DataLayer.Models
{
    public class BookComment
    {
        [Key]
        public int Id { get; set; }

        public int BookId { get; set; }
        [ForeignKey(nameof(BookId))]
        public Book Book { get; set; }

        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [Length(1, 500)]
        [Required]
        public string Comment { get; set; }

        [Range(1, int.MaxValue)]
        public int UserPageProgress { get; set; }
    }
}
