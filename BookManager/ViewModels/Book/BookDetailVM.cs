using BookManager.ApiClients;
using BookManager.Models.Book;
using BookManager.Views.Book;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Alerts;

namespace BookManager.ViewModels.Book
{
    public partial class BookDetailVM : PagedLoadingVM, IQueryAttributable
    {
        [ObservableProperty]
        BookVM book = new();

        readonly ReadingClient _readingClient;
        readonly BookClient _bookClient;

        public BookDetailVM(ReadingClient readingClient, BookClient bookClient)
        {
            _readingClient = readingClient;
            _bookClient = bookClient;
        }

        public override Task Load()
        {
            throw new NotImplementedException();
        }

        public override Task Refresh()
        {
            throw new NotImplementedException();
        }

        [RelayCommand]
        public async Task Edit()
        {
            await Shell.Current.GoToAsync(nameof(BookFormPage), new Dictionary<string, object> { [nameof(BookFormVM.Book)] = Book });
        }

        [RelayCommand]
        public async Task Delete() 
        {
            try
            {
                var result = await _bookClient.DeleteBookAsync(Book.Id);

                if (!result.Success)
                {
                    await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                    return;
                }

                _ = Toast.Make($"{Book.Title} deleted").Show();
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        [RelayCommand]
        public async Task GenreTapped()
        {
            if (Book.Genre.Id == 0)
                return;

            await Shell.Current.GoToAsync(nameof(BookAttributePage), new Dictionary<string, object> { [nameof(BookAttributeVM.Genre)] = Book.Genre });
        }

        [RelayCommand]
        public async Task AuthorTapped()
        {
            if (Book.Author.Id == 0)
                return;

            await Shell.Current.GoToAsync(nameof(BookAttributePage), new Dictionary<string, object> { [nameof(BookAttributeVM.Author)] = Book.Author });
        }

        [RelayCommand]
        public async Task PublisherTapped()
        {
            if(Book.Publisher.Id == 0)
                return;

            await Shell.Current.GoToAsync(nameof(BookAttributePage), new Dictionary<string, object> { [nameof(BookAttributeVM.Publisher)] = Book.Publisher });
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue($"{nameof(Book)}", out var obj) && obj is BookVM book)
            {
                Book.CopyFrom(book);
            }
        }
    }
}
