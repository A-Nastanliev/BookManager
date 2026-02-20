using BookManager.ViewModels.Book;

namespace BookManager.Views.Book;

public partial class PublishingHubPage : ContentPage
{
    public static readonly BindableProperty SelectedSegmentIndexProperty =
    BindableProperty.Create(
        nameof(SelectedSegmentIndex),
        typeof(int),
        typeof(PublishingHubPage),
        0);

    public int SelectedSegmentIndex
    {
        get => (int)GetValue(SelectedSegmentIndexProperty);
        set => SetValue(SelectedSegmentIndexProperty, value);
    }
    public PublishingHubPage()
	{
		InitializeComponent();
	}
}