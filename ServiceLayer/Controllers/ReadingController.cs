using BusinessLayer.Repositories;
using DataLayer.Enums;
using DataLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Dto;
using ServiceLayer.Dto.Reading;
using ServiceLayer.Mappers;

namespace ServiceLayer.Controllers
{
	[Route("api/reading")]
	public class ReadingController : BaseController
	{
		private readonly UserBookRepository _userBookRepository;
		private readonly ReadingLogRepository _readingLogRepository;
		private readonly BookCommentRepository _bookCommentRepository;
		private readonly BookRatingRepository _bookRatingRepository;
		private readonly BookRequestRepository _bookRequestRepository;
		public ReadingController(UserBookRepository userBookRepository, ReadingLogRepository readingLogRepository,
			BookCommentRepository bookCommentRepository, BookRatingRepository bookRatingRepository, BookRequestRepository bookRequestRepository)
		{
			_userBookRepository = userBookRepository;
			_readingLogRepository = readingLogRepository;
			_bookCommentRepository = bookCommentRepository;
			_bookRatingRepository = bookRatingRepository;
			_bookRequestRepository = bookRequestRepository;
		}

		[Authorize(Roles = "Admin")]
		[HttpPost("user-books")]
		public async Task<IActionResult> CreateUserBook([FromBody] int bookId)
		{
			bool success = await _userBookRepository.CreateAsync(new UserBook { BookId = bookId, UserId = UserId });

			if (!success)
				return BadRequest("Failed to create user book");

			return Ok(new { success = true });
		}

		[HttpGet("user-books")]
		public async Task<IActionResult> GetNextUserBooks([FromQuery] LoadNextDto loadNextDto, [FromQuery] UserBookStatus userBookStatus)
		{
			List<UserBook> books = await _userBookRepository.ReadNextByStatusAsync(loadNextDto.Count, loadNextDto.AlreadyLoaded, userBookStatus, UserId);

			return Ok(books.Select(b => b.ToDto()));
		}

		[HttpPut("user-books/{bookId}")]
		public async Task<IActionResult> UpdateUserBook(int bookId, [FromBody] UserBookStatus status)
		{
			bool success = await _userBookRepository.UpdateAsync(new UserBook { UserId = UserId, Status = status, BookId = bookId });

			if (!success)
				return NotFound();

			return NoContent();
		}

		[HttpDelete("user-books/{bookId}")]
		public async Task<IActionResult> DeleteUserBook(int bookId)
		{
			bool success = await _userBookRepository.DeleteAsync(new UserBook { UserId = UserId, BookId = bookId });

			if (!success)
				return NotFound();

			return NoContent();
		}

		[HttpPost("user-books/{bookId}/logs")]
		public async Task<IActionResult> CreateReadingLog(int bookId, [FromBody] ReadingLogDto dto)
		{
			var log = new ReadingLog
			{
				UserId = UserId,
				BookId = bookId,
				StartingPage = dto.StartingPage,
				EndingPage = dto.EndingPage,
				Date = dto.Date
			};

			bool success = await _readingLogRepository.CreateAsync(log);

			if (!success)
				return BadRequest("Failed to create reading log");

			return StatusCode(201, new { id = log.Id });
		}

		[HttpGet("user-books/{bookId}/logs")]
		public async Task<IActionResult> ReadNextReadingLogs(int bookId, [FromQuery] LoadNextDto loadNextDto)
		{
			var logs = await _readingLogRepository.ReadNextByUserBookAsync(
				loadNextDto.Count,
				loadNextDto.AlreadyLoaded,
				(UserId, bookId));

			return Ok(logs.Select(l => l.ToDto()));
		}

		[HttpPut("user-books/{bookId}/logs/{logId}")]
		public async Task<IActionResult> UpdateReadingLog(int bookId, int logId, [FromBody] ReadingLogDto dto)
		{
			var log = new ReadingLog
			{
				Id = logId,
				UserId = UserId,
				BookId = bookId,
				StartingPage = dto.StartingPage,
				EndingPage = dto.EndingPage,
			};

			bool success = await _readingLogRepository.UpdateAsync(log);

			if (!success)
				return NotFound();

			return NoContent();
		}

		[HttpDelete("user-books/{bookId}/logs/{logId}")]
		public async Task<IActionResult> DeleteReadingLog(int bookId, int logId)
		{
			bool success = await _readingLogRepository.DeleteAsync(new ReadingLog
			{
				Id = logId,
				BookId = bookId,
				UserId = UserId,
			});

			if (!success)
				return NotFound();

			return NoContent();
		}

		[HttpPost("book-requests")]
		public async Task<IActionResult> CreateBookRequest([FromBody] BookRequestDto dto)
		{
			var request = new BookRequest(dto.SenderId, dto.ISBN, dto.Title, dto.RequestDescription);

			bool success = await _bookRequestRepository.CreateAsync(request);

			if (!success)
				return BadRequest("Failed to create book request");

			return StatusCode(201, new { id = request.Id });
		}

		[HttpGet("book-requests/mine")]
		public async Task<IActionResult> ReadMyNextBookRequests([FromQuery] LoadNextDto dto)
		{
			List<BookRequest> bookRequests = await _bookRequestRepository.ReadNextByUserAsync(dto.Count, dto.AlreadyLoaded, UserId);
			if (bookRequests == null)
				return NotFound();

			return Ok(bookRequests.Select(br => br.ToDto()));
		}

