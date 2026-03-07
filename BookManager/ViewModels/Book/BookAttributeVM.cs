using BookManager.ApiClients;
using BookManager.Models.Book;
using BookManager.Views.Book;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BookManager.ViewModels.Book
{
    public partial class BookAttributeVM : PagedLoadingVM, IQueryAttributable
    {

        [ObservableProperty]
        AuthorVM author = new();

        [ObservableProperty]
        PublisherVM publisher = new();

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
            if (query.TryGetValue($"{nameof(Author)}", out var obj) && obj is AuthorVM author)
            {
                EntityType = nameof(AuthorVM);
                attributeId = author.Id;
                Author.CopyFrom(author);
                Title = $"{Author.Name}";
                query.Remove($"{nameof(Author)}");
            }
            else if (query.TryGetValue($"{nameof(Publisher)}", out var obj2) && obj2 is PublisherVM publisher)
            {
                EntityType = nameof(PublisherVM);
                Publisher.CopyFrom(publisher);
                attributeId = publisher.Id;
                Title = $"{Publisher.Name}";
                query.Remove($"{nameof(Publisher)}");
            }
            else if (query.TryGetValue($"{nameof(Genre)}", out var obj3) && obj3 is GenreVM genre)
            {
                EntityType = nameof(GenreVM);
                Genre.CopyFrom(genre);
                attributeId = genre.Id;
                Title = $"{Genre.Name}";
                query.Remove($"{nameof(Genre)}");
            }

            if (query.TryGetValue("Updated", out var updatedObj) && updatedObj is bool wasUpdated && wasUpdated)
            {
                await Refresh();
                query.Remove("Updated");
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
            switch (EntityType)
            {
                case nameof(AuthorVM):
                    await Shell.Current.GoToAsync( nameof(FormPage), new Dictionary<string, object> { [nameof(FormVM.Author)] = Author });
                    break;
                case nameof(PublisherVM):
                    await Shell.Current.GoToAsync( nameof(FormPage), new Dictionary<string, object> { [nameof(FormVM.Publisher)] = Publisher });
                    break;
                case nameof(GenreVM):
                    await Shell.Current.GoToAsync( nameof(FormPage), new Dictionary<string, object> { [nameof(FormVM.Genre)] = Genre });
                    break;
            }
        }

        [RelayCommand]
        public async Task Delete()
        {
            string title = EntityType switch
            {
                nameof(AuthorVM) => $"Delete {Author.Name}",
                nameof(PublisherVM) => $"Delete {Publisher.Name}",
                nameof(GenreVM) => $"Delete {Genre.Name}",
                _ => "Delete"
            };

            bool confirm = await Shell.Current.DisplayAlertAsync(title, "This action cannot be undone", "Yes", "No");

            if (!confirm)
                return;

            try
            {
                RequestResult result = new RequestResult(false, "error");
                string name = EntityType;
                switch (EntityType)
                {
                    case nameof(AuthorVM):
                        name = Author.Name;
                        result = await _bookClient.DeleteAuthorAsync(Author.Id);
                        break;
                    case nameof(PublisherVM):
                        name = Publisher.Name;
                        result = await _bookClient.DeletePublisherAsync(Publisher.Id);
                        break;
                    case nameof(GenreVM):
                        name = Genre.Name;
                        result = await _bookClient.DeleteGenreAsync(Genre.Id);
                        break;
                }

                if (!result.Success)
                {
                    await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                    return;
                }

                _ = Toast.Make($"{name} deleted").Show();

                switch (EntityType)
                {
                    case nameof(AuthorVM):
                        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<AuthorVM>(Author), Messages.AuthorDeleted);
                        break;

                    case nameof(PublisherVM):
                        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<PublisherVM>(Publisher),Messages.PublisherDeleted);
                        break;

                    case nameof(GenreVM):
                        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<GenreVM>(Genre), Messages.GenreDeleted);
                        break;
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
            await Shell.Current.GoToAsync(nameof(BookDetailPage), new Dictionary<string, object> { [nameof(BookDetailVM.Book)] = book });
        }
    }
}
