using BusinessLayer.Repositories;
using DataLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.Dto;
using ServiceLayer.Dto.Book;
using ServiceLayer.Mappers;
using ServiceLayer.Services;

namespace ServiceLayer.Controllers
{
	[Route("api/books")]
	public class BookController : BaseController
	{
		private readonly BookRepository _bookRepository;
		private readonly GenreRepository _genreRepository;
		private readonly PublisherRepository _publisherRepository;
		private readonly AuthorRepository _authorRepository;

		private readonly IConfiguration _configuration;
		private readonly IImageStorageService _imageStorageService;

		public BookController(BookRepository bookRepository, GenreRepository genreRepository, PublisherRepository publisherRepository, 
			AuthorRepository authorRepository, IConfiguration configuration, IImageStorageService imageStorageService)
		{
			_bookRepository = bookRepository;
			_genreRepository = genreRepository;
			_publisherRepository = publisherRepository;
			_authorRepository = authorRepository;
			_configuration = configuration;
			_imageStorageService = imageStorageService;
		}

		[Authorize(Roles = "Admin")]
		[HttpPost]
		public async Task<IActionResult> CreateBook([FromForm] BookFormDto req)
		{
            string coverPath = null;

            try
            {
                coverPath = await _imageStorageService.SaveImageAsync(req.Cover, "book-covers");
            }
            catch (InvalidOperationException e)
            {
                return BadRequest(e.Message);
            }

            var book = new Book(req.ISBN, req.Title, req.TotalPages, req.Description, req.AuthorName, req.PublisherName, req.GenreName);
			book.Cover = coverPath;

            var success = await _bookRepository.CreateAsync(book);
            if (!success)
                return BadRequest();

            return StatusCode(201, new
            {
                id = book.Id,
                authorId = book.AuthorId,
                publisherId = book.PublisherId,
                genreId = book.GenreId
            });
        }

		[HttpGet("next")]
		public async Task<IActionResult> GetNextBooks([FromQuery] CursorDto cursor, [FromQuery] string search)
		{
			var (Books, CursorDate, CursorId) = await _bookRepository.ReadNextAsync(cursor.Count, cursor.CursorDate, cursor.CursorKey, search);
            var baseUrl = _configuration["App:BaseUrl"];
            return Ok(new { Books = Books.Select(b => b.ToDto(baseUrl)), CursorDate, CursorId });
        }

