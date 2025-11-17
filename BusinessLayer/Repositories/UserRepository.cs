namespace BusinessLayer.Repositories
{
	public class UserRepository: AbstractRepository<User, int>
	{
		public UserRepository(BookManagerContext context) : base(context) { }
		public override async Task<User> ReadAsync(int id)
		{
			return await _context.Users
				.Include(u => u.UserBooks)
					.ThenInclude(ub => ub.Book)
				.Include(u => u.BookRatings)
				.Include(u => u.BookComments)
				.Include(u => u.UserRestrictions)
				.FirstOrDefaultAsync(g => g.Id == id);
		}
	}
}
