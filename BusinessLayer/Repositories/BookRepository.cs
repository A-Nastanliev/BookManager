namespace BusinessLayer.Repositories
{
	public class BookRepository: AbstractRepository<Book, int>
	{
		public BookRepository(BookManagerContext context) : base(context) { }
		public override async Task<Book> ReadAsync(int id)
		{
			return await _context.Books
				.Include(b => b.Author)
				.Include(b => b.Genre)
				.Include(b => b.Publisher)
				.Include(b => b.Comments)
					.ThenInclude(c => c.User)
				.Include(b => b.Ratings)
				.Include(b => b.UserBooks)
				.FirstOrDefaultAsync(g => g.Id == id);
		}
		public override async Task<List<Book>> ReadAllAsync()
		{
			return await _context.Books
				.Include(b => b.Author)
				.Include(b => b.Genre)
				.Include(b => b.Publisher)
				.Include(b => b.Comments)
					.ThenInclude(c => c.User)
				.Include(b => b.Ratings)
				.ToListAsync();
		}
	}
}
