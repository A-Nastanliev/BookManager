using BookManager.ViewModels.Reading;

namespace BookManager.Views.Reading;

public partial class FinishedBooksPage : ContentPage
{
    readonly UserBooksVM _vm;
	public FinishedBooksPage(UserBooksVM vm)
	{
        InitializeComponent();
        BindingContext = vm;
        vm.Status = UserBookStatus.Finished;
        vm.EmptyViewText = "Your haven't finished a single book";
        BooksView.BindingContext = _vm;
        _vm = vm;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await _vm.Load();
    }
}