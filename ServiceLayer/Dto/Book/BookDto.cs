using System.ComponentModel.DataAnnotations;

namespace ServiceLayer.Dto.Book
{
    public class BookDto
    {
        public int Id { get; set; }

        [Length(13, 13)]
        [Required]
        public string ISBN { get; set; }
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        [Required]
        public string Cover { get; set; }

        [Range(1, int.MaxValue)]
        public int TotalPages { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
        public AuthorDto AuthorDto { get; set; }
        public GenreDto GenreDto { get; set; }
        public PublisherDto PublisherDto { get; set; }

        public BookDto() { }

        public BookDto(int id, string isbn, string title, string cover, int totalPages, string description,
            AuthorDto authorDto, GenreDto genreDto, PublisherDto publisherDto)
        {
            Id = id;
            Title = title;
            Cover = cover;
            TotalPages = totalPages;
            Description = description;
            AuthorDto = authorDto;
            GenreDto = genreDto;
            PublisherDto = publisherDto;
        }
    }
}
