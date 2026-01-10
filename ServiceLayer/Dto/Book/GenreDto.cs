using System.ComponentModel.DataAnnotations;

namespace ServiceLayer.Dto.Book
{
    public class GenreDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public GenreDto() { }

        public GenreDto(int id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }
}
