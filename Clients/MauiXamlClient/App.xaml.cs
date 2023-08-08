using MauiXamlClient.Auth;

namespace MauiXamlClient;

public partial class App : Application
{
    public App(AuthViewModel authViewModel)
    {
        InitializeComponent();

        // We need to pass the AuthViewModel to AppShell.
        MainPage = new AppShell(authViewModel);
    }
}