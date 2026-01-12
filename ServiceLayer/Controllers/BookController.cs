using BusinessLayer.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Dto;
using ServiceLayer.Dto.Book;
using ServiceLayer.Mappers;
using DataLayer.Models;

namespace ServiceLayer.Controllers
{
	[Route("api/books")]
	public class BookController : BaseController
	{
		private readonly BookRepository _bookRepository;
		private readonly GenreRepository _genreRepository;
		private readonly PublisherRepository _publisherRepository;
		private readonly AuthorRepository _authorRepository;

		public BookController(BookRepository bookRepository, GenreRepository genreRepository, PublisherRepository publisherRepository, AuthorRepository authorRepository)
		{
			_bookRepository = bookRepository;
			_genreRepository = genreRepository;
			_publisherRepository = publisherRepository;
			_authorRepository = authorRepository;
		}

		[Authorize(Roles = "Admin")]
		[HttpPost]
		public async Task<IActionResult> CreateBook([FromBody] BookDto bookDto)
		{
			Book book = new Book(bookDto.ISBN, bookDto.Title, bookDto.Cover, bookDto.TotalPages, bookDto.Description,
				bookDto.AuthorDto.Id, bookDto?.GenreDto.Id, bookDto?.PublisherDto.Id);
			var success = await _bookRepository.CreateAsync(book);
			if (!success) return BadRequest();

			return StatusCode(201, new { id = book.Id });
		}

		[HttpGet("next")]
		public async Task<IActionResult> GetNextBooks([FromQuery] LoadNextDto load)
		{
			var books = await _bookRepository.ReadNextAsync(load.Count, load.AlreadyLoaded);
			return Ok(books.Select(b => b.ToDto()));
		}

		[HttpGet("next-by")]
		public async Task<IActionResult> GetNextBooksBy([FromQuery] LoadNextDto load, [FromQuery] string type, [FromQuery] int id)
		{
			var books = await _bookRepository.ReadNextByAsync(type.ToLower(), id, load.Count, load.AlreadyLoaded);

			return Ok(books.Select(b => b.ToDto()));
		}

		[Authorize(Roles = "Admin")]
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateBook(int id, [FromBody] BookUpdateDto req)
		{
			var success = await _bookRepository.UpdateAsync(new Book(req.Id, req.ISBN, req.Title, req.Cover, req.TotalPages, req.Description));
			if (!success) return NotFound();

			return NoContent();
		}

		[Authorize(Roles = "Admin")]
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteBook(int id)
		{
			var success = await _bookRepository.DeleteAsync(new Book { Id = id });
			if (!success) return NotFound();

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
		public async Task<IActionResult> GetAuthors()
		{
			var authors = await _authorRepository.ReadAllAsync();
			return Ok(authors.Select(a => a.ToDto()));
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
		public async Task<IActionResult> ReadAllGenres()
		{
			var genres = await _genreRepository.ReadAllAsync();
			return Ok(genres.Select(g => g.ToDto()));
		}

		[Authorize(Roles = "Admin")]
		[HttpPut("genres/{id}")]
		public async Task<IActionResult> UpdateGenre(int id, [FromBody] GenreDto req)
		{
			Genre genre = new Genre(req.Name, req.Description);
			genre.Id = id;
			var success = await _genreRepository.UpdateAsync(genre);
			if (!success) return NotFound();

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
		public async Task<IActionResult> ReadAllPublishers()
		{
			var publishers = await _publisherRepository.ReadAllAsync();
			return Ok(publishers.Select(p => p.ToDto()));
		}

		[Authorize(Roles = "Admin")]
		[HttpPut("publishers/{id}")]
		public async Task<IActionResult> UpdatePublisher(int id, [FromBody] PublisherDto req)
		{
			Publisher publisher = new Publisher(req.Name, req.Description, req.Website);
			publisher.Id = id;
			var success = await _publisherRepository.UpdateAsync(publisher);
			if (!success) return NotFound();

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