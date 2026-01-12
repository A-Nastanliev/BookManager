using DataLayer.Enums;
using ServiceLayer.Dto.Book;

namespace ServiceLayer.Dto.Reading
{
	public class UserBookDto
	{
		public UserBookStatus Status { get; set; }
		public BookDto BookDto { get; set; }

		public List<ReadingLogDto> ReadingLogDtos { get; set; } = new();

		public UserBookDto() { }

		public UserBookDto(UserBookStatus status, BookDto book, List<ReadingLogDto> readingLogDtos)
		{
			Status = status;
			BookDto = book;
			ReadingLogDtos = readingLogDtos;
		}
	}
}
