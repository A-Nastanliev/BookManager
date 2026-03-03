using BookManager.ApiClients;
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
            ImageManager.CleanupAllTempImages();

            var userClient = activationState?.Context?.Services?.GetService<UserClient>();
            if (userClient != null)
            {
                ApiErrorParser.Initialize(userClient.Logout);
            }

            var user = activationState?.Context?.Services?.GetService<UserVM>();

            if (user != null)
                Resources["User"] = user;

            return new Window(new AppShell());
        }
    }
}