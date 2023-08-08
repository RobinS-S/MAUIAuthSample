using MauiXamlClient.Auth.Services;

namespace MauiXamlClient.Auth.Pages;

public partial class LoginPage : ContentPage
{
    private readonly AuthService authService;

    public LoginPage(AuthService authService)
    {
        InitializeComponent();
        this.authService = authService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await authService.LoginAsync();
        LoadingIndicator.IsVisible = false;
    }
}