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

        public async Task<(List<BookRequest> Items, DateTime? NextCursorDate, int? NextCursorId)> ReadNextByStatusAsync
			(int count, DateTime? cursorDate, int? cursorId, BookRequestStatus status)
        {
            var query = _context.BookRequests
                .Where(br => br.Status == status);

            if (cursorDate.HasValue && cursorId.HasValue)
            {
                query = query.Where(br =>
                    br.DateSent < cursorDate.Value ||
                    (br.DateSent == cursorDate.Value && br.Id < cursorId.Value));
            }

            var items = await query
                .OrderByDescending(br => br.DateSent)
                .ThenByDescending(br => br.Id)
                .Take(count)
                .Include(br => br.Sender)
                .Include(br => br.ActionedBy)
                .ToListAsync();

            var last = items.LastOrDefault();

            return (items, last?.DateSent, last?.Id);
        }

        public async Task<(List<BookRequest> Items, DateTime? NextCursorDate, int? NextCursorId)> ReadNextByUserAsync
			(int count, DateTime? cursorDate, int? cursorId, int userId)
        {
            var query = _context.BookRequests
                .Where(br => br.SenderId == userId);

            if (cursorDate.HasValue && cursorId.HasValue)
            {
                query = query.Where(br =>
                    br.DateSent < cursorDate.Value ||
                    (br.DateSent == cursorDate.Value && br.Id < cursorId.Value));
            }

            var items = await query
				.Where(br=>br.Status == BookRequestStatus.Pending)
                .OrderByDescending(br => br.DateSent)
                .ThenByDescending(br => br.Id)
                .Take(count)
                .Include(br => br.ActionedBy)
                .ToListAsync();

            var last = items.LastOrDefault();

            return (items, last?.DateSent, last?.Id);
        }


        public override async Task<bool> CreateAsync(BookRequest obj)
		{
			var existingBooksOrRequests = await _context.Books.AnyAsync(b=>b.ISBN == obj.ISBN);
			if (existingBooksOrRequests) throw new InvalidOperationException($"Book with ISBN: {obj.ISBN} already exists");

		    existingBooksOrRequests = await _context.BookRequests.AnyAsync(b=>b.ISBN == obj.ISBN && b.Status == BookRequestStatus.Pending);
			if (existingBooksOrRequests) throw new InvalidOperationException($"Another user has requested book with ISBN: {obj.ISBN}");

            obj.DateSent = DateTime.UtcNow;
			obj.DateActioned = null;
			obj.ActionedById = null;
			obj.Status = BookRequestStatus.Pending;
			await _context.BookRequests.AddAsync(obj);
			return await _context.SaveChangesAsync() > 0;
		}

		public async Task<bool> UpdateByAdminAsync(BookRequest obj)
		{
			var bookRequest = await _context.BookRequests.FindAsync(obj.Id);
			if (bookRequest == null || bookRequest.Status != BookRequestStatus.Pending)
				return false;

            var existingBook = await _context.Books.AnyAsync(b => b.ISBN == obj.ISBN);
            if (existingBook) throw new InvalidOperationException($"Book with ISBN: {obj.ISBN} already exists");

            bookRequest.DateActioned = DateTime.UtcNow;
			bookRequest.ActionedById = obj.ActionedById;
			bookRequest.Status = obj.Status;
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
    }
}