namespace BusinessLayer.Repositories
{
    public class AuthorRepository : AbstractRepository<Author, int>
    {
        public AuthorRepository(BookManagerContext context) : base(context) { }
        public override async Task<Author> ReadAsync(int id)
        {
            return await _context.Authors
                .Include(a=> a.Books)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public override async Task<List<Author>> ReadAllAsync()
        {
            return await _context.Authors
                .Include(a=>a.Books)    
                .ToListAsync();
        }

    }
}
