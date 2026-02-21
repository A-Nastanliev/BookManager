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

        public override async Task<(List<Author>, DateTime? cursorDate, int? cursorKey)> ReadNextAsync(int count, DateTime? cursorDate, int? cursorKey)
        {
            var query = _context.Authors
              .OrderByDescending(a => a.Id)
              .AsQueryable();

            if (cursorKey.HasValue)
            {
                query = query.Where(a => a.Id < cursorKey.Value);
            }

            var items = await query
                .Take(count)
                .ToListAsync();

            var last = items.LastOrDefault();

            return (items, null, last?.Id);
        }

        public async Task<(List<Author> Authors, int? CursorKey)> ReadNextAsync(int count, int? cursorKey, string? search)
        {
            var query = _context.Authors.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a => a.Name.Contains(search));
            }

            if (cursorKey.HasValue)
            {
                query = query.Where(a => a.Id < cursorKey.Value);
            }

            query = query.OrderByDescending(a => a.Id);

            var items = await query
                .Take(count)
                .ToListAsync();

            var last = items.LastOrDefault();

            return (items, last?.Id);
        }
    }
}
