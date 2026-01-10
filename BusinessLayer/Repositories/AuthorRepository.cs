namespace BusinessLayer.Repositories
{
    public class AuthorRepository : AbstractRepository<Author, int>
    {
        public AuthorRepository(BookManagerContext context) : base(context) { }
        public override async Task<Author> ReadAsync(int id)
        {
            return await _context.Authors
                .Include(a => a.Books)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public override async Task<List<Author>> ReadAllAsync()
        {
            return await _context.Authors
                .Include(a => a.Books)
                .ToListAsync();
        }

        public override async Task<bool> UpdateAsync(Author obj)
        {
            var author = await _context.Authors.FindAsync(obj.Id);
            if (author == null)
                return false;

            author.Biography = obj.Biography;
            author.BirthDate = obj.BirthDate;
            author.Name = obj.Name;
            return await _context.SaveChangesAsync() > 0;
        }

    }
}
