using BookManager.ViewModels.Book;

namespace BookManager.Views.Book;

public partial class BookSearchPage : ContentPage
{
	public BookSearchPage(BookSearchVM vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}