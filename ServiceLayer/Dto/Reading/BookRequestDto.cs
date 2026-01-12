using DataLayer.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ServiceLayer.Dto.User;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace ServiceLayer.Dto.Reading
{
	public class BookRequestDto
	{
		public int Id { get; set; }

		public int? SenderId { get; set; }
		public UserDto Sender { get; set; }

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
		public UserDto ActionedBy { get; set; }

		public BookRequestDto() { }

		public BookRequestDto(int? senderId, string isbn, string title, string requestDescription)
		{
			SenderId = senderId;
			Title = title;
			ISBN = isbn;
			RequestDescription = requestDescription;
		}
		public BookRequestDto(int? senderId, string title, string isbn, string requestDescription,
							  int id, int? actionedById, DateTime dateSent, DateTime? dateActioned,
							  BookRequestStatus status, UserDto sender, UserDto actionedBy)
			: this(senderId, isbn, title, requestDescription)
		{
			Id = id; SenderId = senderId; ActionedById = actionedById;
			DateSent = dateSent; DateActioned = dateActioned; Status = status;
			Sender = sender; ActionedBy = actionedBy;
		}
	}
}