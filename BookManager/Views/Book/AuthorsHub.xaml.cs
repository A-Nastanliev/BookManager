using BookManager.ViewModels.Book;

namespace BookManager.Views.Book;

public partial class AuthorsHub : ContentView
{
    AuthorsHubVM _vm;

	public AuthorsHub()
	{
		InitializeComponent();
	}

    public void Inject(AuthorsHubVM vm)
    {
        BindingContext = vm;
        _vm = vm;
    }

    public async void OnAppearing()
    {
        await _vm.Load();
    }
}