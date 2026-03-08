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
			if(string.IsNullOrWhiteSpace(obj.Comment) || obj.Comment?.Length <4 || obj.Comment?.Length > 500)
			{
				throw new InvalidOperationException("Comments have to be atleast 4 characters long and maximum 500");
			}

			obj.Comment = obj.Comment.Trim();
            obj.Date = DateTime.UtcNow;

            obj.UserPageProgress = await _context.ReadingLogs
                .Where(rl => rl.UserId == obj.UserId && rl.BookId == obj.BookId)
                .Select(rl => rl.EndingPage - rl.StartingPage + 1)
                .SumAsync();

			if (obj.UserPageProgress <= 0)
				throw new InvalidOperationException("Commenting on books you are not reading isn't allowed");

			await _context.BookComments.AddAsync(obj);
			return await _context.SaveChangesAsync() > 0;
		}

		public override async Task<bool> UpdateAsync(BookComment obj)
		{
            if (string.IsNullOrWhiteSpace(obj.Comment) || obj.Comment?.Length < 4 || obj.Comment?.Length > 500)
            {
                throw new InvalidOperationException("Comments have to be atleast 4 characters long and maximum 500");
            }

            var comment = await _context.BookComments.FindAsync(obj.Id);
			if (comment == null || comment.UserId != obj.UserId)
				return false;

			comment.Comment = obj.Comment.Trim();
			return await _context.SaveChangesAsync() > 0;
		}

		public override async Task<bool> DeleteAsync(BookComment entity)
		{
			var comment = await _context.BookComments.FindAsync(entity.Id);
			if (comment == null)
				return false;

			var commenter = await _context.Users.FindAsync(comment.UserId);
			var deleter = await _context.Users.FindAsync(entity.UserId);
			if (commenter.Role == DataLayer.Enums.UserRole.Admin && commenter.Id != deleter.Id)
                throw new InvalidOperationException("You cannot delete comments made by other admins.");

			if (commenter.Role == DataLayer.Enums.UserRole.User && commenter.Id != deleter.Id && deleter.Role == DataLayer.Enums.UserRole.User )
				throw new UnauthorizedAccessException("Normal users cannot delete other's comments");

            _context.BookComments.Remove(comment);
			return await _context.SaveChangesAsync() > 0;
		}

        public async Task<(List<BookComment> Items, DateTime? NextCursorDate, int? NextCursorId)> ReadNextByBookAsync
			(int bookId, int count, DateTime? cursorDate, int? cursorId)
        {
            IQueryable<BookComment> query = _context.BookComments
                .Where(bc => bc.BookId == bookId)
                .Include(bc => bc.User);

            if (cursorDate.HasValue && cursorId.HasValue)
            {
                query = query.Where(bc =>
                    bc.Date < cursorDate.Value ||
                    (bc.Date == cursorDate.Value && bc.Id < cursorId.Value));
            }

            var items = await query
                .OrderByDescending(bc => bc.Date)
                .ThenByDescending(bc => bc.Id)
                .Take(count)
                .ToListAsync();

            var last = items.LastOrDefault();
            int? nextCursorId = last?.Id;

            return (items, last?.Date, nextCursorId);
        }

    }
}
