using System.ComponentModel.DataAnnotations;

namespace ServiceLayer.Dto.Book
{
    public class PublisherDto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(70)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(200)]
        public string Website { get; set; }

        public PublisherDto() { }

        public PublisherDto(int id, string name, string description, string website)
        {
            Id = id;
            Name = name;
            Description = description;
            Website = website;
        }
    }
}
