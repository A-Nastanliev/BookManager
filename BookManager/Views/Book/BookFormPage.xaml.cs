using BookManager.ViewModels.Book;

namespace BookManager.Views.Book;

public partial class BookFormPage : ContentPage
{
	BookFormVM _vm;
	public BookFormPage(BookFormVM vm)
	{
		InitializeComponent();
		BindingContext = vm;
		_vm = vm;
	}

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
		_vm.OnDissapearing();
    }
}