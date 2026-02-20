using BookManager.ApiClients;
using BookManager.Models.Book;
using BookManager.ViewModels.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using BookManager.Views.Book;

namespace BookManager.ViewModels.Book
{
    public partial class BookSearchVM : PagedLoadingVM
    {
        [ObservableProperty]
        UserVM user;

        [ObservableProperty]
        ObservableCollection<BookVM> books = new();

        [ObservableProperty]
        ObservableCollection<BookVM> currentBooks = new();

        [ObservableProperty]
        ObservableCollection<BookVM> searchedBooks = new();

        [ObservableProperty]
        string entrySearch;

        BookClient _bookClient;

        DateTime? BooksCursorDate { get; set; }
        CancellationTokenSource? _searchCts;

        public BookSearchVM(UserVM user, BookClient bookClient)
        {
            _bookClient = bookClient;
            User = user;
            CurrentBooks = Books;
        }

        [RelayCommand]
        public async override Task Load()
        {
            if (!CanStartLoading())
                return;

            BeginLoading();

            try
            {
                 var ( books, cursorDate, cursorKey) = await _bookClient.GetNextBooksAsync(BatchSize, CursorDate, CursorId, EntrySearch);

                if (books.Any())
                {
                    foreach (var b in books)
                    {
                        CurrentBooks.Add(b);
                    }

                    EndLoading(books.Count, cursorDate, cursorKey);

                    if (string.IsNullOrWhiteSpace(EntrySearch))
                    {
                        BooksCursorDate = cursorDate;
                    }
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
        public async Task Refresh()
        {
            BooksCursorDate = null;
            CursorDate = null;
            CursorId = null;
            CanLoadMore = true;
            Books.Clear();
            EntrySearch = null;
            await Load();
            IsRefreshing = false;
        }

        private async Task HandleSearchAsync(string search, CancellationToken token)
        {
            try
            {
                await Task.Delay(500, token);

                if (token.IsCancellationRequested)
                    return;

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    CursorDate = null;
                    CursorId = null;
                    CanLoadMore = true;
                    Loading = false;

                    if (string.IsNullOrWhiteSpace(search))
                    {
                        CurrentBooks = Books;
                        CursorDate = BooksCursorDate;
                        CursorId = Books.LastOrDefault()?.Id;
                        return;
                    }

                    SearchedBooks.Clear();
                    CurrentBooks = SearchedBooks;
                    await Load();
                });
            }
            catch (TaskCanceledException)
            {
            }
        }

        partial void OnEntrySearchChanged(string oldValue, string newValue)
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();

            _ = HandleSearchAsync(newValue, _searchCts.Token);
        }

        [RelayCommand]
        public async Task Select(BookVM book)
        {
            await Shell.Current.GoToAsync(nameof(BookFormPage), new Dictionary<string, object> { [nameof(BookFormVM.NavigationBook)] = book });
        }

        [RelayCommand]
        public async Task GoToCreateBook()
        {
            await Shell.Current.GoToAsync(nameof(BookFormPage));
        }
    }
}
