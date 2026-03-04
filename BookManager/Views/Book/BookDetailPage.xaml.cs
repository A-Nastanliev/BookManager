using BookManager.ViewModels.Book;

namespace BookManager.Views.Book;

public partial class BookDetailPage : ContentPage
{
	BookDetailVM _vm;
	public BookDetailPage(BookDetailVM vm)
	{
		InitializeComponent();
		_vm = vm;
		BindingContext = vm;
	}
}