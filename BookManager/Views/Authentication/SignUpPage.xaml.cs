using BookManager.ViewModels.Authentication;

namespace BookManager.Views.Authentication;

public partial class SignUpPage : ContentPage
{
	public SignUpPage(SignUpVM signUpVM )
	{
		InitializeComponent();
		BindingContext = signUpVM;
	}
}