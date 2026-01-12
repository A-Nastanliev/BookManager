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
		public override async Task<bool> UpdateAsync(ReadingLog obj)
		{
			var log = await _context.ReadingLogs.Include(l => l.UserBook).FirstOrDefaultAsync(l => l.Id == obj.Id);
			if (log == null || log.UserBook.UserId != obj.UserId || log.UserBook.BookId != obj.BookId)
				return false;

			log.StartingPage = obj.StartingPage;
			log.EndingPage = obj.EndingPage;
			return await _context.SaveChangesAsync() > 0;
		}

		public async Task<List<ReadingLog>> ReadNextByUserBookAsync(int count, int loaded, (int userId, int bookId) key)
		{
			return await _context.ReadingLogs
			   .Where(r => r.UserId == key.userId && r.BookId == key.bookId)
			   .OrderByDescending(r => r.Date)
			   .Skip(loaded)
			   .Take(count)
			   .ToListAsync();
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
