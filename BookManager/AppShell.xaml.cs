using BookManager.ViewModels.Authentication;
using BookManager.Views.Authentication;

namespace BookManager
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(SignUpPage), typeof(SignUpPage));
        }
    }
}
