using BookManager.ViewModels.Book;

namespace BookManager.Views.Book;

public partial class FormPage : ContentPage
{
	public FormPage(FormVM vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}