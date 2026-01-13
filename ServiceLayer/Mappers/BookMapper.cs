using DataLayer.Models;
using ServiceLayer.Dto.Book;

namespace ServiceLayer.Mappers
{
    public static class BookMapper
    {
        public static BookDto ToDto(this Book book, string baseUrl)
        {
            return new BookDto(book.Id, book.ISBN, book.Title, string.IsNullOrWhiteSpace(book.Cover) ? null: $"{baseUrl}/{book.Cover}",
                book.TotalPages, book.Description,
                book.Author.ToDto(), book.Genre.ToDto(), book.Publisher.ToDto()); 
        }

        public static AuthorDto ToDto(this Author author) 
        {
            return new AuthorDto(author.Id, author.Name, author.Biography, author.BirthDate);
        }

        public static GenreDto ToDto(this Genre genre) 
        {
            return new GenreDto(genre.Id, genre.Name, genre.Description);
        }

        public static PublisherDto ToDto(this Publisher publisher) 
        {
            return new PublisherDto(publisher.Id, publisher.Name, publisher.Description, publisher.Website);
        }
    }
}
