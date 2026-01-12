namespace BusinessLayer.Repositories
{
	public class BookRepository : AbstractRepository<Book, int>
	{
		public BookRepository(BookManagerContext context) : base(context) { }

		public override async Task<bool> CreateAsync(Book obj)
		{
			obj.CreatedAt = DateTime.UtcNow;
			await _context.Books.AddAsync(obj);
			return await _context.SaveChangesAsync() > 0;
		}
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

		public override async Task<List<Book>> ReadNextAsync(int count, int loaded)
		{
			return await _context.Books
			   .Include(b => b.Author)
			   .Include(b => b.Publisher)
			   .Include(b => b.Genre)
			   .OrderByDescending(b => b.CreatedAt)
				   .ThenBy(b => b.Title)
			   .Skip(loaded)
			   .Take(count)
			   .ToListAsync();
		}

		public async Task<List<Book>> ReadNextByAsync(string type, int id, int count, int loaded)
		{
			IQueryable<Book> query = _context.Books
				.Include(b => b.Author)
				.Include(b => b.Publisher)
				.Include(b => b.Genre);

			query = type switch
			{
				"author" => query.Where(b => b.AuthorId == id),
				"genre" => query.Where(b => b.GenreId == id),
				"publisher" => query.Where(b => b.PublisherId == id),
				_ => throw new ArgumentException("Invalid filter type")
			};

			return await query
				.OrderByDescending(b => b.CreatedAt)
					.ThenBy(b => b.Title)
				.Skip(loaded)
				.Take(count)
				.ToListAsync();
		}

	}
}
