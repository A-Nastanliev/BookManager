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

    }
}
