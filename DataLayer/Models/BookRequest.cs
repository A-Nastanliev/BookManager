namespace DataLayer.Models
{
    public class BookRequest
    {
        [Key]
        public int Id { get; set; }

        public int? SenderId { get; set; }
        [ForeignKey(nameof(SenderId))]
        public User Sender { get; set; }

        [Required]
        [Length(13, 13)]
        public string ISBN { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        [Length(10, 500)]
        public string RequestDescription { get; set; }

        public BookRequestStatus Status { get; set; }
        public DateTime DateSent { get; set; }
        public DateTime? DateActioned { get; set; }

        public int? ActionedById { get; set; }
        [ForeignKey(nameof(ActionedById))]
        public User ActionedBy { get; set; }
    }
}
