namespace DataLayer.Models
{
    public class Author
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(70)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Biography { get; set; }
        public DateTime? BirthDate { get; set; }

        [Required]
        public List<Book> Books { get; set; } = new();

        public Author() { }

        public Author(string name)
        {
            Name = name;
        }

        public Author(string name, string biography, DateTime? birthDate)
        {
            Name = name;
            Biography = biography;
            BirthDate = birthDate;
        }
    }
}