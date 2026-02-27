using BookManager.ApiClients;
using BookManager.Authentication;
using BookManager.ViewModels.Authentication;
using BookManager.ViewModels.Book;
using BookManager.ViewModels.Models;
using BookManager.ViewModels.Settings;
using BookManager.Views.Authentication;
using BookManager.Views.Book;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;
using ZXing.Net.Maui.Controls;
using BookManager.Views.Settings;  

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
                .UseBarcodeReader()
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
                        client.BaseAddress = new Uri("http://192.168.100.219:5137");
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
            builder.Services.AddSingleton<GenresHubVM>();
            builder.Services.AddSingleton<PublishersHubVM>();
            builder.Services.AddSingleton<AuthorsHubVM>();
            builder.Services.AddTransient<FormVM>();

            builder.Services.AddSingleton<SettingsVM>();

            builder.Services.AddTransient<LoadingPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<SignUpPage>();

            builder.Services.AddSingleton<BookSearchPage>();
            builder.Services.AddTransient<BookFormPage>();
            builder.Services.AddSingleton<PublishingHubPage>(); 
            builder.Services.AddTransient<FormPage>();

            builder.Services.AddSingleton<SettingsPage>();

            return builder.Build();
        }
    }
}
