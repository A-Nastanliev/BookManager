using BookManager.Models.User;
using Microsoft.Extensions.DependencyInjection;

namespace BookManager
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var user = activationState?.Context?.Services?.GetService<UserVM>();

            if (user != null)
                Resources["User"] = user;

            return new Window(new AppShell());
        }
    }
}