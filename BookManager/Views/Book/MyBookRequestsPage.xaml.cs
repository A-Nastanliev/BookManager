using BookManager.ViewModels.Book;

namespace BookManager.Views.Book;

public partial class MyBookRequestsPage : ContentPage
{
	readonly MyBookRequestsVM _vm;

    bool _isBottomSheetOpen;
	public MyBookRequestsPage(MyBookRequestsVM vm)
	{
		InitializeComponent();
		_vm = vm;
		BindingContext = vm;
	}

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await _vm.Load();
    }

    private async void PlusButton_Clicked(object sender, EventArgs e)
    {
        await OpenBottomSheet();
    }

    protected override async void OnDisappearing()
    {
        await CloseBottomSheetAsync();
        base.OnDisappearing();
    }

    public async Task OpenBottomSheet()
    {
        if (_isBottomSheetOpen)
            return;

        Overlay.IsVisible = true;
        Overlay.InputTransparent = false;
        _isBottomSheetOpen = true;
        BottomSheetContent.Opacity = 0;
        BottomSheetContent.Margin = new Thickness(0, 0, 0, -40);
        await Task.WhenAll(BottomSheetContent.FadeToAsync(1, 200), BottomSheetContent.AnimateBottomMargin(-40, 0, 200));
    }

    private async Task CloseBottomSheetAsync()
    {
        if (!_isBottomSheetOpen)
            return;

        await Task.WhenAll(BottomSheetContent.FadeToAsync(0, 200), BottomSheetContent.AnimateBottomMargin(0, -40, 200));
        Overlay.InputTransparent = true;
        Overlay.IsVisible = false;
        _isBottomSheetOpen = false;
    }

    private async void OnOverlayTapped(object sender, EventArgs e)
    {
        await CloseBottomSheetAsync();
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {

    }
}