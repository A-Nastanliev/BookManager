using BookManager.ViewModels.Authentication;
using BookManager.Views.Authentication;
using BookManager.Views.Book;

namespace BookManager
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(SignUpPage), typeof(SignUpPage));
            Routing.RegisterRoute(nameof(BookFormPage), typeof(BookFormPage));
            Routing.RegisterRoute(nameof(FormPage), typeof(FormPage));
        }
    }
}
