using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ServiceLayer.Dto.Book;
using ServiceLayer.Dto.User;

namespace ServiceLayer.Dto.Reading
{
	public class BookRatingDto
	{
		public int BookId { get; set; }
		public BookDto Book { get; set; }

		public int UserId { get; set; }
		public UserDto User { get; set; }

		[Range(1, 10)]
		public byte Rating { get; set; }

		public BookRatingDto() { }

		public BookRatingDto(int bookId, int userId, byte rating)
		{
			BookId = bookId;
			UserId = userId;
			Rating = rating;
		}

		public BookRatingDto(int bookId, BookDto book, int userId, UserDto user, byte rating) : this(bookId, userId, rating)
		{
			Book = book;
			User = user;
		}
	}
}