namespace DataLayer.Models
{
    public class ReadingLog
    {
        [Key]
        public int Id { get; set; }

        public int StartingPage { get; set; } 
        public int EndingPage { get; set; }
        public DateTime Date { get; set; }

        public int UserBookId { get; set; }
        [ForeignKey(nameof(UserBookId))]
        public UserBook UserBook { get; set; }
    }
}
