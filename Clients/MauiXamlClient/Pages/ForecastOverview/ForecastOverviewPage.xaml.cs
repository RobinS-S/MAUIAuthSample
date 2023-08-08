using MauiXamlClient.Auth.Services;

namespace MauiXamlClient.Pages.ForecastOverview;

public partial class ForecastOverviewPage : ContentPage
{
    private readonly AuthService _authService;
    private readonly ForecastOverviewViewModel _viewModel;

    public ForecastOverviewPage(AuthService authService, ForecastOverviewViewModel viewModel)
    {
        InitializeComponent();
        _authService = authService;
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        // To use a protected page, add this code BEFORE any API calls in your view model or in OnAppearing if you're not using a view model:
        // If you're not logged in, you'll be redirected to log in.
        if (!_authService.IsLoggedIn)
        {
            Dispatcher.Dispatch(async () => await _authService.RedirectToLogin());
            return;
        }
        
        // Now you can run API requests
        await _viewModel.LoadDataCommand.ExecuteAsync(null);
    }
}