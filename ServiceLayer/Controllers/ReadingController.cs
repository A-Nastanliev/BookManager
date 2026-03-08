using BusinessLayer.Repositories;
using DataLayer.Enums;
using DataLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Dto;
using ServiceLayer.Dto.Reading;
using ServiceLayer.Mappers;
using System.Runtime.Intrinsics.X86;

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

		private readonly IConfiguration _configuration;

		public ReadingController(UserBookRepository userBookRepository, ReadingLogRepository readingLogRepository,
			BookCommentRepository bookCommentRepository, BookRatingRepository bookRatingRepository, BookRequestRepository bookRequestRepository, IConfiguration configuration)
		{
			_userBookRepository = userBookRepository;
			_readingLogRepository = readingLogRepository;
			_bookCommentRepository = bookCommentRepository;
			_bookRatingRepository = bookRatingRepository;
			_bookRequestRepository = bookRequestRepository;
			_configuration = configuration;
		}

		[HttpPost("user-books")]
		public async Task<IActionResult> WhishlistBook([FromBody] int bookId)
		{
			bool success = await _userBookRepository.CreateAsync(new UserBook { BookId = bookId, UserId = UserId });

			if (!success)
				return BadRequest("Failed to create user book");

			return Ok();
		}

        [HttpGet("user-books/{bookId}/details")]
        public async Task<IActionResult> GetUserBookDetails(int bookId)
        {
            var (status, pagesRead) = await _userBookRepository.GetStatusAndProgressAsync(UserId, bookId);
			var rating = await _bookRatingRepository.ReadAsync((UserId, bookId));
            var(count, avg) = await _bookRatingRepository.ReadSummaryByBookAsync(bookId);

			return Ok(new
			{
				Status = status,
				PagesRead = pagesRead,
				MyRating = rating?.Rating,
				RatingSummary = new BookRatingSummaryDto(count, avg)
			});
        }

        [HttpGet("user-books")]
		public async Task<IActionResult> GetNextUserBooks([FromQuery] CursorDto cursor, [FromQuery] UserBookStatus userBookStatus)
		{
            var (userBooks, nextCursorDate, nextCursorKey) = await _userBookRepository.ReadNextByStatusAsync
				(cursor.Count, cursor.CursorDate, cursor.CursorKey, userBookStatus, UserId);

            var baseUrl = _configuration["App:BaseUrl"];
			var books = userBooks.Select(ub=>ub.Book).ToList();
            return Ok(new
            {
                Books = books.Select(b => b.ToDto(baseUrl)),
                CursorDate = nextCursorDate,
                CursorKey = nextCursorKey
            });
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
		public async Task<IActionResult> ReadNextReadingLogs(int bookId, [FromQuery] CursorDto cursor)
		{	
            var (logs, nextCursorDate, nextCursorId) = await _readingLogRepository.ReadNextByUserBookAsync
				(cursor.Count, cursor.CursorDate, cursor.CursorKey, (UserId, bookId));

            return Ok(new
            {
                ReadingLogs = logs.Select(l => l.ToDto()),
                CursorDate = nextCursorDate,
                CursorId = nextCursorId
            });
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
		public async Task<IActionResult> ReadMyNextBookRequests([FromQuery] CursorDto cursor)
		{
            var (BookRequests, CursorDate, CursorId) = await _bookRequestRepository.ReadNextByUserAsync(cursor.Count, cursor.CursorDate, cursor.CursorKey, UserId);
            var baseUrl = _configuration["App:BaseUrl"];
            return Ok(new { BookRequests = BookRequests.Select(br => br.ToDto(baseUrl)), CursorDate, CursorId });
        }

		[HttpGet("book-requests")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> ReadNextBookRequests([FromQuery] CursorDto cursor)
		{
            var (BookRequests, CursorDate, CursorId) = await _bookRequestRepository.ReadNextAsync(cursor.Count, cursor.CursorDate, cursor.CursorKey);
            var baseUrl = _configuration["App:BaseUrl"];
            return Ok(new { BookRequests = BookRequests.Select(br => br.ToDto(baseUrl)), CursorDate, CursorId });
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
		public async Task<IActionResult> CreateBookRating(int bookId, [FromBody] byte rating)
		{
			var bookRating = new BookRating
			{
				UserId = UserId,
				BookId = bookId,
				Rating = rating
			};

			bool success = await _bookRatingRepository.CreateAsync(bookRating);

			if (!success)
				return BadRequest("Failed to create rating");

			return StatusCode(201);
		}

		[HttpPut("books/{bookId}/rating")]
		public async Task<IActionResult> UpdateBookRating(int bookId, [FromBody] byte rating)
		{
			var bookRating = new BookRating
			{
				UserId = UserId,
				BookId = bookId,
				Rating = rating
			};

			await _bookRatingRepository.UpdateAsync(bookRating);

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
		public async Task<IActionResult> ReadNextBookComments(int bookId, [FromQuery] CursorDto cursor)
		{		
			var (BookComments, CursorDate, CursorId) = await _bookCommentRepository.ReadNextByBookAsync(bookId, cursor.Count, cursor.CursorDate, cursor.CursorKey);
            var baseUrl = _configuration["App:BaseUrl"];
            return Ok(new { BookComments = BookComments.Select(c => c.ToDto(baseUrl)), CursorDate, CursorId });
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