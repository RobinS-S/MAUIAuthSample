using CommunityToolkit.Maui;
using MauiXamlClient.Auth;
using MauiXamlClient.Auth.Pages;
using MauiXamlClient.Auth.Services;
using MauiXamlClient.Pages.ForecastOverview;
using MauiXamlClient.Pages.ForecastToday;
using Microsoft.Extensions.Logging;

namespace MauiXamlClient;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
#if DEBUG
            .UseMauiCommunityToolkit()
#else
            .UseMauiCommunityToolkit(options =>
        {
	        options.SetShouldSuppressExceptionsInAnimations(true);
	        options.SetShouldSuppressExceptionsInBehaviors(true);
	        options.SetShouldSuppressExceptionsInConverters(true);
        })
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register the services as singleton
        builder.Services.AddSingleton(new HttpClient());
        builder.Services.AddSingleton<AuthenticatedHttpClientService>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<AppShell>();

        // Register the pages
        builder.Services.AddTransient<ForecastTodayViewModel>();
        builder.Services.AddTransient<ForecastTodayPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<LogoutPage>();
        builder.Services.AddTransient<ForecastOverviewViewModel>();
        builder.Services.AddTransient<ForecastOverviewPage>();
        builder.Services.AddTransient<AuthViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}