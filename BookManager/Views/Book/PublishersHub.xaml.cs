using BookManager.ViewModels.Book;

namespace BookManager.Views.Book;

public partial class PublishersHub : ContentView
{
    PublishersHubVM _vm;

	public PublishersHub()
	{
		InitializeComponent();
	}
    public void Inject(PublishersHubVM vm)
    {
        BindingContext = vm;
        _vm = vm;
    }
    public async void OnAppearing()
    {
        await _vm.Load();
    }
}