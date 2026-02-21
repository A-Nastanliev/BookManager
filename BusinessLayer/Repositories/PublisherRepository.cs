namespace BusinessLayer.Repositories
{
    public class PublisherRepository : AbstractRepository<Publisher, int>
    {
        public PublisherRepository(BookManagerContext context) : base(context) { }
        public override async Task<Publisher> ReadAsync(int id)
        {
            return await _context.Publishers
                .Include(a => a.Books)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public override async Task<List<Publisher>> ReadAllAsync()
        {
            return await _context.Publishers
                .ToListAsync();
        }

        public override async Task UpdateAsync(Publisher obj)
        {
            var publisher = await _context.Publishers.FindAsync(obj.Id);
            if (publisher == null)
                return;

            publisher.Description = obj.Description;
            publisher.Website = obj.Website;
            publisher.Name = obj.Name;
            await _context.SaveChangesAsync();
        }

        public override async Task<(List<Publisher>, DateTime? cursorDate, int? cursorKey)> ReadNextAsync(int count, DateTime? cursorDate, int? cursorKey)
        {
            var query = _context.Publishers
              .OrderByDescending(p => p.Id)
              .AsQueryable();

            if (cursorKey.HasValue)
            {
                query = query.Where(p => p.Id < cursorKey.Value);
            }

            var items = await query
                .Take(count)
                .ToListAsync();

            var last = items.LastOrDefault();

            return (items,  null, last?.Id);
        }

        public async Task<(List<Publisher> Publishers, int? CursorKey)> ReadNextAsync(int count, int? lastPublisherId, string? search)
        {
            var query = _context.Publishers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.Contains(search));
            }

            if (lastPublisherId.HasValue)
            {
                query = query.Where(p => p.Id < lastPublisherId.Value);
            }

            query = query.OrderByDescending(p => p.Id);

            var items = await query
                .Take(count)
                .ToListAsync();

            var last = items.LastOrDefault();

            return (items, last?.Id);
        }
    }
}
