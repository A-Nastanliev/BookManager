namespace BusinessLayer.Repositories
{
    public class GenreRepository : AbstractRepository<Genre, int>
    {
        public GenreRepository(BookManagerContext context) : base(context) { }
        public override async Task<Genre> ReadAsync(int id)
        {
            return await _context.Genres
                .Include(a => a.Books)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public override async Task<List<Genre>> ReadAllAsync()
        {
            return await _context.Genres
                .ToListAsync();
        }

        public override async Task<bool> UpdateAsync(Genre obj)
        {
            var genre = await _context.Genres.FindAsync(obj.Id);
            if (genre == null)
                return false;

            genre.Description = obj.Description;
            genre.Name = obj.Name;
            return await _context.SaveChangesAsync() > 0;
        }

    }
}
