using BookManager.ApiClients;
using BookManager.Models.Book;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls.Platform.Compatibility;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookManager.ViewModels.Book
{
    public partial class BookFormVM : ObservableObject, IQueryAttributable
    {
        readonly BookClient _bookClient;
        string _selectedImagePath;

        [ObservableProperty]
        BookVM book = new();

        [ObservableProperty]
        string title;
        [ObservableProperty]
        string submitButtonText;

        [ObservableProperty]
        BookVM navigationBook;

        bool isEditMode;

        bool isImageChanged;

        public BookFormVM(BookClient bookClient)
        {
            _bookClient = bookClient;
            Title = "Create Book";
            SubmitButtonText = "Create";
        }

        [RelayCommand]
        private async Task PickCover()
        {
            var options = new MediaPickerOptions
            {
                Title = "Select a book image",
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

            Book.CoverSource = ImageSource.FromFile(localFilePath);

            if (isEditMode)
                isImageChanged = true;
        }

        [RelayCommand]
        public async Task Submit()
        {
            var validateResult = Validate();
            if (validateResult != null)
            {
                await Shell.Current.DisplayAlertAsync("Error", validateResult, "OK");
                return;
            }

            if (!isEditMode)
            {
                await CreateBook();
            }
            else
            {
               await UpdateBook();
            }
        }


        private async Task CreateBook()
        { 
            try
            {
                var result = await _bookClient.CreateBookAsync(Book, _selectedImagePath);

                if (result.Success)
                {
                    _ = Toast.Make($"{Book.Title} created").Show();
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"An unexpected error occurred: {ex.Message}", "OK");
            }
        }
        private async Task UpdateBook()
        {
            if (string.IsNullOrWhiteSpace(Book.Author.Name) || string.IsNullOrWhiteSpace(Book.ISBN) || string.IsNullOrWhiteSpace(Book.Title) )
            {
                await Shell.Current.DisplayAlertAsync("Missing fields", "Author, ISBN and title are required!", "OK");
                return;
            }

            try
            {
                var result = await _bookClient.UpdateBookAsync(Book, isImageChanged ? _selectedImagePath : null);
                if (!result.Success)
                {
                    await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                    return;
                }

                _ = Toast.Make($"{Book.Title} updated").Show();
                NavigationBook.CopyFrom(Book);
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"An unexpected error occurred: {ex.Message}", "OK");
            }
        }

        string? Validate()
        {
            if (string.IsNullOrWhiteSpace(Book.Title))
                return "Title is required.";

            if (string.IsNullOrWhiteSpace(Book.ISBN) || Book.ISBN.Length !=13)
                return "ISBN is required.";

            if (string.IsNullOrWhiteSpace(Book.Author.Name))
                return "Author's name is requeired";

            if (Book.TotalPages <= 0)
                return "The total number of pages is required";

            if (!isEditMode && _selectedImagePath == null)
                return "Image is required when creating a book.";

            return null;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue($"{nameof(NavigationBook)}", out var obj) && obj is BookVM book)
            {
                NavigationBook = book;
                Book.CopyFrom(book);
                Title = "Edit book";
                SubmitButtonText = "Save Changes";
                isEditMode = true;
            }
        }
    
    }
}