		[HttpGet("book-requests")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> ReadNextBookRequests([FromQuery] LoadNextDto dto)
		{
			List<BookRequest> bookRequests = await _bookRequestRepository.ReadNextAsync(dto.Count, dto.AlreadyLoaded);
			if (bookRequests == null)
				return NotFound();

			return Ok(bookRequests.Select(br => br.ToDto()));
		}

		[HttpPut("book-requests/{id}/action")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdateBookRequest([FromQuery] BookRequestDto dto)
		{
			BookRequest bookRequest = new BookRequest(dto.Id, dto.Status);
			bool success = await _bookRequestRepository.UpdateByAdminAsync(bookRequest);
			if (!success)
				return NotFound();

			return NoContent();
		}

		[HttpPut("book-requests/{id}")]
		public async Task<IActionResult> UpdateMyBookRequest([FromQuery] BookRequestDto dto)
		{
			BookRequest bookRequest = new BookRequest(dto.Id, UserId, dto.ISBN, dto.Title, dto.RequestDescription);
			bool success = await _bookRequestRepository.UpdateByAdminAsync(bookRequest);
			if (!success)
				return NotFound();

			return NoContent();
		}

		[Authorize(Roles = "Admin")]
		[HttpDelete("book-requests/{id}")]
		public async Task<IActionResult> DeleteBookRequest(int id)
		{
			bool success = await _bookRequestRepository.DeleteAsync(new BookRequest { Id = id });

			if (!success)
				return NotFound();

			return NoContent();
		}

		[HttpDelete("book-requests/{id}/mine")]
		public async Task<IActionResult> DeleteMyBookRequest(int id)
		{
			bool success = await _bookRequestRepository.DeleteByUserAsync(id, UserId);

			if (!success)
				return NotFound();

			return NoContent();
		}

		[HttpPost("books/{bookId}/rating")]
		public async Task<IActionResult> CreateBookRating(int bookId, [FromBody] BookRatingDto dto)
		{
			var rating = new BookRating
			{
				UserId = UserId,
				BookId = bookId,
				Rating = dto.Rating
			};

			bool success = await _bookRatingRepository.CreateAsync(rating);

			if (!success)
				return BadRequest("Failed to create rating");

			return StatusCode(201);
		}

		[HttpGet("books/{bookId}/rating")]
		public async Task<IActionResult> GetMyBookRating(int bookId)
		{
			var rating = await _bookRatingRepository.ReadAsync((UserId, bookId));

			if (rating == null)
				return NotFound();

			return Ok(rating.ToDto());
		}

		[HttpGet("books/{bookId}/ratings/summary")]
		public async Task<IActionResult> GetBookRatingSummary(int bookId)
		{
			var (count, avg) = await _bookRatingRepository.ReadSummaryByBookAsync(bookId);

			return Ok(new BookRatingSummaryDto(count, avg));
		}

		[HttpPut("books/{bookId}/rating")]
		public async Task<IActionResult> UpdateBookRating(int bookId, [FromBody] BookRatingDto dto)
		{
			var rating = new BookRating
			{
				UserId = UserId,
				BookId = bookId,
				Rating = dto.Rating
			};

			bool success = await _bookRatingRepository.UpdateAsync(rating);

			if (!success)
				return NotFound();

			return NoContent();
		}

		[HttpDelete("books/{bookId}/rating")]
		public async Task<IActionResult> DeleteBookRating(int bookId)
		{
			bool success = await _bookRatingRepository.DeleteAsync(new BookRating
			{
				UserId = UserId,
				BookId = bookId
			});

			if (!success)
				return NotFound();

			return NoContent();
		}

		[HttpPost("books/{bookId}/comments")]
		public async Task<IActionResult> CreateBookComment(int bookId, [FromBody] string content)
		{
			var comment = new BookComment
			{
				BookId = bookId,
				UserId = UserId,
				Comment = content,
			};

			bool success = await _bookCommentRepository.CreateAsync(comment);

			if (!success)
				return BadRequest("Failed to create comment");

			return StatusCode(201, new { id = comment.Id });
		}

		[HttpGet("books/{bookId}/comments")]
		public async Task<IActionResult> ReadNextBookComments(int bookId, [FromQuery] LoadNextDto loadNextDto)
		{
			var comments = await _bookCommentRepository.ReadNextByBookAsync(bookId, loadNextDto.Count, loadNextDto.AlreadyLoaded);

			return Ok(comments.Select(c => c.ToDto()));
		}

		[HttpPut("comments/{commentId}")]
		public async Task<IActionResult> UpdateBookComment(int commentId, [FromBody] string content)
		{
			var comment = new BookComment
			{
				Id = commentId,
				UserId = UserId,
				Comment = content
			};

			bool success = await _bookCommentRepository.UpdateAsync(comment);

			if (!success)
				return NotFound();

			return NoContent();
		}

		[HttpDelete("comments/{commentId}")]
		public async Task<IActionResult> DeleteBookComment(int commentId)
		{
			bool success;

			if (UserRole == UserRole.Admin)
			{
				success = await _bookCommentRepository.DeleteAsync(new BookComment { Id = commentId });
			} else
			{
				success = await _bookCommentRepository.DeleteAsync(new BookComment { Id = commentId, UserId = UserId });
			}

			if (!success)
				return NotFound();

			return NoContent();
		}
	}
}