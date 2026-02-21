using System.Windows.Input;

namespace BookManager.Views.Book;

public partial class GenreView : ContentView
{
    public static readonly BindableProperty TapCommandProperty =
        BindableProperty.Create(
            nameof(TapCommand),
            typeof(ICommand),
            typeof(GenreView));

    public ICommand TapCommand
    {
        get => (ICommand)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    public GenreView()
	{
		InitializeComponent();
	}
}