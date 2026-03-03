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
    public partial class GenresHubVM : PagedLoadingVM
    {
        [ObservableProperty]
        ObservableCollection<GenreVM> genres = new();

        [ObservableProperty]
        ObservableCollection<GenreVM> currentGenres = new();

        [ObservableProperty]
        string entrySearch;

        readonly BookClient _bookClient;

        public GenresHubVM(BookClient bookClient) 
        {
            _bookClient = bookClient;
        }

        [RelayCommand]
        public override async Task Load()
        {
            if (!CanStartLoading())
                return;

            BeginLoading();

            try
            {
                var (genres, cursorKey) = await _bookClient.GetNextGenresAsync(BatchSize, CursorId, EntrySearch);

                if (genres.Any())
                {
                    if (string.IsNullOrWhiteSpace(EntrySearch))
                    {
                        foreach (var g in genres)
                        {
                            CurrentGenres.Add(g);
                            Genres.Add(g);
                        }
                    }
                    else
                    {
                        foreach (var g in genres)
                        {
                            CurrentGenres.Add(g);
                        }
                    }
                    EndLoading(genres.Count, null, cursorKey);
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
                CurrentGenres.Clear();
                Genres.Clear();
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
                    CurrentGenres.Clear();

                    if (string.IsNullOrWhiteSpace(search))
                    {
                        foreach(var g in Genres)
                        {
                            CurrentGenres.Add(g);
                        }
                        CursorId = Genres.LastOrDefault()?.Id;
                        CanLoadMore = Genres.Count % BatchSize == 0;
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
        public async Task Select(GenreVM genre)
        {
            await Shell.Current.GoToAsync(nameof(BookAttributePage), new Dictionary<string, object> { [nameof(BookAttributeVM.NavigationGenre)] = genre });
        }

        [RelayCommand]
        public async Task GoToCreateGenre()
        {
            await Shell.Current.GoToAsync(nameof(FormPage), new Dictionary<string, object> { [nameof(FormVM.EntityType)] = nameof(GenreVM) });
        }
    }
}
