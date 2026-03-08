using BookManager.ViewModels.Reading;

namespace BookManager.Views.Reading;

public partial class CurrentlyReadingBooksPage : ContentPage
{
    readonly UserBooksVM _vm;

    public CurrentlyReadingBooksPage(UserBooksVM vm)
    {
        InitializeComponent();
        BindingContext = vm;
        vm.Status = UserBookStatus.Reading;
        vm.EmptyViewText = "You aren't reading any books currently";
        BooksView.BindingContext = _vm;
        _vm = vm;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await _vm.Load();
    }
}