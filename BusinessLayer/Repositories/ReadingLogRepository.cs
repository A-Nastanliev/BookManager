namespace BusinessLayer.Repositories
{
    public class ReadingLogRepository : AbstractRepository<ReadingLog, int>
    {
        public ReadingLogRepository(BookManagerContext context) : base(context) { }
        public override async Task<ReadingLog> ReadAsync(int id)
        {
            return await _context.ReadingLogs
                .Include(rl=> rl.UserBook)
                .FirstOrDefaultAsync(g => g.Id == id);
        }
		public override async Task UpdateAsync(ReadingLog obj)
		{
			var log = await _context.ReadingLogs.Include(l => l.UserBook).FirstOrDefaultAsync(l => l.Id == obj.Id);
			if (log == null || log.UserBook.UserId != obj.UserId || log.UserBook.BookId != obj.BookId)
				return;

			log.StartingPage = obj.StartingPage;
			log.EndingPage = obj.EndingPage;
		    await _context.SaveChangesAsync();
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
			return await _context.SaveChangesAsync() > 0;
		}
    }
}
