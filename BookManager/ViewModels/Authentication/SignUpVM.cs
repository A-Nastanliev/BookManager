using BookManager.Services;
using BookManager.Views.Authentication;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

        private FileResult selectedImage;

        private ApiService _apiService;

        public SignUpVM(ApiService apiService) 
        {
            _apiService = apiService;
        }


        [RelayCommand]
        private async Task PickProfilePicture()
        {
            var result = await MediaPicker.PickPhotoAsync();

            if (result != null)
            {
                selectedImage = result;
                ProfileImage = ImageSource.FromFile(result.FullPath);
            }
        }

        [RelayCommand]
        private async Task SignUp()
        {
            if (string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(EmailAddress) ||
                string.IsNullOrWhiteSpace(Password))
            {
                await Shell.Current.DisplayAlertAsync( "Error", "All fields are required", "OK");
                return;
            }

            if (selectedImage == null)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Error",
                    "Profile picture is required",
                    "OK");
                return;
            }

            try
            {
                using var content = new MultipartFormDataContent();

                content.Add(new StringContent(Username), "Username");
                content.Add(new StringContent(EmailAddress), "EmailAddress");
                content.Add(new StringContent(Password), "Password");

                if (selectedImage != null)
                {
                    var stream = await selectedImage.OpenReadAsync();
                    var imageContent = new StreamContent(stream);
                    imageContent.Headers.ContentType =
                        new MediaTypeHeaderValue("image/jpeg");

                    content.Add(
                        imageContent,
                        "ProfilePicture",
                        selectedImage.FileName);
                }

                var response = await _apiService.SignUpAsync(content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await Shell.Current.DisplayAlertAsync("Sign up failed", error, "OK");
                    return;
                }

                await Shell.Current.DisplayAlertAsync("Success", "Account created successfully!", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync( "Error", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task GoToLogin()
        {
            await Shell.Current.GoToAsync(nameof(LoginPage));
        }
    }
}
