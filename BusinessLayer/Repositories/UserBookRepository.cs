using DataLayer.Enums;
using System.Diagnostics;

namespace BusinessLayer.Repositories
{
	public class UserBookRepository : AbstractRepository<UserBook, (int userId, int bookId)>
	{
		public UserBookRepository(BookManagerContext context) : base(context) { }

		public async override Task<UserBook> ReadAsync((int userId, int bookId) key)
		{
			var (userId, bookId) = key;
			return await _context.UsersBook
				.Include(ub => ub.User)
				.Include(ub => ub.Book)
				.Include(ub => ub.ReadingLogs)
				.FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == bookId);
		}

		public async override Task<bool> UpdateAsync(UserBook obj)
		{
			var userBook = await _context.UsersBook.FindAsync(obj.UserId, obj.BookId);
			if (userBook == null)
				return false;

			userBook.Status = obj.Status;
			return await _context.SaveChangesAsync() > 0;
		}

		public override async Task<bool> CreateAsync(UserBook obj)
		{
			obj.Status = DataLayer.Enums.UserBookStatus.Whishlisted;
			obj.CreatedAt = DateTime.UtcNow;
			await _context.UsersBook.AddAsync(obj);
			return await _context.SaveChangesAsync() > 0;
		}

		public async Task<List<UserBook>> ReadNextByStatusAsync(int count, int loaded, UserBookStatus status, int userId)
		{
			return await _context.UsersBook
				.Where(ub => ub.UserId == userId && ub.Status == status)
				.OrderByDescending(ub => ub.CreatedAt)
				.Skip(loaded)
				.Take(count)
				.Include(ub => ub.Book)
					.ThenInclude(b => b.Author)
				.Include(ub => ub.Book)
					.ThenInclude(b => b.Publisher)
				.Include(ub => ub.Book)
					.ThenInclude(b => b.Genre)
				.ToListAsync();
		}

	}
}
