using BookManager.ViewModels.Book;

namespace BookManager.Views.Book;

public partial class BookDetailPage : ContentPage
{
	BookDetailVM _vm;
    bool _isCommentSheetOpen;
    public BookDetailPage(BookDetailVM vm)
	{
		InitializeComponent();
		_vm = vm;
		BindingContext = vm;
        _vm.OnSelectComment = OpenCommentSheet;
        _vm.OnDeselectComment = CloseCommentSheetAsync;
    }

    protected override async void OnDisappearing()
    {
        await CloseCommentSheetAsync();
        base.OnDisappearing();
    }

    public async Task OpenCommentSheet()
    {
        if (_isCommentSheetOpen)
            return;

        Overlay.IsVisible = true;
        Overlay.InputTransparent = false;
        _isCommentSheetOpen = true;
        CommentSheetContent.Opacity = 0;
        CommentSheetContent.Margin = new Thickness(0, 0, 0, -40);
        await Task.WhenAll(CommentSheetContent.FadeToAsync(1, 200), CommentSheetContent.AnimateBottomMargin(-40, 0, 200));
    }

    private async Task CloseCommentSheetAsync()
    {
        if (!_isCommentSheetOpen)
            return;

        await Task.WhenAll(CommentSheetContent.FadeToAsync(0, 200), CommentSheetContent.AnimateBottomMargin(0, -40, 200));
        Overlay.InputTransparent = true;
        Overlay.IsVisible = false;
        _isCommentSheetOpen = false;
    }

    private async void OnOverlayTapped(object sender, EventArgs e)
    {
        await CloseCommentSheetAsync();
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {

    }
}