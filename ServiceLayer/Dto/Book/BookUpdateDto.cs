using System.ComponentModel.DataAnnotations;

namespace ServiceLayer.Dto.Book
{
    public class BookUpdateDto
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
    }
}
