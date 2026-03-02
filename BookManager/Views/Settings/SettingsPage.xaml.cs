using BookManager.ViewModels.Settings;
using BookManager.Views.Book;
using System.Diagnostics;

namespace BookManager.Views.Settings;

public partial class SettingsPage : ContentPage
{
	readonly SettingsVM _vm;
	public SettingsPage(SettingsVM vm)
	{		
		InitializeComponent();
		_vm = vm;
		BindingContext = _vm;
        RestrictionsGesture.Command = new Command(async () =>
        {
            await Shell.Current.GoToAsync(nameof(RestrictionsPage));
        });
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm?.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
       _= Task.Run(async () =>
      {
          try
          {
              await _vm.OnDisappearingAsync();
          }
          catch (Exception ex)
          {
              Debug.WriteLine($"Error in disappearing cleanup: {ex}");
          }
      });
    }
}