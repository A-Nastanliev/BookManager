using DataLayer.Models;
namespace BusinessLayer.Repositories
{
	public class BookCommentRepository : AbstractRepository<BookComment, int>
	{
		public BookCommentRepository(BookManagerContext context) : base(context) { }
		public override async Task<BookComment> ReadAsync(int id)
		{
			return await _context.BookComments
				.Include(bc => bc.Book)
				.Include(bc => bc.User)
				.FirstOrDefaultAsync(g => g.Id == id);
		}

		public override async Task<List<BookComment>> ReadAllAsync()
		{
			return await _context.BookComments
				.ToListAsync();
		}

		public override async Task<bool> CreateAsync(BookComment obj)
		{
            obj.Date = DateTime.UtcNow;

            obj.UserPageProgress = await _context.ReadingLogs
                .Where(rl => rl.UserId == obj.UserId && rl.BookId == obj.BookId)
                .Select(rl => rl.EndingPage - rl.StartingPage + 1)
                .DefaultIfEmpty(0)
                .SumAsync();

            if (obj.UserPageProgress < 1)
				return false;

			await _context.BookComments.AddAsync(obj);
			return await _context.SaveChangesAsync() > 0;
		}

		public override async Task<bool> UpdateAsync(BookComment obj)
		{
			var comment = await _context.BookComments.FindAsync(obj.Id);
			if (comment == null || comment.UserId != obj.UserId)
				return false;

			comment.Comment = obj.Comment;
			return await _context.SaveChangesAsync() > 0;
		}

		public override async Task<bool> DeleteAsync(BookComment entity)
		{
			var comment = await _context.BookComments.FindAsync(entity.Id);
			if (comment == null || comment.UserId != entity.UserId)
				return false;

			_context.BookComments.Remove(comment);
			return await _context.SaveChangesAsync() > 0;
		}

        public async Task<(List<BookComment> Items, DateTime? NextCursorDate, int? NextCursorId)> ReadNextByBookAsync
			(int bookId, int count, DateTime? cursorDate, int? cursorId)
        {
            IQueryable<BookComment> query = _context.BookComments
                .Where(bc => bc.BookId == bookId)
                .Include(bc => bc.User);

            if (cursorDate.HasValue && cursorId.HasValue)
            {
                query = query.Where(bc =>
                    bc.Date < cursorDate.Value ||
                    (bc.Date == cursorDate.Value && bc.Id < cursorId.Value));
            }

            var items = await query
                .OrderByDescending(bc => bc.Date)
                .ThenByDescending(bc => bc.Id)
                .Take(count)
                .ToListAsync();

            var last = items.LastOrDefault();
            int? nextCursorId = last?.Id;

            return (items, last?.Date, nextCursorId);
        }

    }
}
