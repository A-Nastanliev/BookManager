using BookManager.Models.Book;
using BookManager.Models.Reading;
using BookManager.Models.User;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace BookManager.ApiClients
{
    public class ReadingClient
    {
        readonly HttpClient _httpClient;
        readonly UserVM _userVM;
        public ReadingClient(HttpClient httpClient, UserVM user)
        {
            _httpClient = httpClient;
            _userVM = user;
        }

        public async Task<RequestResult> WishlistBookAsync(int bookId)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/reading/user-books", bookId);

            if (!response.IsSuccessStatusCode)
                return new RequestResult(false, await ApiErrorParser.ParseAsync(response));

            return new RequestResult(true, null);
        }

        public async Task<(UserBookStatus Status, int PagesRead, byte? myRating, int ratingsCount, double avgRating)> GetUserBookDetailsAsync(int bookId)
        {
            var response = await _httpClient.GetAsync($"/api/reading/user-books/{bookId}/details");

            if (!response.IsSuccessStatusCode)
                throw new Exception(await ApiErrorParser.ParseAsync(response));

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var status = (UserBookStatus)root.GetProperty("status").GetByte();
            var pagesRead = root.GetProperty("pagesRead").GetInt32();

            byte? myRating = null;
            if (root.TryGetProperty("myRating", out var myRatingJson) && myRatingJson.ValueKind != JsonValueKind.Null)
                myRating = myRatingJson.GetByte();

            int ratingCount = 0;
            double ratingAverage = 0;
            if (root.TryGetProperty("ratingSummary", out var summaryJson) && summaryJson.ValueKind != JsonValueKind.Null)
            {
                ratingCount = summaryJson.GetProperty("count").GetInt32();
                ratingAverage = summaryJson.GetProperty("average").GetDouble();
            }

            return (status, pagesRead, myRating, ratingCount, ratingAverage);
        }


        public async Task<(List<BookVM> Books, DateTime? NextCursorDate, int? NextCursorKey)>
            GetNextUserBooksByStatusAsync(UserBookStatus status, int count, DateTime? cursorDate = null, int? cursorBookId = null)
        {
            var query = $"?count={count}&userBookStatus={status}";

            if (cursorDate.HasValue)
                query += $"&cursorDate={Uri.EscapeDataString(cursorDate.Value.ToString("o"))}";

            if (cursorBookId.HasValue)
                query += $"&cursorKey={cursorBookId.Value}";

            var response = await _httpClient.GetAsync($"/api/reading/user-books{query}");

            if (!response.IsSuccessStatusCode)
                throw new Exception(await ApiErrorParser.ParseAsync(response));

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var books = new List<BookVM>();
            if (root.TryGetProperty("books", out var booksJson))
            {
                foreach (var bookJson in booksJson.EnumerateArray())
                {
                    var book = new BookVM();
                    book.FromJson(bookJson);
                    books.Add(book);
                }
            }

            DateTime? nextCursorDate = null;
            int? nextCursorKey = null;

            if (root.TryGetProperty("cursorDate", out var cd) && cd.ValueKind != JsonValueKind.Null)
                nextCursorDate = cd.GetDateTime();

            if (root.TryGetProperty("cursorKey", out var ck) && ck.ValueKind != JsonValueKind.Null)
                nextCursorKey = ck.GetInt32();

            return (books, nextCursorDate, nextCursorKey);
        }

        public async Task<RequestResult> DeleteUserBookAsync(int bookId)
        {
            var response = await _httpClient.DeleteAsync( $"/api/reading/user-books/{bookId}");

            if (!response.IsSuccessStatusCode)
                return new RequestResult(false, await ApiErrorParser.ParseAsync(response));

            return new RequestResult(true, null);
        }

        public async Task<RequestResult> CreateReadingLogAsync(int bookId, ReadingLogVM log)
        {
            var payload = new
            {
                startingPage = log.StartingPage,
                endingPage = log.EndingPage,
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"/api/reading/user-books/{bookId}/logs", payload);

            if (!response.IsSuccessStatusCode)
                return (new RequestResult(false, await ApiErrorParser.ParseAsync(response)));

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            log.Id = doc.RootElement.GetProperty("id").GetInt32();
            log.Date = DateTime.UtcNow;

            return new RequestResult(true, null);
        }

        public async Task<(List<ReadingLogVM> Logs, DateTime? CursorDate, int? CursorId)>
            GetNextReadingLogsAsync(int bookId, int count, DateTime? cursorDate = null, int? cursorId = null)
        {
            var query = $"?count={count}";

            if (cursorDate.HasValue)
                query += $"&cursorDate={Uri.EscapeDataString(cursorDate.Value.ToString("o"))}";

            if (cursorId.HasValue)
                query += $"&cursorKey={cursorId.Value}";

            var response = await _httpClient.GetAsync($"/api/reading/user-books/{bookId}/logs{query}");

            if (!response.IsSuccessStatusCode)
                throw new Exception(await ApiErrorParser.ParseAsync(response));

            var logs = new List<ReadingLogVM>();
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            foreach (var logJson in root.GetProperty("readingLogs").EnumerateArray())
            {
                var log = new ReadingLogVM();
                log.FromJson(logJson);
                logs.Add(log);
            }

            DateTime? nextCursorDate = null;
            int? nextCursorId = null;

            if (root.TryGetProperty("cursorDate", out var cd) && cd.ValueKind != JsonValueKind.Null)
                nextCursorDate = cd.GetDateTime();

            if (root.TryGetProperty("cursorId", out var ci) && ci.ValueKind != JsonValueKind.Null)
                nextCursorId = ci.GetInt32();

            return (logs, nextCursorDate, nextCursorId);
        }

        public async Task<RequestResult> DeleteReadingLogAsync(int bookId, int logId)
        {
            var response = await _httpClient.DeleteAsync($"/api/reading/user-books/{bookId}/logs/{logId}");

            if (!response.IsSuccessStatusCode)
                return new RequestResult(false, await ApiErrorParser.ParseAsync(response));

            return new RequestResult(true, null);
        }

        public async Task<RequestResult> CreateBookRatingAsync(int bookId, byte rating)
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/reading/books/{bookId}/rating", rating);

            if (!response.IsSuccessStatusCode)
                return new RequestResult(false, await ApiErrorParser.ParseAsync(response));

            return new RequestResult(true, null);
        }

        public async Task<RequestResult> UpdateBookRatingAsync(int bookId, byte rating)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/reading/books/{bookId}/rating", rating);

            if (!response.IsSuccessStatusCode)
                return new RequestResult(false, await ApiErrorParser.ParseAsync(response));

            return new RequestResult(true, null);
        }

        public async Task<RequestResult> DeleteBookRatingAsync(int bookId)
        {
            var response = await _httpClient.DeleteAsync($"/api/reading/books/{bookId}/rating");

            if (!response.IsSuccessStatusCode)
                return new RequestResult(false, await ApiErrorParser.ParseAsync(response));

            return new RequestResult(true, null);
        }

        public async Task<RequestResult> CreateBookCommentAsync(int bookId, CommentVM comment)
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/reading/books/{bookId}/comments", comment.Comment);

            if (!response.IsSuccessStatusCode)
                return new RequestResult(false, await ApiErrorParser.ParseAsync(response));

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);

            comment.Id = doc.RootElement.GetProperty("id").GetInt32();
            comment.DateTime = DateTime.UtcNow;

            return new RequestResult(true, null);
        }

        public async Task<(List<CommentVM> Comments, DateTime? CursorDate, int? CursorId)>
            GetNextBookCommentsAsync(int bookId, int count, DateTime? cursorDate = null, int? cursorId = null)
        {
            var query = $"?count={count}";

            if (cursorDate.HasValue)
                query += $"&cursorDate={Uri.EscapeDataString(cursorDate.Value.ToString("o"))}";

            if (cursorId.HasValue)
                query += $"&cursorKey={cursorId.Value}";

            var response = await _httpClient.GetAsync($"/api/reading/books/{bookId}/comments{query}");

            if (!response.IsSuccessStatusCode)
                throw new Exception(await ApiErrorParser.ParseAsync(response));

            var comments = new List<CommentVM>();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            foreach (var commentJson in root.GetProperty("bookComments").EnumerateArray())
            {
                var comment = new CommentVM();
                comment.FromJson(commentJson);
                comments.Add(comment);
            }

            DateTime? nextCursorDate = null;
            int? nextCursorId = null;

            if (root.TryGetProperty("cursorDate", out var cd) && cd.ValueKind != JsonValueKind.Null)
                nextCursorDate = cd.GetDateTime();

            if (root.TryGetProperty("cursorId", out var ci) && ci.ValueKind != JsonValueKind.Null)
                nextCursorId = ci.GetInt32();

            return (comments, nextCursorDate, nextCursorId);
        }

        public async Task<RequestResult> DeleteBookCommentAsync(int commentId)
        {
            var response = await _httpClient.DeleteAsync($"/api/reading/comments/{commentId}");

            if (!response.IsSuccessStatusCode)
                return new RequestResult(false, await ApiErrorParser.ParseAsync(response));

            return new RequestResult(true, null);
        }

        public async Task<RequestResult> CreateBookRequestAsync(BookRequestVM request)
        {
            var dto = new
            {
                senderId = request.Sender.Id,
                isbn = request.Isbn,
                title = request.Title,
            };

            var response = await _httpClient.PostAsJsonAsync("/api/reading/book-requests", dto);

            if (!response.IsSuccessStatusCode)
                return new RequestResult(false, await ApiErrorParser.ParseAsync(response));

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            request.Id = doc.RootElement.GetProperty("id").GetInt32();
            return new RequestResult(true, null);
        }

        public async Task<(List<BookRequestVM> Requests, DateTime? CursorDate, int? CursorId)>
            GetMyNextBookRequestsAsync(int count, DateTime? cursorDate = null, int? cursorId = null)
        {
            var query = $"?count={count}";

            if (cursorDate.HasValue)
                query += $"&cursorDate={Uri.EscapeDataString(cursorDate.Value.ToString("o"))}";

            if (cursorId.HasValue)
                query += $"&cursorKey={cursorId.Value}";

            var response = await _httpClient.GetAsync($"/api/reading/book-requests/mine{query}");

            if (!response.IsSuccessStatusCode)
                throw new Exception(await ApiErrorParser.ParseAsync(response));

            var requests = new List<BookRequestVM>();

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            foreach (var reqJson in root.GetProperty("bookRequests").EnumerateArray())
            {
                var request = new BookRequestVM(_userVM.PublicUser);
                request.FromJson(reqJson);
                requests.Add(request);
            }

            DateTime? nextCursorDate = null;
            int? nextCursorId = null;

            if (root.TryGetProperty("cursorDate", out var cd) && cd.ValueKind != JsonValueKind.Null)
                nextCursorDate = cd.GetDateTime();

            if (root.TryGetProperty("cursorId", out var ci) && ci.ValueKind != JsonValueKind.Null)
                nextCursorId = ci.GetInt32();

            return (requests, nextCursorDate, nextCursorId);
        }

        public async Task<(List<BookRequestVM> Requests, DateTime? CursorDate, int? CursorId)>
            GetNextBookRequestsAsync(BookRequestStatus status, int count, DateTime? cursorDate = null, int? cursorId = null)
        {
            var query = $"?count={count}";

            if (cursorDate.HasValue)
                query += $"&cursorDate={Uri.EscapeDataString(cursorDate.Value.ToString("o"))}";

            if (cursorId.HasValue)
                query += $"&cursorKey={cursorId.Value}";

            var response = await _httpClient.GetAsync($"/api/reading/book-requests/{status}{query}");

            if (!response.IsSuccessStatusCode)
                throw new Exception(await ApiErrorParser.ParseAsync(response));

            var requests = new List<BookRequestVM>();

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            foreach (var reqJson in root.GetProperty("bookRequests").EnumerateArray())
            {
                var request = new BookRequestVM();
                request.FromJson(reqJson);
                requests.Add(request);
            }

            DateTime? nextCursorDate = null;
            int? nextCursorId = null;

            if (root.TryGetProperty("cursorDate", out var cd) && cd.ValueKind != JsonValueKind.Null)
                nextCursorDate = cd.GetDateTime();

            if (root.TryGetProperty("cursorId", out var ci) && ci.ValueKind != JsonValueKind.Null)
                nextCursorId = ci.GetInt32();

            return (requests, nextCursorDate, nextCursorId);
        }

        public async Task<RequestResult> UpdateBookRequestAsync(int id, BookRequestStatus status)
        {
            var dto = new
            {
                id = id,
                status = status
            };

            var response = await _httpClient.PutAsJsonAsync($"/api/reading/book-requests/{id}/action", dto);

            if (!response.IsSuccessStatusCode)
                return new RequestResult(false, await ApiErrorParser.ParseAsync(response));

            return new RequestResult(true, null);
        }


    }
}
