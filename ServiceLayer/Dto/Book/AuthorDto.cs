using System.ComponentModel.DataAnnotations;

namespace ServiceLayer.Dto.Book
{
    public class AuthorDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Biography { get; set; }
        public DateTime? BirthDate { get; set; }

        public AuthorDto() { }

        public AuthorDto(int id, string name, string biography, DateTime? birthDate)
        {
            Id = id;
            Name = name;
            Biography = biography;
            BirthDate = birthDate;
        }
    }
}
