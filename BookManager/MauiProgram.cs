using BookManager.Authentication;
using BookManager.ApiClients;
using BookManager.ViewModels.Authentication;
using BookManager.ViewModels.Book;
using BookManager.ViewModels.Models;
using BookManager.Views.Authentication;
using BookManager.Views.Book;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;

namespace BookManager
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionToolkit()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif


            builder.Services.AddSingleton<UserVM>();

            builder.Services.AddSingleton<ITokenStore, TokenStore>();
            builder.Services.AddTransient<AuthMessageHandler>();

            void AddApiClient<T>() where T : class
            {
                builder.Services
                    .AddHttpClient<T>(client =>
                    {
                        client.BaseAddress = new Uri("http://10.0.2.2:5137");
                    })
                    .AddHttpMessageHandler<AuthMessageHandler>();
            }

            AddApiClient<UserClient>();
            AddApiClient<BookClient>();

            builder.Services.AddTransient<LoadingVM>();
            builder.Services.AddTransient<LoginVM>();
            builder.Services.AddTransient<SignUpVM>();

            builder.Services.AddSingleton<BookSearchVM>();
            builder.Services.AddTransient<BookFormVM>();

            builder.Services.AddTransient<LoadingPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<SignUpPage>();

            builder.Services.AddSingleton<BookSearchPage>();
            builder.Services.AddTransient<BookFormPage>();

            return builder.Build();
        }
    }
}
