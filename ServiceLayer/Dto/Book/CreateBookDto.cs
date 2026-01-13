using System.ComponentModel.DataAnnotations;

namespace ServiceLayer.Dto.Book
{
    public class CreateBookDto
    {
        [Required]
        [Length(13, 13)]
        public string ISBN { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Range(1, int.MaxValue)]
        public int TotalPages { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public int AuthorId { get; set; }

        public int? GenreId { get; set; }
        public int? PublisherId { get; set; }

        public IFormFile Cover { get; set; }
    }
}
