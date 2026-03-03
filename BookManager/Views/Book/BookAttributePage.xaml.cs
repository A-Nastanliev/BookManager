using BookManager.ViewModels.Book;

namespace BookManager.Views.Book;

public partial class BookAttributePage : ContentPage
{
	readonly BookAttributeVM _vm;
	public BookAttributePage(BookAttributeVM vm)
	{
		InitializeComponent();
        BindingContext = vm;
		_vm = vm;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
		await _vm.Load();
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is Label label)
        {
            var url = label.Text;
            if (!string.IsNullOrWhiteSpace(url))
            {
                await Launcher.Default.OpenAsync(url);
            }
        }
    }
}