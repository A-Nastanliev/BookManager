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
    public partial class PublishersHubVM : PagedLoadingVM
    {
        [ObservableProperty]
        ObservableCollection<PublisherVM> publishers = new();

        [ObservableProperty]
        ObservableCollection<PublisherVM> currentPublishers = new();

        [ObservableProperty]
        string entrySearch;

        readonly BookClient _bookClient;

        public PublishersHubVM(BookClient bookClient)
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
                var (publishers, cursorKey) = await _bookClient.GetNextPublishersAsync(BatchSize, CursorId, EntrySearch);

                if (publishers.Any())
                {
                    if (string.IsNullOrWhiteSpace(EntrySearch))
                    {
                        foreach (var p in publishers)
                        {
                            CurrentPublishers.Add(p);
                            Publishers.Add(p);
                        }
                    }
                    else
                    {
                        foreach (var p in publishers)
                        {
                            CurrentPublishers.Add(p);
                        }
                    }
                    EndLoading(publishers.Count, null, cursorKey);
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
                CurrentPublishers.Clear();
                Publishers.Clear();
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
                    CurrentPublishers.Clear();

                    if (string.IsNullOrWhiteSpace(search))
                    {
                        foreach (var g in Publishers)
                        {
                            CurrentPublishers.Add(g);
                        }
                        CursorId = Publishers.LastOrDefault()?.Id;
                        CanLoadMore = Publishers.Count % BatchSize == 0;
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
        public async Task Select(PublisherVM publisher)
        {
            await Shell.Current.GoToAsync(nameof(BookAttributePage), new Dictionary<string, object> { [nameof(BookAttributeVM.NavigationPublisher)] = publisher });
        }

        [RelayCommand]
        public async Task GoToCreatePublisher()
        {
            await Shell.Current.GoToAsync(nameof(FormPage), new Dictionary<string, object> { [nameof(FormVM.EntityType)] = nameof(PublisherVM) });
        }
    }
}
