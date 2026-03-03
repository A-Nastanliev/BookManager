using BookManager.ViewModels.Book;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace BookManager.Views.Book;

public partial class BookSearchPage : ContentPage
{
    bool _isScannerOpen;
    bool _isProcessingScan;
    CameraBarcodeReaderView? _scanner;
    BookSearchVM _vm;

	public BookSearchPage(BookSearchVM vm)
	{
		InitializeComponent();
		BindingContext = vm;
        _vm = vm;
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

        _scanner = new CameraBarcodeReaderView
        {
            Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormat.Ean13 | BarcodeFormat.UpcA | BarcodeFormat.Ean8,
                AutoRotate = true,
                Multiple = false,
            },
            CameraLocation = CameraLocation.Rear,
            IsDetecting = true,
            IsEnabled = true,
            HeightRequest = 300,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.End
        };

        _scanner.BarcodesDetected += barcodeReaderView_BarcodesDetected;

        ScannerHost.Children.Clear();
        ScannerHost.Children.Add(_scanner);
        Overlay.IsVisible = true;
        Overlay.InputTransparent = false;
        _isScannerOpen = true;

        ScannerPanel.Opacity = 0;
        ScannerPanel.Margin = new Thickness(0, 0, 0, -40);

        await Task.WhenAll(
            ScannerPanel.FadeToAsync(1, 200),
            ScannerPanel.TranslateToAsync(0, 0, 0),
            ScannerPanel.AnimateBottomMargin(-40, 0, 200));
    }

    private async Task CloseScannerAsync()
    {
        if (!_isScannerOpen)
            return;

        await Task.WhenAll(ScannerPanel.FadeToAsync(0, 200), ScannerPanel.AnimateBottomMargin(0, -40, 200));

        if (_scanner != null)
        {
            _scanner.IsDetecting = false;
            _scanner.BarcodesDetected -= barcodeReaderView_BarcodesDetected;
            _scanner.Handler?.DisconnectHandler();

            ScannerHost.Children.Clear();
            _scanner = null;
        }

        Overlay.InputTransparent = true;
        Overlay.IsVisible = false;

        _isScannerOpen = false;
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
                await _vm.SearchIsbnAsync(result.Value?.Trim());
            }
            finally
            {
                _isProcessingScan = false;
            }
        });
    }

    protected override async void OnDisappearing()
    {
        await CloseScannerAsync();
        base.OnDisappearing();
    }
}