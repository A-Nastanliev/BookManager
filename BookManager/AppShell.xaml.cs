using BookManager.ViewModels.Authentication;
using BookManager.Views.Authentication;
using BookManager.Views.Book;
using BookManager.Views.Settings;

namespace BookManager
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(BookFormPage), typeof(BookFormPage));
            Routing.RegisterRoute(nameof(FormPage), typeof(FormPage));
            Routing.RegisterRoute(nameof(RestrictionsPage), typeof(RestrictionsPage));
            Routing.RegisterRoute(nameof(BookAttributePage), typeof(BookAttributePage));
        }
    }
}
