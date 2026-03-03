using BookManager.ApiClients;
using BookManager.Models.Book;
using BookManager.Views.Book;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookManager.ViewModels.Book
{
    public partial class FormVM : ObservableObject, IQueryAttributable
    {
        [ObservableProperty]
        AuthorVM navigationAuthor;

        [ObservableProperty]
        AuthorVM author = new();

        [ObservableProperty]
        PublisherVM navigationPublisher;

        [ObservableProperty]
        PublisherVM publisher = new();

        [ObservableProperty]
        GenreVM navigationGenre;

        [ObservableProperty]
        GenreVM genre = new();

        [ObservableProperty]
        string entityType;

        [ObservableProperty]
        string title;
        [ObservableProperty]
        string submitButtonText;

        bool isEditMode;

        BookClient _bookClient;

        public FormVM(BookClient bookClient)
        {
            _bookClient = bookClient;
        }

        [RelayCommand]
        private async Task Submit()
        {
            if (EntityType == nameof(AuthorVM))
                await HandleAuthorAsync();
            else if (EntityType == nameof(PublisherVM))
                await HandlePublisherAsync();
            else if (EntityType == nameof(GenreVM))
                await HandleGenreAsync();
        }

        private async Task HandleAuthorAsync()
        {
            if (string.IsNullOrWhiteSpace(Author.Name))
            {
                await Shell.Current.DisplayAlertAsync("Validation", "Name is required.", "OK");
                return;
            }

            try
            {
                if (isEditMode)
                {
                    var result = await _bookClient.UpdateAuthorAsync(Author);

                    if (!result.Success)
                    {
                        await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                        return;
                    }

                    NavigationAuthor.CopyFrom(Author);
                    _ = Toast.Make($"{Author.Name} updated").Show();
                }
                else
                {
                    var result = await _bookClient.CreateAuthorAsync(Author);

                    if (!result.Success)
                    {
                        await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                        return;
                    }

                    _ = Toast.Make($"{Author.Name} created").Show();
                }

                await Shell.Current.GoToAsync("..", new Dictionary<string, object> 
                { 
                    [nameof(BookAttributeVM.NavigationAuthor)] = Author,
                    ["Updated"] = true
                });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        private async Task HandlePublisherAsync()
        {
            if (string.IsNullOrWhiteSpace(Publisher.Name))
            {
                await Shell.Current.DisplayAlertAsync("Validation", "Name is required.", "OK");
                return;
            }

            try
            {
                if (isEditMode)
                {
                    var result = await _bookClient.UpdatePublisherAsync(Publisher);

                    if (!result.Success)
                    {
                        await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                        return;
                    }

                    NavigationPublisher.CopyFrom(Publisher);
                    _ = Toast.Make($"{Publisher.Name} updated").Show();
                }
                else
                {
                    var result = await _bookClient.CreatePublisherAsync(Publisher);

                    if (!result.Success)
                    {
                        await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                        return;
                    }

                    _ = Toast.Make($"{Publisher.Name} created").Show();
                }

                await Shell.Current.GoToAsync("..", new Dictionary<string, object>
                {
                    [nameof(BookAttributeVM.NavigationPublisher)] = Publisher,
                    ["Updated"] = true
                });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        private async Task HandleGenreAsync()
        {
            if (string.IsNullOrWhiteSpace(Genre.Name))
            {
                await Shell.Current.DisplayAlertAsync("Validation", "Name is required.", "OK");
                return;
            }

            try
            {
                if (isEditMode)
                {
                    var result = await _bookClient.UpdateGenreAsync(Genre);

                    if (!result.Success)
                    {
                        await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                        return;
                    }

                    NavigationGenre.CopyFrom(Genre);
                    _ = Toast.Make($"{Genre.Name} updated").Show();
                }
                else
                {
                    var result = await _bookClient.CreateGenreAsync(Genre);

                    if (!result.Success)
                    {
                        await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                        return;
                    }

                    _ = Toast.Make($"{Genre.Name} created").Show();
                }

                await Shell.Current.GoToAsync("..", new Dictionary<string, object> 
                { 
                    [nameof(BookAttributeVM.NavigationGenre)] = Genre,
                    ["Updated"] = true
                });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue($"{nameof(NavigationAuthor)}", out var obj) && obj is AuthorVM author)
            {
                EntityType = nameof(AuthorVM);
                NavigationAuthor = author;
                Author.CopyFrom(author);
                Title = "Edit Author";
                SubmitButtonText = "Save Changes";
                isEditMode = true;
            }
            else if (query.TryGetValue($"{nameof(NavigationPublisher)}", out var obj2) && obj2 is PublisherVM publisher)
            {
                EntityType = nameof(PublisherVM);
                NavigationPublisher = publisher;
                Publisher.CopyFrom(publisher);
                Title = "Edit Publisher";
                SubmitButtonText = "Save Changes";
                isEditMode = true;
            }
            else if (query.TryGetValue($"{nameof(NavigationGenre)}", out var obj3) && obj3 is GenreVM genre)
            {
                EntityType = nameof(GenreVM);
                NavigationGenre = genre;
                Genre.CopyFrom(genre);
                Title = "Edit Genre";
                SubmitButtonText = "Save Changes";
                isEditMode = true;
            }
            else if (query.TryGetValue($"{nameof(EntityType)}", out var typeObj) && typeObj is string typeName)
            {
                EntityType = typeName;
                Title = $"Create {typeName.Replace("VM", "")}";
                SubmitButtonText = "Create";
            }
        }
    }
}
