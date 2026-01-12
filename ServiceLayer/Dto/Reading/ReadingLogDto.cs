using DataLayer.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceLayer.Dto.Reading
{
	public class ReadingLogDto
	{
		public int Id { get; set; }

		public int StartingPage { get; set; }
		public int EndingPage { get; set; }
		public DateTime Date { get; set; }

		public ReadingLogDto() { }

		public ReadingLogDto(int id, int startingPage, int endingPage, DateTime date)
		{
			Id = id;
			StartingPage = startingPage;
			EndingPage = endingPage;
			Date = date;
		}
	}
}