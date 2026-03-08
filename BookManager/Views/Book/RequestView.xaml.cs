using BookManager.Views.Reading;
using System.Windows.Input;

namespace BookManager.Views.Book;

public partial class RequestView : ContentView
{
    public static readonly BindableProperty TapCommandProperty =
    BindableProperty.Create(
        nameof(TapCommand),
        typeof(ICommand),
        typeof(RequestView));

    public ICommand TapCommand
    {
        get => (ICommand)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }
    public RequestView()
	{
		InitializeComponent();
	}
}