using BookManager.ViewModels.Book;

namespace BookManager.Views.Book;

public partial class BookSearchPage : ContentPage
{
    BookSearchVM _vm;
	public BookSearchPage(BookSearchVM vm)
	{
		InitializeComponent();
		BindingContext = vm;
        _vm = vm;
	}

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await _vm.Load();
    }
}