using BookManager.ApiClients;
using BookManager.Models.Reading;
using BookManager.Models.User;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BookManager.ViewModels.Book
{
    public partial class MyBookRequestsVM : PagedLoadingVM
    {
        [ObservableProperty]
        ObservableCollection<BookRequestVM> requests = new();

        [ObservableProperty]
        BookRequestVM newRequest;

        readonly ReadingClient _readingClient;
        readonly UserVM _userVM;

        public MyBookRequestsVM(ReadingClient readingClient, UserVM user)
        {
            _readingClient = readingClient;
            _userVM = user;
            NewRequest = new BookRequestVM(user.PublicUser);
            UserClient.OnLogout += () =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    CursorDate = null;
                    CursorId = null;
                    CanLoadMore = true;
                    Requests.Clear();
                });
                return Task.CompletedTask;
            };
        }

        [RelayCommand]
        public async Task MakeRequest()
        {
            if(string.IsNullOrWhiteSpace(NewRequest.Title) || string.IsNullOrWhiteSpace(NewRequest.Isbn) || NewRequest.Isbn?.Length < 13)
            {
                await Shell.Current.DisplayAlertAsync("Invalid input", "Requests require book title and isbn. Isbn must be 13 characters", "OK");
                return;
            }

            try
            {
                NewRequest.Title = NewRequest.Title.Trim();
                NewRequest.Isbn = NewRequest.Isbn.Trim();
                var result = await _readingClient.CreateBookRequestAsync(NewRequest);
                if (!result.Success)
                {
                    await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                    return;
                }
                Requests.Insert(0, NewRequest);
                NewRequest.DateSent = DateTime.UtcNow;
                NewRequest = new BookRequestVM(_userVM.PublicUser);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        [RelayCommand]
        public async override Task Load()
        {
            if (!CanStartLoading())
                return;

            BeginLoading();

            try
            {
                var (bookRequests, cursorDate, cursorKey) = await _readingClient.GetMyNextBookRequestsAsync(BatchSize, CursorDate, CursorId);

                if (bookRequests.Any())
                {
                     foreach (var b in bookRequests)
                     {
                        Requests.Add(b);
                     }

                    EndLoading(bookRequests.Count, cursorDate, cursorKey);
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
                Requests.Clear();
                await Load();
            }
            finally
            {
                IsRefreshing = false;
            }
        }
    }
}
