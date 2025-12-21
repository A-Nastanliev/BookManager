
using Org.BouncyCastle.Asn1.IsisMtt.X509;

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

        public override async Task<bool> CreateAsync(UserRestriction obj)
        {
            obj.StartDate = DateTime.UtcNow;
            await _context.UserRestrictions.AddAsync(obj);
            return await _context.SaveChangesAsync() > 0;
        }

        public override async Task<bool> UpdateAsync(UserRestriction obj)
        {
            var restriction = await _context.UserRestrictions.FindAsync(obj.Id);
            if (restriction == null)
                return false;

            restriction.EndDate = DateTime.UtcNow;
            return await _context.SaveChangesAsync() > 0;
        }

        public override async Task<List<UserRestriction>> ReadNextAsync(int count, int loaded)
        {
            return await _context.UserRestrictions
              .Where(cr => cr.EndDate == null || cr.EndDate > DateTime.UtcNow)
              .OrderByDescending(f => f.StartDate)
              .Skip(loaded)
              .Take(count)
              .Include(cr => cr.User)
              .ToListAsync();
        }
	}
}
