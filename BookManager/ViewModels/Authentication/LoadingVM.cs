using BookManager.Services;
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
        private readonly ApiService _apiService;
        private bool _initialized;

        public LoadingVM(ApiService apiService)
        {
            _apiService = apiService;
        }

        public void Reset()
        {
            _initialized = false;
        }

        public async Task InitializeAsync()
        {
            await Task.Delay(100);

            try
            {
                if (_initialized)
                    return;

                var restored = await _apiService.TokenLoginAsync();

                if (restored)
                {
                    _initialized = true;
                    await Shell.Current.GoToAsync($"///{nameof(BookSearchPage)}");
                }
                else
                {
                    await Shell.Current.GoToAsync(nameof(LoginPage));
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync(ex.Message, "0" + ex?.InnerException, "OK");
            }
        }
    }
}
