using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace BookManager.Models.Book
{
    public partial class BookVM : ObservableObject, IJsonParseable, ICopyable<BookVM>
    {
        [ObservableProperty]
        int id;

        [ObservableProperty]
        string iSBN;

        [ObservableProperty]
        string title;

        [ObservableProperty]
        string cover;

        [ObservableProperty]
        private ImageSource coverSource;

        [ObservableProperty]
        int? totalPages;

        [ObservableProperty]
        string description;

        [ObservableProperty]
        AuthorVM author;

        [ObservableProperty]
        GenreVM genre;

        [ObservableProperty]
        PublisherVM publisher;

        public BookVM()
        {
            Author = new AuthorVM();
            Genre = new GenreVM();
            Publisher = new PublisherVM();
        }

        public void CopyFrom(BookVM original)
        {
            Id = original.Id;
            Title = original.Title;
            ISBN = original.ISBN;
            Cover = original.Cover;
            CoverSource = original.CoverSource;
            TotalPages = original.TotalPages;
            Description = original.Description;
            Genre.CopyFrom(original.Genre);
            Publisher.CopyFrom(original.Publisher);
            Author.CopyFrom(original.Author);
        }

        public void FromJson(JsonElement json)
        {
            Id = json.GetProperty("id").GetInt32();
            ISBN = json.GetProperty("isbn").GetString()!;
            Title = json.GetProperty("title").GetString()!;
            Cover = json.GetProperty("cover").GetString()!;
            TotalPages = json.GetProperty("totalPages").GetInt32();

            if (!string.IsNullOrWhiteSpace(Cover))
            {
                try
                {
                    CoverSource = ImageSource.FromUri(new Uri(Cover));
                }
                catch
                {
                    CoverSource = null;
                }
            }
            else
            {
                CoverSource = null;
            }

            if (json.TryGetProperty("description", out var desc) && desc.ValueKind != JsonValueKind.Null)
                Description = desc.GetString();

            Author.FromJson(json.GetProperty("authorDto"));

            if (json.TryGetProperty("genreDto", out var genre) && genre.ValueKind != JsonValueKind.Null)
            {
                Genre ??= new GenreVM();
                Genre.FromJson(genre);
            }

            if (json.TryGetProperty("publisherDto", out var publisher) && publisher.ValueKind != JsonValueKind.Null)
            {
                Publisher ??= new PublisherVM();
                Publisher.FromJson(publisher);
            }
        }
    }

}
