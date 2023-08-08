using MauiXamlClient.Auth.Services;
using MauiXamlClient.Pages.ForecastOverview;

namespace MauiXamlClient.Pages.ForecastToday;

public partial class ForecastTodayPage : ContentPage
{
    private readonly AuthService _authService;
    private readonly ForecastTodayViewModel _viewModel;
    private bool _testedLogin;

    public ForecastTodayPage(AuthService authService, ForecastTodayViewModel viewModel)
    {
        InitializeComponent();
        _authService = authService;
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    private async void OnForecastOverviewClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(ForecastOverviewPage)}");
    }

    protected override async void OnAppearing()
    {
        if (!_testedLogin)
        {
            // We run this once upon start up to load possible existing tokens on the startup page.
            await _authService.LoadCredentialsAsync();
            _testedLogin = true;
        }

        await _viewModel.LoadDataCommand.ExecuteAsync(null);
    }
}