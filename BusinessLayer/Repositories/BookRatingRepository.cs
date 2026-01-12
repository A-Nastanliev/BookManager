namespace BusinessLayer.Repositories
{
	public class BookRatingRepository : AbstractRepository<BookRating, (int userId, int bookId)>
	{
		public BookRatingRepository(BookManagerContext context) : base(context) { }

		public override async Task<BookRating> ReadAsync((int userId, int bookId) id)
		{
			return await _context.BookRatings.FindAsync(id);
		}

		public override async Task<bool> UpdateAsync(BookRating obj)
		{
			var bookRating = await _context.BookRatings.FindAsync((obj.UserId, obj.BookId));
			if (bookRating == null)
				return false;

			bookRating.Rating = obj.Rating;
			return await _context.SaveChangesAsync() > 0;
		}

		public override async Task<bool> DeleteAsync(BookRating entity)
		{
			var bookRating = await _context.BookRatings.FindAsync((entity.UserId, entity.BookId));
			if (bookRating == null)
				return false;

			_context.BookRatings.Remove(bookRating);
			return await _context.SaveChangesAsync() > 0;
		}
		public async Task<(int count, double avg)> ReadSummaryByBookAsync(int bookId)
		{
			var result = await _context.BookRatings
				.Where(r => r.BookId == bookId)
				.GroupBy(r => r.BookId)
				.Select(g => new
				{
					Count = g.Count(),
					Avg = g.Average(r => r.Rating)
				})
				.FirstOrDefaultAsync();

			return result == null
				? (0, 0)
				: (result.Count, result.Avg);
		}
	}
}
