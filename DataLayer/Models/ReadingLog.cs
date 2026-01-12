namespace DataLayer.Models
{

	public class ReadingLog
	{
		[Key]
		public int Id { get; set; }

		public int StartingPage { get; set; }
		public int EndingPage { get; set; }
		public DateTime Date { get; set; }

		public int UserId { get; set; }
		public int BookId { get; set; }

		public UserBook UserBook { get; set; }
	}
}
