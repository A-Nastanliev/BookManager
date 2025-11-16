namespace BusinessLayer.Repositories
{
    public class GenreRepository : AbstractRepository<Genre, int>
    {
        public GenreRepository(BookManagerContext context) : base(context) { }
        public override async Task<Genre> ReadAsync(int id)
        {
            return await _context.Genres
                .Include(a=> a.Books)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public override async Task<List<Genre>> ReadAllAsync()
        {
            return await _context.Genres
                .ToListAsync();
        }

    }
}
