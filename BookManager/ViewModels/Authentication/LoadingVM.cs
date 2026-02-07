using BookManager.ApiClients;
using BookManager.Views.Authentication;
using BookManager.Views.Book;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookManager.ViewModels.Authentication
{
    public partial class LoadingVM : ObservableObject
    {
        private readonly UserClient _userClient;

        public LoadingVM(UserClient userClient)
        {
            _userClient = userClient;
        }

        public async Task InitializeAsync()
        {
            try
            {
                var result = await _userClient.TokenLoginAsync();

                if (result.Success)
                {
                    await Shell.Current.GoToAsync($"//{nameof(BookSearchPage)}");
                }
                else
                {
                    await Shell.Current.GoToAsync(nameof(LoginPage));
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
    }
}
