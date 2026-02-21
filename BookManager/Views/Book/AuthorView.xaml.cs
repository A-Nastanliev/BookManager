using System.Windows.Input;

namespace BookManager.Views.Book;

public partial class AuthorView : ContentView
{
    public static readonly BindableProperty TapCommandProperty =
    BindableProperty.Create(
        nameof(TapCommand),
        typeof(ICommand),
        typeof(AuthorView));

    public ICommand TapCommand
    {
        get => (ICommand)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }
    public AuthorView()
	{
		InitializeComponent();
	}
}