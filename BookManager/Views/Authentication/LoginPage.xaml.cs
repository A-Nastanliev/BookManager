using BookManager.ViewModels.Authentication;

namespace BookManager.Views.Authentication;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginVM vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}