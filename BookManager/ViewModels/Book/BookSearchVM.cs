using BookManager.ApiClients;
using BookManager.Models.Book;
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
        ObservableCollection<BookVM> books = new();

        [ObservableProperty]
        ObservableCollection<BookVM> currentBooks = new();

        [ObservableProperty]
        string entrySearch;

        BookClient _bookClient;

        DateTime? BooksCursorDate { get; set; }

        public BookSearchVM(BookClient bookClient)
        {
            _bookClient = bookClient;
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
                    if (string.IsNullOrWhiteSpace(EntrySearch))
                    {
                        foreach (var b in books)
                        {
                            CurrentBooks.Add(b);
                            Books.Add(b);
                        }
                        BooksCursorDate = cursorDate;
                    }
                    else
                    {
                        foreach (var b in books)
                        {
                            CurrentBooks.Add(b);
                        }
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
                BooksCursorDate = null;
                CursorDate = null;
                CursorId = null;
                CanLoadMore = true;
                CurrentBooks.Clear();
                Books.Clear();
                EntrySearch = null;
                await Load();
            }
            finally
            {
                IsRefreshing = false;
            }
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
                    Loading = false;
                    CurrentBooks.Clear();

                    if (string.IsNullOrWhiteSpace(search))
                    {
                        foreach(var b in Books)
                        {
                            CurrentBooks.Add(b);
                        }
                        CursorDate = BooksCursorDate;
                        CursorId = Books.LastOrDefault()?.Id;
                        CanLoadMore = Books.Count % BatchSize == 0;
                        return;
                    }

                    CanLoadMore = true;
                    await Load();
                });
            }
            catch (TaskCanceledException)
            {
            }
        }

        partial void OnEntrySearchChanged(string oldValue, string newValue)
        {
            if (IsRefreshing)
                return;

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

        [RelayCommand]
        public async Task SearchIsbnAsync(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                await Shell.Current.DisplayAlertAsync("Error", "ISBN is required.", "OK");
                return;
            }

            try
            {
                var (result, book) =
                    await _bookClient.GetBookByIsbnAsync(barcode);

                if (!result.Success)
                {
                    await Shell.Current.DisplayAlertAsync("Error",
                        result.Error ?? "Unexpected error occurred.",
                        "OK");
                    return;
                }

                if (book == null)
                {
                    await Shell.Current.DisplayAlertAsync("Not found", $"No book with ISBN {barcode} exists.", "OK");
                    return;
                }

                await SelectCommand.ExecuteAsync(book);
            }
            catch (Exception ex)
            {

                await Shell.Current.DisplayAlertAsync("Connection Error", $"Unable to contact server.\n\n{ex.Message}", "OK");
            }
        }
    }
}
