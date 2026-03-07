using BookManager.ApiClients;
using BookManager.Models.Book;
using BookManager.Views.Book;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookManager.ViewModels.Book
{
    public partial class FormVM : ObservableObject, IQueryAttributable
    {

        [ObservableProperty]
        AuthorVM author = new();

        [ObservableProperty]
        PublisherVM publisher = new();

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
            switch (EntityType)
            {
                case nameof(AuthorVM):
                    await HandleAuthorAsync();
                    break;
                case nameof(PublisherVM):
                    await HandlePublisherAsync();
                    break;
                case nameof(GenreVM):
                    await HandleGenreAsync();
                    break;
            }
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

                    _ = Toast.Make($"{Author.Name} updated").Show();
                    WeakReferenceMessenger.Default.Send(new ValueChangedMessage<AuthorVM>(Author), Messages.AuthorUpdated);
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
                    WeakReferenceMessenger.Default.Send(new ValueChangedMessage<AuthorVM>(Author), Messages.AuthorCreated);
                }

                await Shell.Current.GoToAsync("..", new Dictionary<string, object> 
                { 
                    [nameof(BookAttributeVM.Author)] = Author,
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

                    _ = Toast.Make($"{Publisher.Name} updated").Show();
                    WeakReferenceMessenger.Default.Send(new ValueChangedMessage<PublisherVM>(Publisher), Messages.PublisherUpdated);
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
                    WeakReferenceMessenger.Default.Send(new ValueChangedMessage<PublisherVM>(Publisher), Messages.PublisherCreated);
                }

                await Shell.Current.GoToAsync("..", new Dictionary<string, object>
                {
                    [nameof(BookAttributeVM.Publisher)] = Publisher,
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

                    _ = Toast.Make($"{Genre.Name} updated").Show();
                    WeakReferenceMessenger.Default.Send(new ValueChangedMessage<GenreVM>(Genre), Messages.GenreUpdated);
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
                    WeakReferenceMessenger.Default.Send(new ValueChangedMessage<GenreVM>(Genre), Messages.GenreCreated);
                }

                await Shell.Current.GoToAsync("..", new Dictionary<string, object> 
                { 
                    [nameof(BookAttributeVM.Genre)] = Genre,
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
            if (query.TryGetValue($"{nameof(Author)}", out var obj) && obj is AuthorVM author)
            {
                EntityType = nameof(AuthorVM);
                Author.CopyFrom(author);
                Title = "Edit Author";
                SubmitButtonText = "Save Changes";
                isEditMode = true;
            }
            else if (query.TryGetValue($"{nameof(Publisher)}", out var obj2) && obj2 is PublisherVM publisher)
            {
                EntityType = nameof(PublisherVM);
                Publisher.CopyFrom(publisher);
                Title = "Edit Publisher";
                SubmitButtonText = "Save Changes";
                isEditMode = true;
            }
            else if (query.TryGetValue($"{nameof(Genre)}", out var obj3) && obj3 is GenreVM genre)
            {
                EntityType = nameof(GenreVM);
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
