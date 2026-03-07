using BookManager.ViewModels.Reading;

namespace BookManager.Views.Reading;

public partial class UserBooksPage : ContentPage
{
    readonly UserBooksVM _vm;

    public UserBooksPage(UserBooksVM vm)
	{
		InitializeComponent();
        BindingContext = vm;
        _vm  = vm;
	}

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await _vm.Load();
    }
}