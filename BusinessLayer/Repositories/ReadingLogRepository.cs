using DataLayer.Enums;
using System.Net;

namespace BusinessLayer.Repositories
{
    public class ReadingLogRepository : AbstractRepository<ReadingLog, int>
    {
        public ReadingLogRepository(BookManagerContext context) : base(context) { }

        public override async Task<bool> CreateAsync(ReadingLog obj)
        {
            var hasOverlap = await _context.ReadingLogs.AnyAsync(rl =>
                rl.UserId == obj.UserId &&
                rl.BookId == obj.BookId &&
                rl.StartingPage <= obj.EndingPage &&
                rl.EndingPage >= obj.StartingPage);

            if (hasOverlap)
                return false;

            var userBook = await _context.UsersBook.FindAsync(obj.UserId, obj.BookId);
            if (userBook == null)
            {
                userBook = new UserBook
                {
                    UserId = obj.UserId,
                    BookId = obj.BookId,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.UsersBook.AddAsync(userBook);
            }
            obj.UserBook = userBook;
            obj.Date = DateTime.UtcNow;

            await _context.ReadingLogs.AddAsync(obj);

            var book = await _context.Books.FindAsync(obj.BookId);

            var totalPages = await _context.ReadingLogs
               .Where(r => r.UserId == obj.UserId && r.BookId == obj.BookId)
               .SumAsync(r => r.EndingPage - r.StartingPage + 1);

            if ((totalPages + obj.PagesRead) == book.TotalPages)
                userBook.Status = UserBookStatus.Finished;
            else
                userBook.Status = UserBookStatus.Reading;

            return await _context.SaveChangesAsync() > 0;
        }

        public override async Task<ReadingLog> ReadAsync(int id)
        {
            return await _context.ReadingLogs
                .Include(rl=> rl.UserBook)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<(List<ReadingLog> Items, DateTime? NextCursorDate, int? NextCursorId)> ReadNextByUserBookAsync
            (int count, DateTime? cursorDate, int? cursorId, (int userId, int bookId) key)
        {
            var query = _context.ReadingLogs
                .Where(r => r.UserId == key.userId && r.BookId == key.bookId);

            if (cursorDate.HasValue && cursorId.HasValue)
            {
                query = query.Where(r =>
                    r.Date < cursorDate.Value ||
                    (r.Date == cursorDate.Value && r.Id < cursorId.Value));
            }

            var items = await query
                .OrderByDescending(r => r.Date)
                .ThenByDescending(r => r.Id)
                .Take(count)
                .ToListAsync();

            var last = items.LastOrDefault();
            int? nextCursorId = last?.Id;

            return (items, last?.Date, nextCursorId);
        }


        public async override Task<bool> DeleteAsync(ReadingLog obj)
		{
			var log = await _context.ReadingLogs.Include(l => l.UserBook).FirstOrDefaultAsync(l => l.Id == obj.Id);
			if (log == null || log.UserBook.UserId != obj.UserId || log.UserBook.BookId != obj.BookId)
				return false;

			_context.Remove(log);

            var userBook = await _context.UsersBook
                .Include(ub => ub.Book)
                .FirstOrDefaultAsync(ub => ub.UserId == obj.UserId && ub.BookId == obj.BookId);

            var totalPages = await _context.ReadingLogs
               .Where(r => r.UserId == obj.UserId && r.BookId == obj.BookId)
               .SumAsync(r => r.EndingPage - r.StartingPage + 1);

            if ((totalPages - log.PagesRead) == 0)
                userBook.Status = UserBookStatus.Wishlisted;
            else
                userBook.Status = UserBookStatus.Reading;

            return await _context.SaveChangesAsync() > 0;
		}
    }
}
