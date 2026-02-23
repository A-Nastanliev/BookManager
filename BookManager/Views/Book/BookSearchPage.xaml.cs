using BookManager.ViewModels.Book;
using ZXing.Net.Maui;

namespace BookManager.Views.Book;

public partial class BookSearchPage : ContentPage
{
    bool _isScannerOpen;
    bool _isProcessingScan;

    BookSearchVM _vm;
	public BookSearchPage(BookSearchVM vm)
	{
		InitializeComponent();
		BindingContext = vm;
        _vm = vm;
        barcodeReaderView.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.Ean13,
            AutoRotate = true,
            Multiple = false,
        };
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await _vm.Load();
    }

    private async void ScanButtonBase_Clicked(object sender, EventArgs e)
    {
        if (_isScannerOpen)
            return;

        barcodeReaderView.IsEnabled = true;
        _isScannerOpen = true;
        Overlay.IsVisible = true;
        barcodeReaderView.IsDetecting = true;
        Overlay.InputTransparent = false;
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
        _isScannerOpen = false;
        barcodeReaderView.IsEnabled = false;
    }

    private async void OnOverlayTapped(object sender, EventArgs e)
    {
        await CloseScannerAsync();
    }

    private void barcodeReaderView_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_isProcessingScan)
            return;

        var result = e.Results?.FirstOrDefault();
        if (result == null)
            return;

        _isProcessingScan = true;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await CloseScannerAsync();

                if (_vm is BookSearchVM vm)
                {
                    await vm.SearchIsbnAsync(result.Value?.Trim());
                }
            }
            finally
            {
                _isProcessingScan = false;
            }
        });
    }
}