namespace BusinessLayer.Repositories
{
    public class BookRatingRepository : AbstractRepository<BookRating, (int userId, int bookId)>
    {
        public BookRatingRepository(BookManagerContext context) : base(context) { }
    }
}
