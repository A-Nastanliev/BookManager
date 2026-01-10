namespace DataLayer.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Genre
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public List<Book> Books { get; set; } = new();

        public Genre() { }

        public Genre(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}