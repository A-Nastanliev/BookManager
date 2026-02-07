namespace BookManager.Views;

public partial class VisibilityButton : ContentView
{
    public static readonly BindableProperty TargetEntryProperty =
     BindableProperty.Create(
         nameof(TargetEntry),
         typeof(Entry),
         typeof(VisibilityButton),
         propertyChanged: OnTargetEntryChanged);

    public Entry TargetEntry
    {
        get => (Entry)GetValue(TargetEntryProperty);
        set => SetValue(TargetEntryProperty, value);
    }

    public VisibilityButton()
    {
        InitializeComponent();
    }

    void OnClicked(object sender, EventArgs e)
    {
        if (TargetEntry == null)
            return;

        TargetEntry.IsPassword = !TargetEntry.IsPassword;
        UpdateIcon();
    }

    static void OnTargetEntryChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (VisibilityButton)bindable;
        control.UpdateIcon();
    }

    void UpdateIcon()
    {
        if (TargetEntry == null)
            return;

        Button.Source = TargetEntry.IsPassword ? "hide_password.png" : "show_password.png";
    }
}