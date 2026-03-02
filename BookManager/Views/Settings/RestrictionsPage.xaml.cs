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
        Shell.SetTabBarIsVisible(this, true);
        base.OnAppearing();
        await _vm.OnAppearingAsync();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        Shell.SetTabBarIsVisible(this, false);
    }
}