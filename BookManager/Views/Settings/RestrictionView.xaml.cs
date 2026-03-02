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

    public static readonly BindableProperty SelectedIdProperty =
          BindableProperty.Create(
              nameof(SelectedId),
              typeof(int),
              typeof(RestrictionView),
              0);

    public int SelectedId
    {
        get => (int)GetValue(SelectedIdProperty);
        set => SetValue(SelectedIdProperty, value);
    }


    public RestrictionView()
	{
		InitializeComponent();
	}
}