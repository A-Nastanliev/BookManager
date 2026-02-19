using DataLayer.Enums;
using System.Diagnostics;

namespace BusinessLayer.Repositories
{
	public class UserBookRepository : AbstractRepository<UserBook, (int userId, int bookId)>
	{
		public UserBookRepository(BookManagerContext context) : base(context) { }

		public async override Task<UserBook> ReadAsync((int userId, int bookId) key)
		{
			var (userId, bookId) = key;
			return await _context.UsersBook
				.Include(ub => ub.Book)
				.Include(ub => ub.ReadingLogs)
				.FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == bookId);
		}

		public async override Task UpdateAsync(UserBook obj)
		{
			var userBook = await _context.UsersBook.FindAsync(obj.UserId, obj.BookId);
			if (userBook == null)
				return;

			userBook.Status = obj.Status;
			await _context.SaveChangesAsync();
		}

		public override async Task<bool> CreateAsync(UserBook obj)
		{
			obj.Status = UserBookStatus.Whishlisted;
			obj.CreatedAt = DateTime.UtcNow;
			await _context.UsersBook.AddAsync(obj);
			return await _context.SaveChangesAsync() > 0;
		}

        public async Task<(List<UserBook> Items, DateTime? NextCursorDate, (int userId, int bookId)? NextCursorKey)> ReadNextByStatusAsync
            (int count, DateTime? cursorDate, (int userId, int bookId)? cursorKey, UserBookStatus status, int userId)
        {
            var query = _context.UsersBook
                .Where(ub => ub.UserId == userId && ub.Status == status);

            if (cursorDate.HasValue && cursorKey.HasValue)
            {
                query = query.Where(ub =>
                    ub.CreatedAt < cursorDate.Value ||
                    (ub.CreatedAt == cursorDate.Value && ub.BookId < cursorKey.Value.bookId));
            }

            var items = await query
                .OrderByDescending(ub => ub.CreatedAt)
                .ThenByDescending(ub => ub.BookId)
                .Take(count)
                .Include(ub => ub.Book)
                    .ThenInclude(b => b.Author)
                .Include(ub => ub.Book)
                    .ThenInclude(b => b.Publisher)
                .Include(ub => ub.Book)
                    .ThenInclude(b => b.Genre)
                .ToListAsync();

            var last = items.LastOrDefault();
            (int userId, int bookId)? nextCursorKey = last == null ? null : (last.UserId, last.BookId);

            return (items, last?.CreatedAt, nextCursorKey);
        }


        public override Task<(List<UserBook>, DateTime? cursorDate, (int userId, int bookId)? cursorKey)> ReadNextAsync(int count, DateTime? cursorDate, (int userId, int bookId)? cursorKey)
        {
            throw new NotImplementedException();
        }
    }
}
