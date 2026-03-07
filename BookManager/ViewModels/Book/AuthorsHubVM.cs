using BookManager.ApiClients;
using BookManager.Models.Book;
using BookManager.Views.Book;
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
    public partial class AuthorsHubVM : PagedLoadingVM
    {
        [ObservableProperty]
        ObservableCollection<AuthorVM> authors = new();

        [ObservableProperty]
        ObservableCollection<AuthorVM> currentAuthors = new();

        [ObservableProperty]
        string entrySearch;

        readonly BookClient _bookClient;

        public AuthorsHubVM(BookClient bookClient)
        {
            _bookClient = bookClient;

            WeakReferenceMessenger.Default.Register<ValueChangedMessage<AuthorVM>, string>(this, Messages.AuthorCreated, (recipient, message) =>
            {
                Authors.Insert(0, message.Value);

                if (string.IsNullOrWhiteSpace(EntrySearch) || message.Value.Name.Contains(EntrySearch, StringComparison.OrdinalIgnoreCase))
                {
                    CurrentAuthors.Insert(0, message.Value);
                }
            });

            WeakReferenceMessenger.Default.Register<ValueChangedMessage<AuthorVM>, string>(this, Messages.AuthorUpdated, (recipient, message) =>
            {
                var author = Authors.FirstOrDefault(a => a.Id == message.Value.Id);
                author?.CopyFrom(message.Value);
            });

            WeakReferenceMessenger.Default.Register<ValueChangedMessage<AuthorVM>, string>(this, Messages.AuthorDeleted, (recipient, message) =>
            {
                var a = Authors.FirstOrDefault(x => x.Id == message.Value.Id);
                if (a != null) Authors.Remove(a);

                a = CurrentAuthors.FirstOrDefault(x => x.Id == message.Value.Id);
                if (a != null) CurrentAuthors.Remove(a);
            });
        }

        [RelayCommand]
        public override async Task Load()
        {
            if (!CanStartLoading())
                return;

            BeginLoading();

            try
            {
                var (authors, cursorKey) = await _bookClient.GetNextAuthorsAsync(BatchSize, CursorId, EntrySearch);

                if (authors.Any())
                {
                    if (string.IsNullOrWhiteSpace(EntrySearch))
                    {
                        foreach (var a in authors)
                        {
                            CurrentAuthors.Add(a);
                            Authors.Add(a);
                        }
                    }
                    else
                    {
                        foreach (var a in authors)
                        {
                            CurrentAuthors.Add(a);
                        }
                    }

                    EndLoading(authors.Count, null, cursorKey);
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

                CurrentAuthors.Clear();
                Authors.Clear();

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
                    CurrentAuthors.Clear();

                    if (string.IsNullOrWhiteSpace(search))
                    {
                        foreach (var a in Authors)
                        {
                            CurrentAuthors.Add(a);
                        }

                        CursorId = Authors.LastOrDefault()?.Id;
                        CanLoadMore = Authors.Count % BatchSize == 0;
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
        public async Task Select(AuthorVM author)
        {
            await Shell.Current.GoToAsync(nameof(BookAttributePage), new Dictionary<string, object> { [nameof(BookAttributeVM.Author)] = author });
        }

        [RelayCommand]
        public async Task GoToCreateAuthor()
        {
            await Shell.Current.GoToAsync(nameof(FormPage), new Dictionary<string, object> { [nameof(FormVM.EntityType)] = nameof(AuthorVM) });
        }
    }
}
