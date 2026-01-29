using BookManager.ViewModels.Authentication;

namespace BookManager.Views.Authentication;

public partial class LoadingPage : ContentPage
{
    private readonly LoadingVM _vm;
    
    public LoadingPage(LoadingVM loadingVM)
	{
		InitializeComponent();
		BindingContext = loadingVM;
        _vm = loadingVM;
	}

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await _vm.InitializeAsync();
    }
}