using MauiXamlClient.Auth.Services;

namespace MauiXamlClient.Auth.Pages;

public partial class LogoutPage : ContentPage
{
    private readonly AuthService authService;

    public LogoutPage(AuthService authService)
    {
        InitializeComponent();
        this.authService = authService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (authService.IsLoggedIn) await authService.LogoutAsync();
        LoadingIndicator.IsVisible = false;
    }
}