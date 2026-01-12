using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ServiceLayer.Dto.Book;
using ServiceLayer.Dto.User;

namespace ServiceLayer.Dto.Reading
{
	public class BookCommentDto
	{
		public int Id { get; set; }

		public int BookId { get; set; }
		public BookDto Book { get; set; }

		public int UserId { get; set; }
		public UserDto User { get; set; }

		public string Comment { get; set; }

		public int UserPageProgress { get; set; }

		public DateTime DateTime { get; set; }

		public BookCommentDto() { }

		public BookCommentDto(int id, int bookId, BookDto book, int userId, UserDto user, string comment, int userPageProgress, DateTime dateTime)
		{
			Id = id;
			BookId = bookId;
			Book = book;
			UserId = userId;
			User = user;
			Comment = comment;
			UserPageProgress = userPageProgress;
			DateTime = dateTime;
		}
	}
}