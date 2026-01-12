using DataLayer.Enums;

namespace BusinessLayer.Repositories
{
	public class BookRequestRepository : AbstractRepository<BookRequest, int>
	{
		public BookRequestRepository(BookManagerContext context) : base(context) { }
		public override async Task<BookRequest> ReadAsync(int id)
		{
			return await _context.BookRequests
				.Include(br => br.Sender)
				.Include(br => br.ActionedBy)
				.FirstOrDefaultAsync(g => g.Id == id);
		}

		public override async Task<List<BookRequest>> ReadAllAsync()
		{
			return await _context.BookRequests
				.Include(br => br.Sender)
				.Include(br => br.ActionedBy)
				.ToListAsync();
		}

		public async Task<List<BookRequest>> ReadNextByStatusAsync(int count, int loaded, BookRequestStatus status)
		{
			return await _context.BookRequests
				.Where(br => br.Status == status)
				.OrderByDescending(br => br.DateSent)
				.Include(br => br.Sender)
				.Include(br => br.ActionedBy)
				.ToListAsync();
		}

		public async Task<List<BookRequest>> ReadNextByUserAsync(int count, int loaded, int userId)
		{
			return await _context.BookRequests
			  .Where(br => br.SenderId == userId)
			  .OrderByDescending(br => br.DateSent)
			  .Include(br => br.Sender)
			  .Include(br => br.ActionedBy)
			  .ToListAsync();
		}

		public override async Task<bool> CreateAsync(BookRequest obj)
		{
			obj.DateSent = DateTime.UtcNow;
			obj.DateActioned = null;
			obj.ActionedById = null;
			await _context.BookRequests.AddAsync(obj);
			return await _context.SaveChangesAsync() > 0;
		}

		public async Task<bool> UpdateByAdminAsync(BookRequest obj)
		{
			var bookRequest = await _context.BookRequests.FindAsync(obj.Id);
			if (bookRequest == null || bookRequest.Status == BookRequestStatus.Pending)
				return false;

			bookRequest.DateActioned = DateTime.UtcNow;
			bookRequest.ActionedById = obj.ActionedById;
			return await _context.SaveChangesAsync() > 0;
		}

		public async Task<bool> UpdateByUserAsync(BookRequest obj)
		{
			var bookRequest = await _context.BookRequests.FindAsync(obj.Id);
			if (bookRequest == null || bookRequest.Status == BookRequestStatus.Pending || bookRequest.SenderId != obj.SenderId)
				return false;

			bookRequest.ISBN = obj.ISBN;
			bookRequest.RequestDescription = obj.RequestDescription;
			bookRequest.Title = obj.Title;
			return await _context.SaveChangesAsync() > 0;
		}


		public override async Task<bool> DeleteAsync(BookRequest entity)
		{
			var bookRequest = await _context.BookRequests.FindAsync(entity.Id);
			if (bookRequest == null)
				return false;

			_context.Remove(bookRequest);
			return await _context.SaveChangesAsync() > 0;
		}

		public async Task<bool> DeleteByUserAsync(int id, int userId)
		{
			var request = await _context.BookRequests
				.FirstOrDefaultAsync(br =>
					br.Id == id &&
					br.SenderId == userId &&
					br.Status == BookRequestStatus.Pending);

			if (request == null)
				return false;

			_context.BookRequests.Remove(request);
			return await _context.SaveChangesAsync() > 0;
		}
	}
}