using BookManager.ViewModels.Reading;

namespace BookManager.Views.Reading;

public partial class WishlistedBooksPage : ContentPage
{
    readonly UserBooksVM _vm;

    public WishlistedBooksPage(UserBooksVM vm)
    {
        InitializeComponent();
        BindingContext = vm;
        vm.Status = UserBookStatus.Wishlisted;
        vm.EmptyViewText = "Your wishlist is empty";
        BooksView.BindingContext = _vm;
        _vm = vm;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await _vm.Load();
    }
}