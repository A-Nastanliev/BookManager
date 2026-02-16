using BookManager.ViewModels;
using BookManager.ViewModels.Authentication;

namespace BookManager.Views.Authentication;

public partial class SignUpPage : ContentPage
{
    ITemporaryImageCleaner _vm;
	public SignUpPage(SignUpVM signUpVM )
	{
		InitializeComponent();
		BindingContext = signUpVM;
        _vm = signUpVM;
	}
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.CleanupTempImage();
    }
}