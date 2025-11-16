namespace BusinessLayer.Repositories
{
    public class BookCommentRepository : AbstractRepository<BookComment, int>
    {
        public BookCommentRepository(BookManagerContext context) : base(context) { }
        public override async Task<BookComment> ReadAsync(int id)
        {
            return await _context.BookComments
                .Include(bc => bc.Book)
                .Include(bc=>bc.User)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public override async Task<List<BookComment>> ReadAllAsync()
        {
            return await _context.BookComments
                .ToListAsync();
        }

    }
}
