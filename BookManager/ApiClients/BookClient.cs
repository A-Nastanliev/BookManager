using BookManager.Models.Book;
using BookManager.ViewModels.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BookManager.ApiClients
{
    public class BookClient
    {
        readonly HttpClient _httpClient;

        public BookClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
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
                return (false, await ApiErrorParser.ParseAsync(response));
            }

            return (true, null);
        }

        public async Task<(bool Success, BookVM? Book, string? Error)> GetBookByIsbnAsync(string isbn)
        {
            var response = await _httpClient.GetAsync(
                $"/api/books/{Uri.EscapeDataString(isbn)}");

            if (response.StatusCode == HttpStatusCode.NotFound)
                return (true, null, null);

            if (!response.IsSuccessStatusCode)
            {
                return (false,null, await ApiErrorParser.ParseAsync(response));
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var book = new BookVM();
            book.FromJson(doc.RootElement);

            return (true, book, null);
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

            if (!response.IsSuccessStatusCode)
                throw new Exception(await ApiErrorParser.ParseAsync(response));

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

        public async Task<(bool Success, string? Error)> UpdateBookAsync(BookVM book, string? coverPath)
        {
            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(book.ISBN), "ISBN");
            content.Add(new StringContent(book.Title), "Title");
            content.Add(new StringContent(book.TotalPages.ToString()), "TotalPages");
            content.Add(new StringContent(book.Description ?? ""), "Description");
            content.Add(new StringContent(book.Author.Name), "AuthorName");

            if (!string.IsNullOrWhiteSpace(book?.Publisher?.Name))
                content.Add(new StringContent(book.Publisher.Name), "PublisherName");

            if (!string.IsNullOrWhiteSpace(book?.Genre?.Name))
                content.Add(new StringContent(book.Genre.Name), "GenreName");

            bool hasImage = !string.IsNullOrWhiteSpace(coverPath) && File.Exists(coverPath);

            if (hasImage)
            {
                var stream = File.OpenRead(coverPath);
                var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

                content.Add(streamContent, "Cover", Path.GetFileName(coverPath));
            }

            var response = await _httpClient.PutAsync($"/api/books/{book.Id}", content);

            if (!response.IsSuccessStatusCode)
            {
                return (false, await ApiErrorParser.ParseAsync(response));
            }

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> CreateAuthorAsync(AuthorVM author)
        {
            var json = JsonSerializer.Serialize(new
            {
                name = author.Name,
                biography = author.Biography,
                birthDate = author.BirthDate
            });

            var response = await _httpClient.PostAsync(
                "/api/books/authors",
                new StringContent(json, Encoding.UTF8, "application/json"));

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            if (!response.IsSuccessStatusCode)
            {
                return (false, await ApiErrorParser.ParseAsync(response));
            }

            author.Id = doc.RootElement.GetProperty("id").GetInt32();
            return (true,  null);
        }

        public async Task<(List<AuthorVM> Authors, int? CursorKey)> GetNextAuthorsAsync(int count, int? cursorKey, string search)
        {
            var query = $"?count={count}";

            if (cursorKey.HasValue)
                query += $"&cursorKey={cursorKey.Value}";

            if (!string.IsNullOrWhiteSpace(search))
                query += $"&search={Uri.EscapeDataString(search)}";

            var response = await _httpClient.GetAsync("/api/books/authors" + query);

            if (!response.IsSuccessStatusCode)
                throw new Exception(await ApiErrorParser.ParseAsync(response));

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);

            var root = doc.RootElement;

            var authors = new List<AuthorVM>();

            foreach (var json in root.GetProperty("authors").EnumerateArray())
            {
                var vm = new AuthorVM();
                vm.FromJson(json);
                authors.Add(vm);
            }

            int? nextCursorKey = null;

            if (root.TryGetProperty("cursorKey", out var ck) && ck.ValueKind != JsonValueKind.Null)
                nextCursorKey = ck.GetInt32();

            return (authors, nextCursorKey);
        }

        public async Task<(bool Success, string? Error)> UpdateAuthorAsync(AuthorVM author)
        {
            var json = JsonSerializer.Serialize(new
            {
                name = author.Name,
                biography = author.Biography,
                birthDate = author.BirthDate
            });

            var response = await _httpClient.PutAsync($"/api/books/authors/{author.Id}", new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                return (false, await ApiErrorParser.ParseAsync(response));
            }

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> DeleteAuthorAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"/api/books/authors/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return (false, await ApiErrorParser.ParseAsync(response));
            }

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> CreateGenreAsync(GenreVM genre)
        {
            var json = JsonSerializer.Serialize(new
            {
                name = genre.Name,
                description = genre.Description
            });

            var response = await _httpClient.PostAsync(
                "/api/books/genres",
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                return (false, await ApiErrorParser.ParseAsync(response));
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            genre.Id = doc.RootElement.GetProperty("id").GetInt32();

            return (true, null);
        }

        public async Task<(List<GenreVM> Genres, int? CursorKey)> GetNextGenresAsync(int count, int? cursorKey, string search)
        {
            var query = $"?count={count}";

            if (cursorKey.HasValue)
                query += $"&cursorKey={cursorKey.Value}";

            if (!string.IsNullOrWhiteSpace(search))
                query += $"&search={Uri.EscapeDataString(search)}";

            var response = await _httpClient.GetAsync("/api/books/genres" + query);

            if (!response.IsSuccessStatusCode)
                throw new Exception(await ApiErrorParser.ParseAsync(response));

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);

            var root = doc.RootElement;

            var genres = new List<GenreVM>();

            foreach (var json in root.GetProperty("genres").EnumerateArray())
            {
                var vm = new GenreVM();
                vm.FromJson(json);
                genres.Add(vm);
            }

            int? nextCursorKey = null;

            if (root.TryGetProperty("cursorKey", out var ck) && ck.ValueKind != JsonValueKind.Null)
                nextCursorKey = ck.GetInt32();

            return (genres, nextCursorKey);
        }

        public async Task<(bool Success, string? Error)> UpdateGenreAsync(GenreVM genre)
        {
            var json = JsonSerializer.Serialize(new
            {
                name = genre.Name,
                description = genre.Description
            });

            var response = await _httpClient.PutAsync($"/api/books/genres/{genre.Id}",
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                return (false, await ApiErrorParser.ParseAsync(response));
            }

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> DeleteGenreAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"/api/books/genres/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return (false, await ApiErrorParser.ParseAsync(response));
            }

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> CreatePublisherAsync(PublisherVM publisher)
        {
            var json = JsonSerializer.Serialize(new
            {
                name = publisher.Name,
                description = publisher.Description,
                website = publisher.Website
            });

            var response = await _httpClient.PostAsync(
                "/api/books/publishers",
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                return (false, await ApiErrorParser.ParseAsync(response));
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            publisher.Id = doc.RootElement.GetProperty("id").GetInt32();

            return (true, null);
        }

        public async Task<(List<PublisherVM> Publishers, int? CursorKey)> GetNextPublishersAsync(int count, int? cursorKey, string search)
        {
            var query = $"?count={count}";

            if (cursorKey.HasValue)
                query += $"&cursorKey={cursorKey.Value}";

            if (!string.IsNullOrWhiteSpace(search))
                query += $"&search={Uri.EscapeDataString(search)}";

            var response = await _httpClient.GetAsync("/api/books/publishers" + query);

            if (!response.IsSuccessStatusCode)
                throw new Exception(await ApiErrorParser.ParseAsync(response));

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);

            var root = doc.RootElement;

            var publishers = new List<PublisherVM>();

            foreach (var json in root.GetProperty("publishers").EnumerateArray())
            {
                var vm = new PublisherVM();
                vm.FromJson(json);
                publishers.Add(vm);
            }

            int? nextCursorKey = null;

            if (root.TryGetProperty("cursorKey", out var ck) && ck.ValueKind != JsonValueKind.Null)
                nextCursorKey = ck.GetInt32();

            return (publishers, nextCursorKey);
        }

        public async Task<(bool Success, string? Error)> UpdatePublisherAsync(PublisherVM publisher)
        {
            var json = JsonSerializer.Serialize(new
            {
                name = publisher.Name,
                description = publisher.Description,
                website = publisher.Website
            });

            var response = await _httpClient.PutAsync(
                $"/api/books/publishers/{publisher.Id}",
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                return (false, await ApiErrorParser.ParseAsync(response));
            }

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> DeletePublisherAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"/api/books/publishers/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return (false, await ApiErrorParser.ParseAsync(response));
            }

            return (true, null);
        }
    }
}
