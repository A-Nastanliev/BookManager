using BookManager.ViewModels.Book;
using ZXing.Net.Maui;

namespace BookManager.Views.Book;

public partial class BookFormPage : ContentPage
{
	BookFormVM _vm;
    bool _isProcessingScan;
    bool _isScannerOpen;

    public BookFormPage(BookFormVM vm)
	{
		InitializeComponent();
		BindingContext = vm;
		_vm = vm;
	}

    protected override async void OnDisappearing()
    {
        await CloseScannerAsync();
        Shell.SetTabBarIsVisible(this, true);
        barcodeReaderView.Handler?.DisconnectHandler();
        base.OnDisappearing();
		_vm.OnDissapearing();
    }
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        Shell.SetTabBarIsVisible(this, false);
    }

    private void barcodeReaderView_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_isProcessingScan)
            return;

        var result = e.Results?.FirstOrDefault();
        if (result == null)
            return;

        _isProcessingScan = true;

        _vm.Book.ISBN = result.Value.Trim();
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await CloseScannerAsync();
            }
            finally
            {
                _isProcessingScan = false;
            }
        });
    }

    private async void ScanButtonBase_Clicked(object sender, EventArgs e)
    {
        if (_isScannerOpen)
            return;

        barcodeReaderView.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.Ean13 | BarcodeFormat.UpcA | BarcodeFormat.Ean8,
            AutoRotate = true,
            Multiple = false,
        };
        Overlay.IsVisible = true;
        Overlay.InputTransparent = false;
        _isScannerOpen = true;
        barcodeReaderView.CameraLocation = CameraLocation.Front;
        barcodeReaderView.CameraLocation = CameraLocation.Rear;
        barcodeReaderView.IsEnabled = true;
        barcodeReaderView.IsDetecting = true;
        await ScannerPanel.TranslateToAsync(0, 0, 250, Easing.SinOut);
    }


    private async Task CloseScannerAsync()
    {
        if (!_isScannerOpen)
            return;

        await ScannerPanel.TranslateToAsync(0, 300, 250, Easing.SinIn);
        Overlay.InputTransparent = true;
        Overlay.IsVisible = false;
        barcodeReaderView.IsDetecting = false;
        barcodeReaderView.IsEnabled = false;
        _isScannerOpen = false;
    }

    private async void OnOverlayTapped(object sender, EventArgs e)
    {
        await CloseScannerAsync();
    }
}