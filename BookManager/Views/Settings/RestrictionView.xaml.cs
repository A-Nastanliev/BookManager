using System.Windows.Input;

namespace BookManager.Views.Settings;

public partial class RestrictionView : ContentView
{
    public static readonly BindableProperty TapCommandProperty =
        BindableProperty.Create(
            nameof(TapCommand),
            typeof(ICommand),
            typeof(RestrictionView));

    public ICommand TapCommand
    {
        get => (ICommand)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    public RestrictionView()
	{
		InitializeComponent();
	}
}