        [Authorize(Roles = "Admin")]
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateBook(int id, [FromForm] BookFormDto req)
		{

            var existingBook = await _bookRepository.ReadAsync(id);
            if (existingBook == null)
                return NotFound();

            var book = new Book(req.ISBN, req.Title, req.TotalPages, req.Description, req.AuthorName, req.PublisherName, req.GenreName);
			book.Id = id;

            string? oldImagePath = existingBook.Cover;
            string? newImagePath = null;
            bool imageUpdated = false;

            if (req.Cover != null && req.Cover.Length > 0)
            {
                try
                {
                    newImagePath = await _imageStorageService.SaveImageAsync(req.Cover, "book-covers");

                    book.Cover = newImagePath;
                    imageUpdated = true;
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            try
            {
                await _bookRepository.UpdateAsync(book);
            }
            catch (DbUpdateException ex)
            {
                if (imageUpdated && !string.IsNullOrWhiteSpace(newImagePath))
                    _imageStorageService.DeleteImage(newImagePath);

                return Conflict(new { error = ex.Message });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                if (imageUpdated && !string.IsNullOrWhiteSpace(newImagePath))
                    _imageStorageService.DeleteImage(newImagePath);

                return BadRequest(new { error = ex.Message });
            }

            if (imageUpdated && !string.IsNullOrWhiteSpace(oldImagePath))
            {
                _imageStorageService.DeleteImage(oldImagePath);
                var baseUrl = _configuration["App:BaseUrl"];
                return Ok(new { image = $"{baseUrl}/{newImagePath}" });
            }

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteBook(int id)
		{
            var book = await _bookRepository.ReadAsync(id);
            if (book == null)
                return NotFound();

            _imageStorageService.DeleteImage(book.Cover);

            var success = await _bookRepository.DeleteAsync(new Book { Id = id });
            if (!success)
                return NotFound();

            return NoContent();
        }

		[Authorize(Roles = "Admin")]
		[HttpPost("authors")]
		public async Task<IActionResult> CreateAuthor([FromBody] AuthorDto req)
		{
			Author author = new Author(req.Name, req.Biography, req.BirthDate);
			var success = await _authorRepository.CreateAsync(author);
			if (!success) return BadRequest();

			return StatusCode(201, new { id = author.Id });
		}

		[HttpGet("authors")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReadNextAuthors([FromQuery] CursorDto cursor, [FromQuery] string search)
		{
            var (authors, nextCursorKey) = await _authorRepository.ReadNextAsync(cursor.Count, cursor.CursorKey, search);

            return Ok(new
            {
                authors = authors.Select(a => a.ToDto()),
                cursorKey = nextCursorKey,
            });
        }

		[Authorize(Roles = "Admin")]
		[HttpPut("authors/{id}")]
		public async Task<IActionResult> UpdateAuthor(int id, [FromBody] AuthorDto req)
		{
			Author author = new Author(req.Name, req.Biography, req.BirthDate);
			author.Id = id;
			var success = await _authorRepository.UpdateAsync(author);
			if (!success) return NotFound();

			return NoContent();
		}

		[Authorize(Roles = "Admin")]
		[HttpDelete("authors/{id}")]
		public async Task<IActionResult> DeleteAuthor(int id)
		{
			var success = await _authorRepository.DeleteAsync(new Author { Id = id });
			if (!success) return NotFound();

			return NoContent();
		}

		[Authorize(Roles = "Admin")]
		[HttpPost("genres")]
		public async Task<IActionResult> CreateGenre([FromBody] GenreDto req)
		{
			Genre genre = new Genre(req.Name, req.Description);
			var success = await _genreRepository.CreateAsync(genre);
			if (!success) return BadRequest();

			return StatusCode(201, new { id = genre.Id });
		}

		[HttpGet("genres")]
		public async Task<IActionResult> ReadNextGenres([FromQuery] CursorDto cursor, [FromQuery] string search)
		{
            var (genres, nextCursorKey) = await _genreRepository.ReadNextAsync(cursor.Count, cursor.CursorKey, search);

            return Ok(new
            {
                genres = genres.Select(g => g.ToDto()),
                cursorKey = nextCursorKey,
            });
        }

		[Authorize(Roles = "Admin")]
		[HttpPut("genres/{id}")]
		public async Task<IActionResult> UpdateGenre(int id, [FromBody] GenreDto req)
		{
			Genre genre = new Genre(req.Name, req.Description);
			genre.Id = id;
			await _genreRepository.UpdateAsync(genre);

			return NoContent();
		}

		[Authorize(Roles = "Admin")]
		[HttpDelete("genres/{id}")]
		public async Task<IActionResult> DeleteGenre(int id)
		{
			var success = await _genreRepository.DeleteAsync(new Genre { Id = id });
			if (!success) return NotFound();

			return NoContent();
		}

		[Authorize(Roles = "Admin")]
		[HttpPost("publishers")]
		public async Task<IActionResult> CreatePublisher([FromBody] PublisherDto req)
		{
			Publisher publisher = new Publisher(req.Name, req.Description, req.Website);
			var success = await _publisherRepository.CreateAsync(publisher);
			if (!success) return BadRequest();

			return StatusCode(201, new { id = publisher.Id });
		}

		[HttpGet("publishers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReadNextPublishers([FromQuery] CursorDto cursor, [FromQuery] string search)
		{
            var (publishers, nextCursorKey) = await _publisherRepository.ReadNextAsync(cursor.Count, cursor.CursorKey, search);

            return Ok(new
            {
                publishers = publishers.Select(p => p.ToDto()),
                cursorKey = nextCursorKey,
            });
        }

		[Authorize(Roles = "Admin")]
		[HttpPut("publishers/{id}")]
		public async Task<IActionResult> UpdatePublisher(int id, [FromBody] PublisherDto req)
		{
			Publisher publisher = new Publisher(req.Name, req.Description, req.Website);
			publisher.Id = id;
			await _publisherRepository.UpdateAsync(publisher);

			return NoContent();
		}

		[Authorize(Roles = "Admin")]
		[HttpDelete("publishers/{id}")]
		public async Task<IActionResult> DeletePublisher(int id)
		{
			var success = await _publisherRepository.DeleteAsync(new Publisher { Id = id });
			if (!success) return NotFound();

			return NoContent();
		}
	}
}