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
                .OrderByDescending(br => br.DateSent)
                .ThenByDescending(br => br.Id)
                .Take(count)
                .Include(br => br.Sender)
                .Include(br => br.ActionedBy)
                .ToListAsync();

            var last = items.LastOrDefault();

            return (items, last?.DateSent, last?.Id);
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

        public override Task<(List<BookRequest>, DateTime? cursorDate, int? cursorKey)> ReadNextAsync(int count, DateTime? cursorDate, int? cursorKey)
        {
            throw new NotImplementedException();
        }
    }
}