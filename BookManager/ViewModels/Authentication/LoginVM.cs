using BookManager.Services;
using BookManager.Views.Authentication;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BookManager.ViewModels.Authentication
{
    public partial class LoginVM : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        public LoginVM(ApiService apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        private async Task Login()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Shell.Current.DisplayAlertAsync("Error", "Email and password are required", "OK");
                return;
            }

            try
            {
                var response = await _apiService.EmailLoginAsync(Email, Password);

                if (!response.IsSuccessStatusCode)
                {
                    await Shell.Current.DisplayAlertAsync("Login failed", "Invalid email or password", "OK");
                    return;
                }

                await Shell.Current.GoToAsync("//LoadingPage");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task GoToSignUp()
        {
            await Shell.Current.GoToAsync(nameof(SignUpPage));
        }
    }
}
