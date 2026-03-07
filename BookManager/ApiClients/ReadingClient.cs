using BookManager.Models.Book;
using BookManager.Models.Reading;
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

        public ReadingClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<RequestResult> WishlistBookAsync(int bookId)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/reading/user-books", bookId);

            if (!response.IsSuccessStatusCode)
                return new RequestResult(false, await ApiErrorParser.ParseAsync(response));

            return new RequestResult(true, null);
        }

        public async Task<(UserBookStatus Status, int PagesRead)> GetUserBookStatusAsync(int bookId)
        {
            var response = await _httpClient.GetAsync($"/api/reading/user-books/{bookId}/status");

            if (!response.IsSuccessStatusCode)
                throw new Exception(await ApiErrorParser.ParseAsync(response));

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var status = (UserBookStatus)root.GetProperty("status").GetByte();
            var pagesRead = root.GetProperty("pagesRead").GetInt32();

            return (status, pagesRead);
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
    }
}
