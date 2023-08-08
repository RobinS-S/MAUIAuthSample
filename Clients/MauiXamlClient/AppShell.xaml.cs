using MauiXamlClient.Auth;
using MauiXamlClient.Auth.Pages;

namespace MauiXamlClient;

public partial class AppShell : Shell
{
    public AppShell(AuthViewModel authViewModel)
    {
        InitializeComponent();
        // Sets the binding context of the page to the view model so you can use bindings.
        BindingContext = authViewModel;

        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(LogoutPage), typeof(LogoutPage));
    }
}