using BusinessLayer.Repositories;
using Microsoft.AspNetCore.Mvc;

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
    }
}
