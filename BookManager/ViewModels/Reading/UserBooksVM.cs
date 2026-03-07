using BookManager.ApiClients;
using BookManager.Models.Book;
using BookManager.ViewModels.Book;
using BookManager.Views.Book;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BookManager.ViewModels.Reading
{
    public partial class UserBooksVM : PagedLoadingVM, IQueryAttributable
    {
        [ObservableProperty]
        ObservableCollection<BookVM> books = new();

        [ObservableProperty]
        UserBookStatus status;

        [ObservableProperty]
        string title;

        static readonly Queue<UserBookStatus> _statusQueue = new(new[] { UserBookStatus.Wishlisted, UserBookStatus.Reading, UserBookStatus.Finished });

        readonly ReadingClient _readingClient;

        public UserBooksVM(ReadingClient readingClient) 
        {
            _readingClient = readingClient;
            if (_statusQueue.Count > 0)
                Status = _statusQueue.Dequeue();

            WeakReferenceMessenger.Default.Register<ValueChangedMessage<(BookVM , UserBookStatus)>, string>(this, Messages.UserBookStatusChanged,
                (receptient, message) =>
            {
                var (book, newStatus) = message.Value;

                if (newStatus == this.Status)
                {
                    if (!Books.Any(b => b.Id == book.Id))
                    {
                        var copy = new BookVM();
                        copy.CopyFrom(book);
                        Books.Insert(0, copy);
                        if(Books.Count == 1)
                        {
                            CursorDate = DateTime.UtcNow;
                            CursorId = book.Id;
                        }
                    }
                }
                else
                {
                    var existing = Books.FirstOrDefault(b => b.Id == book.Id);
                    if (existing != null)
                        Books.Remove(existing);
                }
            });
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("deletedBookId", out var idObj) && int.TryParse(idObj?.ToString(), out var id))
            {
                var deletedBook = Books.FirstOrDefault(b => b.Id == id);
                if (deletedBook != null)
                    Books.Remove(deletedBook);

                query.Remove("deletedBookId");
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
                var (books, cursorDate, cursorKey) = await _readingClient.GetNextUserBooksByStatusAsync(Status, BatchSize, CursorDate, CursorId);
                if (books.Any())
                {
                    foreach (var b in books)
                    {
                        Books.Add(b);
                    }
                    EndLoading(books.Count, cursorDate, cursorKey);
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
        public override async Task Refresh()
        {
            try
            {
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

        partial void OnStatusChanged(UserBookStatus value)
        {
            switch (status)
            {
                case UserBookStatus.Wishlisted:
                    Title = "My Wishlist";
                    break;
                case UserBookStatus.Reading:
                    Title = "Currently Reading";
                    break;
                case UserBookStatus.Finished:
                    Title = "Finished Books";
                    break;
            }
        }

    }
}
