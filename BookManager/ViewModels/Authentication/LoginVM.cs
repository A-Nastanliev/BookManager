using BookManager.ApiClients;
using BookManager.Views.Authentication;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BookManager.Authentication;
using BookManager.Views.Book;

namespace BookManager.ViewModels.Authentication
{
    public partial class LoginVM : ObservableObject
    {
        private readonly UserClient _userClient;

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        public LoginVM(UserClient userClient)
        {
            _userClient = userClient;
        }

        [RelayCommand]
        private async Task Login()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Shell.Current.DisplayAlertAsync("Error", "Email and password are required", "OK");
                return;
            }

            if (!EmailValidator.IsValid(Email))
            {
                await Shell.Current.DisplayAlertAsync("Invalid Email", "Please, enter a valid email address", "OK");
                return;
            }

            try
            {
                var result = await _userClient.EmailLoginAsync(Email, Password);

                if (!result.Success)
                {
                    await Shell.Current.DisplayAlertAsync("Login failed", result.Error ?? "Invalid email or password", "OK");
                    return;
                }

                await Shell.Current.GoToAsync($"//{nameof(BookSearchPage)}");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.ToString(), "OK");
            }
        }

        [RelayCommand]
        private async Task GoToSignUp()
        {
            await Shell.Current.GoToAsync($"//{nameof(SignUpPage)}");
        }
    }
}
