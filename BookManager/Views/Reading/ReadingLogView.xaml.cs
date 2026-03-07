using BookManager.Views.Settings;
using System.Windows.Input;

namespace BookManager.Views.Reading;

public partial class ReadingLogView : ContentView
{
    public static readonly BindableProperty DeleteCommandProperty =
    BindableProperty.Create(
        nameof(DeleteCommand),
        typeof(ICommand),
        typeof(ReadingLogView));

    public ICommand DeleteCommand
    {
        get => (ICommand)GetValue(DeleteCommandProperty);
        set => SetValue(DeleteCommandProperty, value);
    }
    public ReadingLogView()
	{
		InitializeComponent();
	}
}