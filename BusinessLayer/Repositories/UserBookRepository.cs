namespace BusinessLayer.Repositories
{
	public class UserBookRepository: AbstractRepository<UserBook, int>
	{
		public UserBookRepository(BookManagerContext context) : base(context) { }
		public override async Task<UserBook> ReadAsync(int id)
		{
			return await _context.UsersBook
				.Include(ub => ub.User)
				.Include(ub => ub.Book)
				.Include(ub => ub.ReadingLogs)
				.FirstOrDefaultAsync(g => g.Id == id);
		}
	}
}
