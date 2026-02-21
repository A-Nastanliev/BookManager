using System.Windows.Input;

namespace BookManager.Views.Book;

public partial class PublisherView : ContentView
{
    public static readonly BindableProperty TapCommandProperty =
        BindableProperty.Create(
            nameof(TapCommand),
            typeof(ICommand),
            typeof(PublisherView));

    public ICommand TapCommand
    {
        get => (ICommand)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }
    public PublisherView()
	{
		InitializeComponent();
	}
}