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
			obj.Status = UserBookStatus.Wishlisted;
			obj.CreatedAt = DateTime.UtcNow;
			await _context.UsersBook.AddAsync(obj);
			return await _context.SaveChangesAsync() > 0;
		}

        public async Task<(UserBookStatus Status, int PagesRead)> GetStatusAndProgressAsync(int userId, int bookId)
        {
            var userBook = await _context.UsersBook
                .Include(ub => ub.ReadingLogs)
                .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == bookId);

            if (userBook == null)
                return (UserBookStatus.None, 0);

            int totalPagesRead = userBook.ReadingLogs.Sum(rl => rl.EndingPage - rl.StartingPage + 1);
            return (userBook.Status, totalPagesRead);
        }

        public async Task<(List<UserBook> Items, DateTime? NextCursorDate, int? NextCursorKey)> ReadNextByStatusAsync
            (int count, DateTime? cursorDate, int? cursorKey, UserBookStatus status, int userId)
        {
            var query = _context.UsersBook
                .Where(ub => ub.UserId == userId && ub.Status == status);

            if (cursorDate.HasValue && cursorKey.HasValue)
            {
                query = query.Where(ub =>
                    ub.CreatedAt < cursorDate.Value ||
                    (ub.CreatedAt == cursorDate.Value && ub.BookId < cursorKey.Value));
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
            int? nextCursorKey = last?.BookId;

            return (items, last?.CreatedAt, nextCursorKey);
        }

    }
}
