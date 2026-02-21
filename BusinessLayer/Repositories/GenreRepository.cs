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

        public override async Task UpdateAsync(Genre obj)
        {
            var genre = await _context.Genres.FindAsync(obj.Id);
            if (genre == null)
                return;

            genre.Description = obj.Description;
            genre.Name = obj.Name;
            await _context.SaveChangesAsync();
        }

        public override async Task<(List<Genre>, DateTime? cursorDate, int? cursorKey)> ReadNextAsync(int count, DateTime? cursorDate, int? cursorKey)
        {
            var query = _context.Genres
                .OrderByDescending(g => g.Id)
                .AsQueryable();

            if (cursorKey.HasValue)
            {
                query = query.Where(g => g.Id < cursorKey.Value);
            }

            var items = await query
                .Take(count)
                .ToListAsync();

            var last = items.LastOrDefault();

            return (items, null, last?.Id);
        }

        public async Task<(List<Genre> Genres, int? CursorKey)>ReadNextAsync(int count, int? lastGenreId, string? search)
        {
            var query = _context.Genres.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(g => g.Name.Contains(search));
            }

            if (lastGenreId.HasValue)
            {
                query = query.Where(g => g.Id < lastGenreId.Value);
            }

            query = query.OrderByDescending(g => g.Id);

            var items = await query
                .Take(count)
                .ToListAsync();

            var last = items.LastOrDefault();

            return (items, last?.Id);
        }
    }
}
