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

		public string ISBN { get; set; }

		public string Title { get; set; }

		public BookRequestStatus Status { get; set; }
		public DateTime DateSent { get; set; }
		public DateTime? DateActioned { get; set; }

		public int? ActionedById { get; set; }
		public UserDto ActionedBy { get; set; }

		public BookRequestDto() { }

		public BookRequestDto(int? senderId, string title, string isbn)
		{
			SenderId = senderId;
			Title = title;
			ISBN = isbn;
		}
		public BookRequestDto(int? senderId, string title, string isbn,
							  int id, int? actionedById, DateTime dateSent, DateTime? dateActioned,
							  BookRequestStatus status, UserDto sender, UserDto actionedBy)
			: this(senderId, title, isbn)
		{
			Id = id; SenderId = senderId; 
			ActionedById = actionedById;
			DateSent = dateSent; 
			DateActioned = dateActioned; 
			Status = status;
			Sender = sender; 
			ActionedBy = actionedBy;
		}
	}
}