using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiXamlClient.Auth.Pages;
using MauiXamlClient.Auth.Services;

namespace MauiXamlClient.Auth;

public partial class AuthViewModel : ObservableObject
{
    private readonly AuthService _authService;

    [ObservableProperty]
    private bool _loggedIn;

    public AuthViewModel(AuthService authService)
    {
        _authService = authService;
        LoggedIn = _authService.IsLoggedIn;
        _authService.AuthChanged += _authService_AuthChanged;
    }

    private void _authService_AuthChanged(object sender, bool e)
    {
        SetLoggedInCommand.Execute(e);
    }

    [RelayCommand]
    private async Task SetLoggedIn(bool loggedIn)
    {
        if (LoggedIn != loggedIn)
        {
            // The reason why we handle the navigation here is if we try to hide the LoginPage or the LogoutPage when we are currently on it, AppShell will throw an exception
            // So we make sure we're on neither of those pages before setting the value.
            // It will navigate to the default page after login or the default page for non-logged in users.
            if (Shell.Current.CurrentPage is LoginPage or LogoutPage)
            {
                await Shell.Current.GoToAsync("//" + (loggedIn
                    ? ApiConfig.Instance.AppShellDefaultPageAfterLogin
                    : ApiConfig.Instance.AppShellDefaultPageAnonymous));
            }

            LoggedIn = loggedIn;
        }
    }
}