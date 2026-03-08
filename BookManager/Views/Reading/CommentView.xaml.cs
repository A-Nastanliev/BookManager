using BookManager.Views.Settings;
using System.Windows.Input;

namespace BookManager.Views.Reading;

public partial class CommentView : ContentView
{
    public static readonly BindableProperty TapCommandProperty =
    BindableProperty.Create(
        nameof(TapCommand),
        typeof(ICommand),
        typeof(CommentView));

    public ICommand TapCommand
    {
        get => (ICommand)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    public CommentView()
	{
		InitializeComponent();
	}
}