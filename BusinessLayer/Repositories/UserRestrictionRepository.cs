
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
            var pending = await GetPendingRestrictionAsync(obj.UserId);
            if (pending != null)
            {
                string endDateText = pending.EndDate.HasValue
                    ? pending.EndDate.Value.ToString("HH:mm d MMMM yyyy")
                    : "undefined";

                throw new InvalidOperationException($"User already has a restriction. End date: {endDateText}");
            }

            obj.StartDate = DateTime.UtcNow;
            await _context.UserRestrictions.AddAsync(obj);
            return await _context.SaveChangesAsync() > 0;
        }

        public override async Task UpdateAsync(UserRestriction obj)
        {
            var restriction = await _context.UserRestrictions.FindAsync(obj.Id);
            if (restriction == null)
                return;

            restriction.EndDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<(List<UserRestriction> restrictions, DateTime? cursorDate, int? cursorKey)>
            ReadNextAsync(int count, RestrictionFilter filter, DateTime? cursorDate, int? cursorKey)
        {
            var now = DateTime.UtcNow;

            IQueryable<UserRestriction> query = _context.UserRestrictions
                .Include(cr => cr.User);

            query = filter switch
            {
                RestrictionFilter.Finished =>
                    query.Where(cr =>
                        cr.EndDate != null &&
                        cr.EndDate < now),

                RestrictionFilter.ActiveWithEndDate =>
                    query.Where(cr =>
                        cr.EndDate != null &&
                        cr.EndDate > now),

                RestrictionFilter.ActiveWithoutEndDate =>
                    query.Where(cr =>
                        cr.EndDate == null),

                _ => query
            };


            if (cursorDate.HasValue && cursorKey.HasValue)
            {
                query = query.Where(cr =>
                    cr.StartDate < cursorDate.Value ||
                    (cr.StartDate == cursorDate.Value && cr.Id < cursorKey.Value));
            }


            var restrictions = await query
                .OrderByDescending(cr => cr.StartDate)
                .ThenByDescending(cr => cr.Id)
                .Take(count)
                .ToListAsync();

            var lastItem = restrictions.LastOrDefault();

            return (
                restrictions,
                lastItem?.StartDate,
                lastItem?.Id
            );
        }

        public async Task<UserRestriction> GetPendingRestrictionAsync(int userId)
        {
            return await _context.UserRestrictions.FirstOrDefaultAsync(ur => (ur.EndDate > DateTime.UtcNow || ur.EndDate == null) && ur.UserId == userId);
        }
    }
}
