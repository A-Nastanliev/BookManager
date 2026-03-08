using BookManager.ApiClients;
using BookManager.Models.User;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using BookManager.Models.Reading;

namespace BookManager.ViewModels.Book
{
    public partial class BookRequestsVM : PagedLoadingVM
    {
        [ObservableProperty]
        int selectedSegment;

        [ObservableProperty]
        ObservableCollection<BookRequestVM> requests = new();

        [ObservableProperty]
        BookRequestVM selectedRequest = new();

        [ObservableProperty]
        UserVM user;

        public Func<Task> OpenBottomSheet;

        public Func<Task> CloseBottomSheet;

        ReadingClient _readingClient;

        public BookRequestsVM(ReadingClient readingClient, UserVM userVM)
        {
            _readingClient = readingClient;
            User = userVM;
        }

        [RelayCommand]
        public override async Task Load()
        {
            if (!CanStartLoading()) 
                return;

            BeginLoading();

            try
            {
                var (requests, cursorDate, cursorId) =
                    await _readingClient.GetNextBookRequestsAsync((BookRequestStatus)SelectedSegment, BatchSize, CursorDate, CursorId);

                foreach (var r in requests)
                {
                    Requests.Add(r);
                }

                if (requests.Any())
                {
                    EndLoading(requests.Count, cursorDate, cursorId);
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
        public Task AcceptRequest()
        {
            return HandleRequestAsync(BookRequestStatus.Accept);
        }

        [RelayCommand]
        public Task DeclineRequest()
        {
            return HandleRequestAsync(BookRequestStatus.Declined);
        }

        private async Task HandleRequestAsync(BookRequestStatus status)
        {
            if (SelectedRequest == null) return;

            try
            {
                var result = await _readingClient.UpdateBookRequestAsync(SelectedRequest.Id, status);

                if (!result.Success)
                {
                    await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                    return;
                }

                Requests.Remove(SelectedRequest);
                await CloseBottomSheet?.Invoke();
                SelectedRequest = null;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }


        partial void OnSelectedSegmentChanged(int oldValue, int newValue)
        {
            Requests.Clear();
            CanLoadMore = true;
            Loading = false;
            CursorId = null;
            CursorDate = null;
            LoadCommand.Execute(null);
        }

        [RelayCommand]
        public override async Task Refresh()
        {
            try
            {
                Requests.Clear();
                CanLoadMore = true;
                Loading = false;
                CursorId = null;
                CursorDate = null;
                await Load();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        public async Task OnAppearingAsync()
        {
            await Load();
        }

        [RelayCommand]
        public async Task SelectRequest(BookRequestVM request)
        {
            SelectedRequest = request;
            await OpenBottomSheet?.Invoke();
        }
    }
}
