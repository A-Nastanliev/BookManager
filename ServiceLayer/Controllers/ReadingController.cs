using BusinessLayer.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ServiceLayer.Controllers
{
    [Route("api/reading")]
    public class ReadingController : BaseController
    {
        private readonly UserBookRepository _userBookRepository;
        private readonly ReadingLogRepository _readingLogRepository;
        private readonly BookCommentRepository _bookCommentRepository;
        private readonly BookRatingRepository _bookRatingRepository;
        private readonly BookRequestRepository _bookRequestRepository;
        public ReadingController( UserBookRepository userBookRepository, ReadingLogRepository readingLogRepository,
            BookCommentRepository bookCommentRepository, BookRatingRepository bookRatingRepository, BookRequestRepository bookRequestRepository) 
        {
            _userBookRepository = userBookRepository;
            _readingLogRepository = readingLogRepository;
            _bookCommentRepository = bookCommentRepository;
            _bookRatingRepository = bookRatingRepository;
            _bookRequestRepository = bookRequestRepository;
        }
    }
}
