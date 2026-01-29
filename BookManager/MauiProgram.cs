using BookManager.Services;
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

            builder.Services.AddSingleton(sp =>
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri("http://10.0.2.2:5137")
                };
                return client;
            });

            builder.Services.AddSingleton<ApiService>();

            builder.Services.AddSingleton<UserVM>();

            builder.Services.AddSingleton<LoadingVM>();
            builder.Services.AddSingleton<LoginVM>();
            builder.Services.AddSingleton<SignUpVM>();

            builder.Services.AddSingleton<BookSearchVM>();

            builder.Services.AddSingleton<LoadingPage>();
            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<SignUpPage>();

            builder.Services.AddSingleton<BookSearchPage>();

            return builder.Build();
        }
    }
}
