namespace BusinessLayer.Repositories
{
	public class UserBookRepository: AbstractRepository<UserBook, (int userId, int bookId)>
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

    }
}
