using DataLayer.Models;

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

		public async Task<bool> UpdateCoverAsync(Book book)
		{
			var bookToUpdate = await _context.Books.FindAsync(book.Id);
			if(bookToUpdate == null)
				return false;

			bookToUpdate.Cover = book.Cover;
			return await _context.SaveChangesAsync() > 0;
		}


        public async override Task<bool> UpdateAsync(Book obj)
        {
            var bookToUpdate = await _context.Books.FindAsync(obj.Id);
            if (bookToUpdate == null)
                return false;

			bookToUpdate.Title = obj.Title;
			bookToUpdate.Description = obj.Description;
			bookToUpdate.ISBN = obj.ISBN;
			if(obj.GenreId != null)
			{
				bookToUpdate.GenreId = obj.GenreId;
			}
			if(obj.PublisherId != null)
			{
				bookToUpdate.PublisherId = obj.PublisherId;
			}
            return await _context.SaveChangesAsync() > 0;
        }

        public override async Task<(List<Book>, DateTime? cursorDate, int? cursorKey)> ReadNextAsync(int count, DateTime? cursorDate, int? cursorKey)
        {
            IQueryable<Book> query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Include(b => b.Genre);

            if (cursorDate.HasValue && cursorKey.HasValue)
            {
                query = query.Where(b =>
                    b.CreatedAt < cursorDate.Value ||
                    (b.CreatedAt == cursorDate.Value && b.Id < cursorKey.Value));
            }

            var items = await query
                .OrderByDescending(b => b.CreatedAt)
                .ThenByDescending(b => b.Id)
                .Take(count)
                .ToListAsync();

            var last = items.LastOrDefault();
            int? nextCursorId = last?.Id;

            return (items, last?.CreatedAt, nextCursorId);
        }

    }
}
