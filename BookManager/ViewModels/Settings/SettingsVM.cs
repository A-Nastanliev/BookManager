using BookManager.ApiClients;
using BookManager.Authentication;
using BookManager.ViewModels.Models;
using BookManager.Views.Authentication;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Maui.Alerts;

namespace BookManager.ViewModels.Settings
{
    public partial class SettingsVM: ObservableObject
    {
        readonly UserClient _userClient;
        [ObservableProperty]
        UserVM user;

        [ObservableProperty]
        bool isPictureChanged;

        [ObservableProperty]
        ImageSource profileImageSource;

        string _selectedImagePath;

        [ObservableProperty]
        string oldPassword;

        [ObservableProperty]
        string newPassword;

        [ObservableProperty]
        string newUsername;

        [ObservableProperty]
        string newEmail;

        public SettingsVM(UserClient userClient, UserVM userVM)
        {
            _userClient = userClient;
            User = userVM;
        }

        [RelayCommand]
        private async Task PickProfilePicture()
        {
            var options = new MediaPickerOptions
            {
                Title = "Pik a profile picture",
                SelectionLimit = 1,
            };
            var results = await MediaPicker.Default.PickPhotosAsync(options);

            if (results == null || results.Count == 0)
                return;

            var result = results[0];

            var localFilePath = Path.Combine(FileSystem.CacheDirectory, $"{Guid.NewGuid()}{Path.GetExtension(result.FileName)}");

            using (var sourceStream = await result.OpenReadAsync())
            using (var localFileStream = File.OpenWrite(localFilePath))
            {
                await sourceStream.CopyToAsync(localFileStream);
            }

            ImageCleaner.CleanupTempImage(_selectedImagePath);
            _selectedImagePath = localFilePath;

            ProfileImageSource = ImageSource.FromFile(localFilePath);
            IsPictureChanged = true;
        }

        [RelayCommand]
        public async Task ConfirmProfilePicture()
        {
            var result = await _userClient.UpdateProfilePictureAsync(_selectedImagePath);

            if (!result.Success)
            {
                await Shell.Current.DisplayAlertAsync("Error", "Couldn't update profile picture", "OK");
                return;
            }
            User.PublicUser.ProfilePictureSource = _userClient.GetProfilePicture(User.PublicUser.ProfilePicture);
            IsPictureChanged = false;
        }

        [RelayCommand]
        public async Task CancelProfilePicture()
        {
            ImageCleaner.CleanupTempImage(_selectedImagePath);
            ProfileImageSource = User.PublicUser.ProfilePictureSource;
            IsPictureChanged = false;
        }

        [RelayCommand]
        public async Task Logout()
        {
            await _userClient.Logout();
            await Shell.Current.GoToAsync(nameof(LoginPage));
        }

        [RelayCommand]
        public async Task DeleteAccount()
        {
            bool confirm = await Shell.Current.DisplayAlertAsync("Confirm Delete",
                "Are you sure you want to delete your account? This action cannot be undone.", "Yes", "No");

            if (!confirm)
                return;

            try
            {
                var result = await _userClient.DeleteUserAsync(User.PublicUser.Id);
                if (!result.Success)
                {
                    await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                }

                _= Toast.Make($"{User.PublicUser.Username} was deleted", ToastDuration.Short).Show();
                await Shell.Current.GoToAsync(nameof(LoginPage));
                await _userClient.Logout();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"{ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task UpdatePassword()
        {
            try
            {
                var result = await _userClient.UpdatePasswordAsync(OldPassword, NewPassword);
                if (!result.Success)
                {
                    await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                    return;
                }

                _= Toast.Make("Password updated").Show();
                OldPassword = null;
                NewPassword = null;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"{ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task UpdateUsernameEmail()
        {
            if (string.IsNullOrWhiteSpace(NewUsername) && string.IsNullOrWhiteSpace(NewEmail))
            {
                return;
            }

            try
            {
                var result = await _userClient.UpdateUsernameEmailAsync(NewUsername, NewEmail);

                if (!result.Success)
                {
                    await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                    return;
                }

                _= Toast.Make("Username updated").Show();
                NewUsername = null;
                NewEmail = null;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
        
        public void OnAppearing()
        {
            NewUsername = null;
            NewEmail = null;
            NewPassword = null;
            OldPassword = null;

            ProfileImageSource = User.PublicUser.ProfilePictureSource;
        }

        public async Task OnDisappearingAsync()
        {
            await CancelProfilePicture();
        }
    }
}
