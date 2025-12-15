namespace BusinessLayer.Repositories
{
	public class UserRestrictionRepository: AbstractRepository<UserRestriction, int>
	{
		public UserRestrictionRepository(BookManagerContext context) : base(context) { }
		public override async Task<UserRestriction> ReadAsync(int id)
		{
			return await _context.UserRestrictions
				.Include(ur => ur.User)
				.FirstOrDefaultAsync(g => g.Id == id);
		}
	}
}
