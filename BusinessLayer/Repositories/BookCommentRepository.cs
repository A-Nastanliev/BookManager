using DataLayer.Models;
namespace BusinessLayer.Repositories
{
	public class BookCommentRepository : AbstractRepository<BookComment, int>
	{
		public BookCommentRepository(BookManagerContext context) : base(context) { }
		public override async Task<BookComment> ReadAsync(int id)
		{
			return await _context.BookComments
				.Include(bc => bc.Book)
				.Include(bc => bc.User)
				.FirstOrDefaultAsync(g => g.Id == id);
		}

		public override async Task<List<BookComment>> ReadAllAsync()
		{
			return await _context.BookComments
				.ToListAsync();
		}

		public override async Task<bool> CreateAsync(BookComment obj)
		{
			obj.Date = DateTime.UtcNow;
			obj.UserPageProgress = await _context.ReadingLogs
				.Where(rl => rl.UserId == obj.UserId && rl.BookId == obj.BookId)
				.Select(rl => rl.EndingPage)
				.DefaultIfEmpty(0)
				.MaxAsync();

			if (obj.UserPageProgress! > 1)
				return false;

			await _context.BookComments.AddAsync(obj);
			return await _context.SaveChangesAsync() > 0;
		}

		public override async Task<bool> UpdateAsync(BookComment obj)
		{
			var comment = await _context.BookComments.FindAsync(obj.Id);
			if (comment == null || comment.UserId != obj.UserId)
				return false;

			comment.Comment = obj.Comment;
			return await _context.SaveChangesAsync() > 0;
		}

		public override async Task<bool> DeleteAsync(BookComment entity)
		{
			var comment = await _context.BookComments.FindAsync(entity.Id);
			if (comment == null || (entity.UserId != 0 && comment.UserId != entity.UserId))
				return false;

			_context.BookComments.Remove(comment);
			return await _context.SaveChangesAsync() > 0;
		}

		public async Task<List<BookComment>> ReadNextByBookAsync(int bookId, int count, int loaded)
		{
			return await _context.BookComments
				.Where(bc => bc.BookId == bookId)
				.OrderByDescending(bc => bc.Date)
				.Skip(loaded)
				.Take(count)
				.Include(bc => bc.User)
				.ToListAsync();
		}
	}
}
