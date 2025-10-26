namespace DataLayer.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Publisher
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(200)]
        public string Website { get; set; } 

        public List<Book> Books { get; set; } = new();
    }
}
