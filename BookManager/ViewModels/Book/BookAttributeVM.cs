using BookManager.ApiClients;
using BookManager.Models.Book;
using BookManager.Views.Book;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BookManager.ViewModels.Book
{
    public partial class BookAttributeVM : PagedLoadingVM, IQueryAttributable
    {
        [ObservableProperty]
        AuthorVM navigationAuthor;

        [ObservableProperty]
        AuthorVM author = new();

        [ObservableProperty]
        PublisherVM navigationPublisher;

        [ObservableProperty]
        PublisherVM publisher = new();

        [ObservableProperty]
        GenreVM navigationGenre;

        [ObservableProperty]
        GenreVM genre = new();

        [ObservableProperty]
        string entityType;

        [ObservableProperty]
        string title;

        [ObservableProperty]
        ObservableCollection<BookVM> books = new();

        [ObservableProperty]
        int span;

        int attributeId;

        readonly BookClient _bookClient;

        public BookAttributeVM(BookClient bookClient)
        {
            _bookClient = bookClient;
            Span = 1;
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue($"{nameof(NavigationAuthor)}", out var obj) && obj is AuthorVM author)
            {
                EntityType = nameof(AuthorVM);
                NavigationAuthor = author;
                attributeId = author.Id;
                Author.CopyFrom(author);
                Title = $"{NavigationAuthor.Name}";
            }
            else if (query.TryGetValue($"{nameof(NavigationPublisher)}", out var obj2) && obj2 is PublisherVM publisher)
            {
                EntityType = nameof(PublisherVM);
                NavigationPublisher = publisher;
                Publisher.CopyFrom(publisher);
                attributeId = publisher.Id;
                Title = $"{NavigationPublisher.Name}";
            }
            else if (query.TryGetValue($"{nameof(NavigationGenre)}", out var obj3) && obj3 is GenreVM genre)
            {
                EntityType = nameof(GenreVM);
                NavigationGenre = genre;
                Genre.CopyFrom(genre);
                attributeId = genre.Id;
                Title = $"{NavigationGenre.Name}";
            }

            if (query.TryGetValue("Updated", out var updatedObj) && updatedObj is bool wasUpdated && wasUpdated)
            {
                await Refresh();
                return;
            }
        }

        [RelayCommand]
        public override async Task Load()
        {
            if (!CanStartLoading())
                return;

            BeginLoading();

            try
            {
                var (books, cursorDate, cursorKey) = await _bookClient.GetNextBooksByAttributeAsync(EntityType, attributeId,BatchSize, CursorDate, CursorId);
                if (books.Any())
                {
                    foreach (var b in books)
                    {
                        Books.Add(b);
                    }
                    EndLoading(books.Count, cursorDate, cursorKey);
                    Span = 2;
                    return;
                }

                EndLoading(0, null, null);
            }
            catch (Exception ex)
            {
                Loading = false;
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        [RelayCommand]
        public async Task Edit()
        {
            if (EntityType == nameof(AuthorVM))
                await Shell.Current.GoToAsync(nameof(FormPage), new Dictionary<string, object> { [nameof(FormVM.NavigationAuthor)] = Author });
            else if (EntityType == nameof(PublisherVM))
                await Shell.Current.GoToAsync(nameof(FormPage), new Dictionary<string, object> { [nameof(FormVM.NavigationPublisher)] = Publisher });
            else if (EntityType == nameof(GenreVM))
                await Shell.Current.GoToAsync(nameof(FormPage), new Dictionary<string, object> { [nameof(FormVM.NavigationGenre)] = Genre });
        }

        [RelayCommand]
        public async Task Delete()
        {
            try
            {
                RequestResult result = new RequestResult(false, "error");
                if (EntityType == nameof(AuthorVM))
                    result = await _bookClient.DeleteAuthorAsync(Author.Id);
                else if (EntityType == nameof(PublisherVM))
                    result = await _bookClient.DeletePublisherAsync(Publisher.Id);
                else if (EntityType == nameof(GenreVM))
                    result = await _bookClient.DeleteGenreAsync(Genre.Id);

                if (!result.Success)
                {
                    await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                    return;
                }

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex) 
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }


        [RelayCommand]
        public override async Task Refresh()
        {
            try
            {
                Span = 1;
                CursorDate = null;
                CursorId = null;
                CanLoadMore = true;
                Books.Clear();
                await Load();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public async Task Select(BookVM book)
        {
            await Shell.Current.GoToAsync(nameof(BookFormPage), new Dictionary<string, object> { [nameof(BookFormVM.NavigationBook)] = book });
        }
    }
}
