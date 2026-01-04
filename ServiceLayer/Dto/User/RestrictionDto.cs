using DataLayer.Models;
using Org.BouncyCastle.Ocsp;
using System.ComponentModel.DataAnnotations;

namespace ServiceLayer.Dto.User
{
    public class RestrictionDto
    {
        public int Id { get; set; }

        [Required]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [MaxLength(500)]
        public string Reason { get; set; }
        public UserDto User { get; set; }

        public RestrictionDto() { }

        public RestrictionDto( string reason, DateTime? endDate)
        {
            Reason = reason;
            EndDate = endDate;
        }

        public RestrictionDto(int id, string reason, DateTime startDate, DateTime? endDate, UserDto user)
        {
            Id = id;
            StartDate = startDate;
            EndDate = endDate;
            Reason = reason;
            User = user;
        }
    }
}
