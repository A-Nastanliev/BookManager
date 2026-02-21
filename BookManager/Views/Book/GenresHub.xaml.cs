using BookManager.ViewModels.Book;

namespace BookManager.Views.Book;

public partial class GenresHub : ContentView
{
	GenresHubVM _vm;

	public GenresHub()
	{
		InitializeComponent();
	}

	public void Inject(GenresHubVM vm)
	{
		BindingContext = vm;
		_vm = vm;
	}

    public async void OnAppearing()
    {
        await _vm.Load();
    }

}