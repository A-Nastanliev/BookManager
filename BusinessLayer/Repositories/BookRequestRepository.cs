namespace BusinessLayer.Repositories
{
    public class BookRequestRepository : AbstractRepository<BookRequest, int>
    {
        public BookRequestRepository(BookManagerContext context) : base(context) { }
        public override async Task<BookRequest> ReadAsync(int id)
        {
            return await _context.BookRequests
                .Include(br=>br.Sender)
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

    }
}