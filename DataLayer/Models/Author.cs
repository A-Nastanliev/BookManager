namespace DataLayer.Models
{
    public class Author
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string Biography { get; set; }
        public DateTime? BirthDate { get; set; }

        [Required]
        public List<Book> Books { get; set; } = new();
    }
}