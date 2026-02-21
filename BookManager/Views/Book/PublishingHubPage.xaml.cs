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
    public PublishingHubPage(GenresHubVM genresHubVM, PublishersHubVM publishersHubVM, AuthorsHubVM authorsHubVM)
	{
		InitializeComponent();
        GenresView.Inject(genresHubVM);
        PublishersView.Inject(publishersHubVM);
        AuthorsView.Inject(authorsHubVM);
        SelectedSegmentIndex = 0;
	}

    private void SegmentControl_SelectionChanged(object sender, Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs e)
    {
        AuthorsView.IsVisible = SelectedSegmentIndex == 0;
        PublishersView.IsVisible = SelectedSegmentIndex == 1;
        GenresView.IsVisible = SelectedSegmentIndex == 2;

        switch (SelectedSegmentIndex)
        {
            case 0: AuthorsView.OnAppearing(); break;
            case 1: PublishersView.OnAppearing(); break;
            case 2: GenresView.OnAppearing(); break;
        }
    }
}