using BookManager.ApiClients;
using BookManager.Authentication;
using BookManager.Views.Authentication;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Alerts;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace BookManager.ViewModels.Authentication
{
    public partial class SignUpVM : ObservableObject
    {
        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private string emailAddress;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private ImageSource profileImage;

        string _selectedImagePath;

        readonly UserClient _userClient;

        public SignUpVM(UserClient userClient) 
        {
            _userClient = userClient;
        }


        [RelayCommand]
        private async Task PickProfilePicture()
        {
            var options = new MediaPickerOptions
            {
                Title = "Pick a profile picture",
                SelectionLimit = 1,
            };
            var results = await MediaPicker.Default.PickPhotosAsync(options);

            if (results == null || results.Count == 0)
                return;

            var result = results[0];
                
            await using var sourceStream = await result.OpenReadAsync();
            var localFilePath = await ImageManager.SaveTempImageAsync(sourceStream, Path.GetExtension(result.FileName));

            ImageManager.CleanupTempImage(_selectedImagePath);
            _selectedImagePath = localFilePath;

            ProfileImage = ImageSource.FromFile(localFilePath);
        }

        [RelayCommand]
        private async Task SignUp()
        {
            if (string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(EmailAddress) ||
                string.IsNullOrWhiteSpace(Password))
            {
                await Shell.Current.DisplayAlertAsync("Error", "All fields are required", "OK");
                return;
            }

            if (!EmailValidator.IsValid(EmailAddress))
            {
                await Shell.Current.DisplayAlertAsync("Invalid Email", "Please, enter a valid email address", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_selectedImagePath))
            {
                await Shell.Current.DisplayAlertAsync(
                    "Error",
                    "Profile picture is required",
                    "OK");
                return;
            }

            if(Password.Length < 4)
            {
                await Shell.Current.DisplayAlertAsync("Password", "Password should be atleast 4 charachters", "OK");
                return;
            }

            try
            {
                var result = await _userClient.SignUpAsync(Username, EmailAddress, Password, _selectedImagePath);

                if (!result.Success)
                {
                    await Shell.Current.DisplayAlertAsync("Sign up failed", result.Error, "OK");
                    return;
                }

                _ = Toast.Make($"{Username} created").Show();
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task GoToLogin()
        {
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }
    }
}
