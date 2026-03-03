using BookManager.ViewModels.Settings;

namespace BookManager.Views.Settings;

public partial class RestrictionsPage : ContentPage
{
	RestrictionsVM _vm;

	public RestrictionsPage(RestrictionsVM vm)
	{
		InitializeComponent();
		_vm = vm;
		BindingContext = vm;
	}

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await _vm.OnAppearingAsync();
    }

}