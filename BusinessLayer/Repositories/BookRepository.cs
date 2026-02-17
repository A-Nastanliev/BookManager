using DataLayer.Models;

namespace BusinessLayer.Repositories
{
	public class BookRepository : AbstractRepository<Book, int>
	{
		public BookRepository(BookManagerContext context) : base(context) { }

        public override async Task<bool> CreateAsync(Book obj)
        {
            var author = await _context.Authors
                .FirstOrDefaultAsync(a => a.Name == obj.Author.Name);

            if (author == null)
            {
                author = new Author { Name = obj.Author.Name };
                _context.Authors.Add(author);
                await _context.SaveChangesAsync();
            }

            Genre? genre = null;
            if (obj.Genre != null)
            {
                genre = await _context.Genres
                    .FirstOrDefaultAsync(g => g.Name == obj.Genre.Name);

                if (genre == null)
                {
                    genre = new Genre { Name = obj.Genre.Name };
                    _context.Genres.Add(genre);
                    await _context.SaveChangesAsync();
                }
            }

            Publisher? publisher = null;
            if (obj.Publisher != null)
            {
                publisher = await _context.Publishers
                    .FirstOrDefaultAsync(p => p.Name == obj.Publisher.Name);

                if (publisher == null)
                {
                    publisher = new Publisher { Name = obj.Publisher.Name };
                    _context.Publishers.Add(publisher);
                    await _context.SaveChangesAsync();
                }
            }

            var book = new Book
            {
                ISBN = obj.ISBN,
                Title = obj.Title,
                Cover = obj.Cover,
                TotalPages = obj.TotalPages,
                Description = obj.Description,
                CreatedAt = DateTime.UtcNow,
                AuthorId = author.Id,
                GenreId = genre?.Id,
                PublisherId = publisher?.Id
            };

            _context.Books.Add(book);

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

        public async override Task<bool> UpdateAsync(Book obj)
        {
            var bookToUpdate = await _context.Books.FindAsync(obj.Id);
            if (bookToUpdate == null)
                return false;

			bookToUpdate.Title = obj.Title;
			bookToUpdate.Description = obj.Description;
			bookToUpdate.ISBN = obj.ISBN;
            if(obj.Cover  != null)
            {
                bookToUpdate.Cover = obj.Cover;
            }
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

        public async Task<(List<Book> Books, DateTime? CursorDate, int? CursorKey)>
            ReadNextAsync(int count, DateTime? lastCreatedAt, int? lastBookId, string? search)
        {
            var query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Include(b => b.Genre)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var words = search
                    .Trim()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (var word in words)
                {
                    var w = word;

                    query = query.Where(b =>
                        b.Title.Contains(w) ||
                        b.Author.Name.Contains(w) ||
                        (b.Publisher != null && b.Publisher.Name.Contains(w)) ||
                        (b.Genre != null && b.Genre.Name.Contains(w))
                    );
                }
            }

            if (lastCreatedAt.HasValue && lastBookId.HasValue)
            {
                query = query.Where(b =>
                    b.CreatedAt < lastCreatedAt.Value ||
                    (b.CreatedAt == lastCreatedAt.Value && b.Id < lastBookId.Value));
            }

            var items = await query
                .OrderByDescending(b => b.CreatedAt)
                .ThenByDescending(b => b.Id)
                .Take(count)
                .ToListAsync();

            var last = items.LastOrDefault();

            return (items, last?.CreatedAt, last?.Id);
        }

        public override Task<(List<Book>, DateTime? cursorDate, int? cursorKey)> ReadNextAsync(int count, DateTime? cursorDate, int? cursorKey)
        {
            throw new NotImplementedException();
        }
    }
}
