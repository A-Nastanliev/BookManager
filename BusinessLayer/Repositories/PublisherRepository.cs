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

        public override async Task<bool> UpdateAsync(Publisher obj)
        {
            var publisher = await _context.Publishers.FindAsync(obj.Id);
            if (publisher == null)
                return false;

            publisher.Description = obj.Description;
            publisher.Website = obj.Website;
            publisher.Name = obj.Name;
            return await _context.SaveChangesAsync() > 0;
        }

    }
}
