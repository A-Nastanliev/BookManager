using BookManager.Models.Book;
using BookManager.ViewModels.Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BookManager.ApiClients
{
    public class BookClient
    {
        readonly HttpClient _httpClient;

        readonly UserVM _user;

        public BookClient(HttpClient httpClient, UserVM currentUser)
        {
            _httpClient = httpClient;
            _user = currentUser;
        }

        public async Task<(bool Success, string? Error)> CreateBookAsync
            ( BookVM book, string coverPath)
        {
            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(book.ISBN), "ISBN");
            content.Add(new StringContent(book.Title), "Title");
            content.Add(new StringContent(book.TotalPages.ToString()), "TotalPages");
            content.Add(new StringContent(book.Description ?? ""), "Description");
            content.Add(new StringContent(book.Author.Name), "AuthorName");

            if (!string.IsNullOrWhiteSpace(book?.Publisher.Name))
                content.Add(new StringContent(book.Publisher.Name), "PublisherName");

            if (!string.IsNullOrWhiteSpace(book?.Genre?.Name))
                content.Add(new StringContent(book.Genre.Name), "GenreName");

            var stream = File.OpenRead(coverPath);
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

            content.Add(streamContent, "Cover", Path.GetFileName(coverPath));

            var response = await _httpClient.PostAsync("/api/books", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }

            return (true, null);
        }


        public async Task<(List<BookVM> Books, DateTime? CursorDate, int? CursorId)>
            GetNextBooksAsync(int count, DateTime? cursorDate, int? cursorId, string? search)
        {
            var query = $"?count={count}";

            if (cursorDate.HasValue)
                query += $"&cursorDate={Uri.EscapeDataString(cursorDate.Value.ToString("o"))}";

            if (cursorId.HasValue)
                query += $"&cursorKey={cursorId.Value}";

            if (!string.IsNullOrWhiteSpace(search))
                query += $"&search={Uri.EscapeDataString(search)}";

            var response = await _httpClient.GetAsync("/api/books/next" + query);

            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);

            var root = doc.RootElement;

            var books = new List<BookVM>();

            foreach (var bookJson in root.GetProperty("books").EnumerateArray())
            {
                var book = new BookVM();
                book.FromJson(bookJson);
                books.Add(book);
            }

            DateTime? nextCursorDate = null;
            int? nextCursorId = null;

            if (root.TryGetProperty("cursorDate", out var cd) && cd.ValueKind != JsonValueKind.Null)
                nextCursorDate = cd.GetDateTime();

            if (root.TryGetProperty("cursorId", out var ci) && ci.ValueKind != JsonValueKind.Null)
                nextCursorId = ci.GetInt32();

            return (books, nextCursorDate, nextCursorId);
        }


    }
}
