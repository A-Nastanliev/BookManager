using DataLayer.Models;
using ServiceLayer.Dto.Book;
using ServiceLayer.Dto.Reading;

namespace ServiceLayer.Mappers
{
	public static class ReadingMapper
	{
		public static UserBookDto ToDto(this UserBook userbook)
		{
			return new UserBookDto(userbook.Status, userbook.Book.ToDto(), userbook.ReadingLogs?.Select(rl => rl.ToDto()).ToList());
		}

		public static ReadingLogDto ToDto(this ReadingLog log)
		{
			return new ReadingLogDto(log.Id, log.StartingPage, log.EndingPage, log.Date);
		}

		public static BookRequestDto ToDto(this BookRequest bookRequest)
		{
			return new BookRequestDto(bookRequest.SenderId, bookRequest.ISBN, bookRequest.Title, bookRequest.RequestDescription, bookRequest.Id,
				bookRequest.ActionedById, bookRequest.DateSent, bookRequest.DateActioned, bookRequest.Status, bookRequest.Sender?.ToDto(), bookRequest.ActionedBy?.ToDto());
		}

		public static BookRatingDto ToDto(this BookRating bookRating)
		{
			return new BookRatingDto(bookRating.BookId, bookRating?.Book.ToDto(), bookRating.UserId, bookRating?.User.ToDto(), bookRating.Rating);
		}

		public static BookCommentDto ToDto(this BookComment bookComment)
		{
			return new BookCommentDto(bookComment.Id, bookComment.BookId, null, bookComment.UserId, bookComment.User.ToPublicDto(),
				bookComment.Comment, bookComment.UserPageProgress, bookComment.Date);
		}
	}
}