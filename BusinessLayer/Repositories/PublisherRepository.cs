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

        public override Task<(List<Publisher>, DateTime? cursorDate, int? cursorKey)> ReadNextAsync(int count, DateTime? cursorDate, int? cursorKey)
        {
            throw new NotImplementedException();
        }
    }
}
